// Uncomment to add a (heavy) check of buffer content (vertex and index buffer only for the moment)
//#define DEBUG_BUFFERS 
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;

namespace PetitMoteur3D.DebugGui
{
    /// <summary>
    /// ImGui code for DirectX 11 Backend
    /// </summary>
    /// <remarks>
    /// Adapted from official ImGui code (https://github.com/ocornut/imgui/blob/master/backends/imgui_impl_dx11.cpp)
    /// </remarks>
    internal class ImGuiImplDX11 : IImGuiBackendRenderer
    {
        /// <summary>
        /// Singleton
        /// </summary>
        private unsafe readonly nint _backendRendererName;
        private ImGuiImplDX11Data _backendRendererUserData;
        private bool _backendInitialized = false;

        /// <summary>
        /// GUID DebugObjectName
        /// </summary>
        /// <unmanaged>WKPDID_D3DDebugObjectName</unmanaged>
        /// <unmanaged-short>WKPDID_D3DDebugObjectName</unmanaged-short>
        public static readonly System.Guid DebugObjectName = new(0x429B8C22, 0x9188, 0x4B0C, 0x87, 0x42, 0xAC, 0xB0, 0xBF, 0x85, 0xC2, 0x00);

        private readonly DeviceD3D11 _renderDevice;

        public unsafe ImGuiImplDX11(DeviceD3D11 renderDevice)
        {
            _renderDevice = renderDevice;
            _backendRendererName = Marshal.StringToHGlobalAuto("imgui_impl_dx11");
            _backendRendererUserData = new();
        }

        ~ImGuiImplDX11()
        {
            Dispose(disposing: false);
        }

        public unsafe bool Init(ImGuiIOPtr io)
        {
            return InitImpl(io, _renderDevice.Device, _renderDevice.DeviceContext);
        }

        private unsafe bool InitImpl(ImGuiIOPtr io, ComPtr<ID3D11Device> device, ComPtr<ID3D11DeviceContext> deviceContext)
        {
            if (io.BackendRendererUserData != IntPtr.Zero || _backendInitialized)
            {
                throw new InvalidOperationException("Already initialized a renderer backend!");
            }

            // Setup backend capabilities flags
            // io.BackendRendererUserData = (nint)_backendRendererUserDataPtr.GetAddressOf();
            io.NativePtr->BackendRendererName = (byte*)_backendRendererName;
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;  // We can honor the ImDrawCmd::VtxOffset field, allowing for large meshes.

            io.Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;

            // Get factory from device
            ComPtr<IDXGIDevice> dxgiDevice;
            ComPtr<IDXGIAdapter> dxgiAdapter;
            ComPtr<IDXGIFactory> factory;

            SilkMarshal.ThrowHResult
            (
                device.QueryInterface(out dxgiDevice)
            );
            SilkMarshal.ThrowHResult(
                dxgiDevice.GetParent(out dxgiAdapter)
            );
            SilkMarshal.ThrowHResult(
                dxgiAdapter.GetParent(out factory)
            );
            {
                _backendRendererUserData.D3dDevice = device;
                _backendRendererUserData.D3dDeviceContext = deviceContext;
                _backendRendererUserData.Factory = factory;
            }
            dxgiDevice.Dispose();
            dxgiAdapter.Dispose();

            _backendInitialized = true;

            return true;
        }

        public unsafe void Shutdown()
        {
            if (!_backendInitialized)
            {
                throw new InvalidOperationException("No renderer backend to shutdown, or already shutdown?");
            }

            InvalidateDeviceObjects();
            _backendRendererUserData.Factory.Dispose();
            _backendRendererUserData.D3dDevice.Dispose();
            _backendRendererUserData.D3dDeviceContext.Dispose();

            ImGuiIOPtr io = ImGui.GetIO();
            io.NativePtr->BackendRendererName = null;
            io.BackendRendererUserData = IntPtr.Zero;
            io.BackendFlags &= ~ImGuiBackendFlags.RendererHasVtxOffset;

            _backendInitialized = false;
        }

        public unsafe void NewFrame()
        {
            if (!_backendInitialized)
            {
                throw new InvalidOperationException("Context or backend not initialized! Did you call ImGuiImplDX11.Init()?");
            }

            if (_backendRendererUserData.FontSampler.Handle is null)
                CreateDeviceObjects(_renderDevice.ShaderCompiler);
        }

        public unsafe void RenderDrawData(ImDrawDataPtr drawData)
        {
            // Avoid rendering when minimized
            if (drawData.DisplaySize.X <= 0.0f || drawData.DisplaySize.Y <= 0.0f)
                return;

            ComPtr<ID3D11DeviceContext> deviceContext = _backendRendererUserData.D3dDeviceContext;

            // Create and grow vertex/index buffers if needed
            if (_backendRendererUserData.VertexBuffer.Handle is null || _backendRendererUserData.VertexBufferSize < drawData.TotalVtxCount)
            {
                if (_backendRendererUserData.VertexBuffer.Handle is not null) { _backendRendererUserData.VertexBuffer.Dispose(); _backendRendererUserData.VertexBuffer = null; }
                _backendRendererUserData.VertexBufferSize = drawData.TotalVtxCount + 5000;
                CreateVertexBuffer(_backendRendererUserData.D3dDevice, (uint)(_backendRendererUserData.VertexBufferSize * sizeof(ImDrawVert)), ref _backendRendererUserData.VertexBuffer);
            }
            if (_backendRendererUserData.IndexBuffer.Handle is null || _backendRendererUserData.IndexBufferSize < drawData.TotalIdxCount)
            {
                if (_backendRendererUserData.IndexBuffer.Handle is not null) { _backendRendererUserData.IndexBuffer.Dispose(); _backendRendererUserData.IndexBuffer = null; }
                _backendRendererUserData.IndexBufferSize = drawData.TotalIdxCount + 10000;
                CreateIndexBuffer(_backendRendererUserData.D3dDevice, (uint)(_backendRendererUserData.IndexBufferSize * sizeof(ImDrawIdx)), ref _backendRendererUserData.IndexBuffer);
            }

#if DEBUG && DEBUG_BUFFERS
            #region Debugging buffer
            // Upload vertex/index data into a single contiguous CPU buffer
            // Only to debug if data is correctly set for next step (same action but into GPU buffers this time)
            {
                int vertexBufferSize = _backendRendererUserData.VertexBufferSize * sizeof(ImDrawVert);
                int indexBufferSize = _backendRendererUserData.IndexBufferSize * sizeof(ImDrawIdx);
                System.Console.WriteLine("vertexBufferSize = " + vertexBufferSize);
                System.Console.WriteLine("indexBufferSize = " + indexBufferSize);
                int remainingVertexBufferSpace = _backendRendererUserData.VertexBufferSize;
                int remainingIndexBufferSpace = _backendRendererUserData.IndexBufferSize;
                byte[] debugBufferVertex = new byte[vertexBufferSize];
                byte[] debugBufferIndex = new byte[indexBufferSize];

                System.Collections.Generic.List<ImDrawVert> cmdListVertex = new();
                System.Collections.Generic.List<ImDrawIdx> cmdListIndex = new();

                System.Collections.Generic.List<ImDrawVert> serializedVertex = new();
                System.Collections.Generic.List<ImDrawIdx> serializedIndex = new();

                int totalVertex = 0;
                int totalIndex = 0;
                fixed (byte* debugBufferVertexPtr = debugBufferVertex)
                fixed (byte* debugBufferIndexPtr = debugBufferIndex)
                {
                    ImDrawVert* vertexDest = (ImDrawVert*)(debugBufferVertexPtr);
                    ImDrawIdx* indexDest = (ImDrawIdx*)(debugBufferIndexPtr);

                    for (int n = 0; n < drawData.CmdListsCount; n++)
                    {
                        ImDrawListPtr cmdList = drawData.CmdLists[n];

                        cmdListVertex.EnsureCapacity(cmdList.VtxBuffer.Size);
                        for (int i = 0; i < cmdList.VtxBuffer.Size; i++)
                        {
                            cmdListVertex.Add(((ImDrawVert*)cmdList.VtxBuffer.Data)[i]);
                        }
                        cmdListIndex.EnsureCapacity(cmdList.IdxBuffer.Size);
                        for (int i = 0; i < cmdList.IdxBuffer.Size; i++)
                        {
                            cmdListIndex.Add(((ImDrawIdx*)cmdList.IdxBuffer.Data)[i]);
                        }

                        //MemoryCopy (void* source, void* destination, long destinationSizeInBytes, long sourceBytesToCopy)
                        System.Buffer.MemoryCopy((void*)cmdList.VtxBuffer.Data, vertexDest, remainingVertexBufferSpace * sizeof(ImDrawVert), cmdList.VtxBuffer.Size * sizeof(ImDrawVert));
                        System.Buffer.MemoryCopy((void*)cmdList.IdxBuffer.Data, indexDest, remainingIndexBufferSpace * sizeof(ImDrawIdx), cmdList.IdxBuffer.Size * sizeof(ImDrawIdx));

                        remainingVertexBufferSpace -= cmdList.VtxBuffer.Size;
                        remainingIndexBufferSpace -= cmdList.IdxBuffer.Size;
                        vertexDest += cmdList.VtxBuffer.Size;
                        indexDest += cmdList.IdxBuffer.Size;

                        totalVertex += cmdList.VtxBuffer.Size;
                        totalIndex += cmdList.IdxBuffer.Size;
                    }

                    serializedVertex.EnsureCapacity(totalVertex);
                    for (int i = 0; i < totalVertex; i++)
                    {
                        serializedVertex.Add(((ImDrawVert*)debugBufferVertexPtr)[i]);
                    }

                    serializedIndex.EnsureCapacity(totalIndex);
                    for (int i = 0; i < totalIndex; i++)
                    {
                        serializedIndex.Add(((ImDrawIdx*)debugBufferIndexPtr)[i]);
                    }
                }

                System.Console.WriteLine("totalVertex = " + totalVertex);
                System.Console.WriteLine("totalIndex = " + totalIndex);
                System.Diagnostics.Debug.Assert(cmdListVertex.Count == serializedVertex.Count);
                System.Diagnostics.Debug.Assert(cmdListIndex.Count == serializedIndex.Count);
                for (int i = 0; i < totalVertex; i++)
                {
                    ImDrawVert vertexCmd = cmdListVertex[i];
                    ImDrawVert vertexSerialized = serializedVertex[i];
                    System.Diagnostics.Debug.Assert(vertexCmd.pos.X == vertexSerialized.pos.X && vertexCmd.pos.Y == vertexSerialized.pos.Y, $"Fail serialization vertex {i} (Position)");
                    System.Diagnostics.Debug.Assert(vertexCmd.uv.X == vertexSerialized.uv.X && vertexCmd.uv.Y == vertexSerialized.uv.Y, $"Fail serialization vertex {i} (Texture)");
                    System.Diagnostics.Debug.Assert(vertexCmd.col == vertexSerialized.col, $"Fail serialization vertex {i} (Color)");
                }

                for (int i = 0; i < totalIndex; i++)
                {
                    ImDrawIdx indexCmd = cmdListIndex[i];
                    ImDrawIdx indexSerialized = serializedIndex[i];
                    System.Diagnostics.Debug.Assert(indexCmd == indexSerialized, $"Fail serialization index {i}");
                }
            }
            #endregion
#endif

            // Upload vertex/index data into a single contiguous GPU buffer
            {
                MappedSubresource vertexResource = default;
                MappedSubresource indexResource = default;
                SilkMarshal.ThrowHResult(
                    deviceContext.Map(_backendRendererUserData.VertexBuffer, 0, Map.WriteDiscard, 0, ref vertexResource)
                );
                SilkMarshal.ThrowHResult(
                    deviceContext.Map(_backendRendererUserData.IndexBuffer, 0, Map.WriteDiscard, 0, ref indexResource)
                );
                ImDrawVert* vertexDest = (ImDrawVert*)(vertexResource.PData);
                ImDrawIdx* indexDest = (ImDrawIdx*)(indexResource.PData);
                int remainingVertexBufferSpace = _backendRendererUserData.VertexBufferSize;
                int remainingIndexBufferSpace = _backendRendererUserData.IndexBufferSize;

                for (int n = 0; n < drawData.CmdListsCount; n++)
                {
                    ImDrawListPtr cmdList = drawData.CmdLists[n];
                    //MemoryCopy (void* source, void* destination, long destinationSizeInBytes, long sourceBytesToCopy)
                    System.Buffer.MemoryCopy((void*)cmdList.VtxBuffer.Data, vertexDest, remainingVertexBufferSpace * sizeof(ImDrawVert), cmdList.VtxBuffer.Size * sizeof(ImDrawVert));
                    System.Buffer.MemoryCopy((void*)cmdList.IdxBuffer.Data, indexDest, remainingIndexBufferSpace * sizeof(ImDrawIdx), cmdList.IdxBuffer.Size * sizeof(ImDrawIdx));
                    remainingVertexBufferSpace -= cmdList.VtxBuffer.Size;
                    remainingIndexBufferSpace -= cmdList.IdxBuffer.Size;
                    vertexDest += cmdList.VtxBuffer.Size;
                    indexDest += cmdList.IdxBuffer.Size;
                }

                deviceContext.Unmap(_backendRendererUserData.VertexBuffer, 0);
                deviceContext.Unmap(_backendRendererUserData.IndexBuffer, 0);
            }

            // Setup orthographic projection matrix into our constant buffer
            // Our visible imgui space lies from drawData->DisplayPos (top left) to drawData->DisplayPos+data_data->DisplaySize (bottom right). DisplayPos is (0,0) for single viewport apps.
            {
                MappedSubresource mappedResource = default;
                SilkMarshal.ThrowHResult(
                    deviceContext.Map(_backendRendererUserData.VertexConstantBuffer, 0, Map.WriteDiscard, 0, ref mappedResource)
                );
                void* constantBufferPtr = mappedResource.PData;
                float left = drawData.DisplayPos.X;
                float right = drawData.DisplayPos.X + drawData.DisplaySize.X;
                float top = drawData.DisplayPos.Y;
                float bottom = drawData.DisplayPos.Y + drawData.DisplaySize.Y;
                float nearPlane = 1f;
                Matrix4X4<float> matriceMonde = Matrix4X4.CreateTranslation(0, 0, nearPlane);
                Matrix4X4<float> matriceProjection = CreateOrthographicOffCenterLH(left, right, bottom, top, nearPlane, 3);
                Matrix4X4<float> mvp = matriceMonde * matriceProjection;
                System.Buffer.MemoryCopy(Unsafe.AsPointer(ref mvp), constantBufferPtr, (uint)sizeof(Matrix4X4<float>), (uint)sizeof(Matrix4X4<float>));
                deviceContext.Unmap(_backendRendererUserData.VertexConstantBuffer, 0);
            }

            // Saveold DX conf
            BackupDX11State old = new();
            {
                deviceContext.RSGetScissorRects(ref old.ScissorRectsCount, ref old.ScissorRects.AsSpan()[0]);
                deviceContext.RSGetViewports(ref old.ViewportsCount, ref old.Viewports[0]);
                deviceContext.RSGetState(ref old.RasterizerState);
                deviceContext.OMGetBlendState(ref old.BlendState, ref old.BlendFactor[0], ref old.SampleMask);
                deviceContext.OMGetDepthStencilState(ref old.DepthStencilState, ref old.StencilRef);
                deviceContext.PSGetShaderResources(0, 1, ref old.PSShaderResource);
                deviceContext.PSGetSamplers(0, 1, ref old.PSSampler);
                deviceContext.PSGetShader(ref old.PixelShader, ref old.PSInstances, ref old.PSInstancesCount);
                deviceContext.VSGetShader(ref old.VertexShader, ref old.VSInstances, ref old.VSInstancesCount);
                deviceContext.VSGetConstantBuffers(0, 1, ref old.VSConstantBuffer);
                deviceContext.GSGetShader(ref old.GeometryShader, ref old.GSInstances, ref old.GSInstancesCount);
                deviceContext.IAGetPrimitiveTopology(ref old.PrimitiveTopology);
                deviceContext.IAGetIndexBuffer(ref old.IndexBuffer, ref old.IndexBufferFormat, ref old.IndexBufferOffset);
                deviceContext.IAGetVertexBuffers(0, 1, ref old.VertexBuffer, ref old.VertexBufferStride, ref old.VertexBufferOffset);
                deviceContext.IAGetInputLayout(ref old.InputLayout);
            }

            // Setup desired DX state
            SetupRenderState(drawData, deviceContext);

            // Render command lists
            // (Because we merged all buffers into a single one, we maintain our own offset into them)
            {
                int globalIdxOffset = 0;
                int globalVtxOffset = 0;
                System.Numerics.Vector2 clipOff = drawData.DisplayPos;
                for (int i = 0; i < drawData.CmdListsCount; i++)
                {
                    ImDrawListPtr cmdList = drawData.CmdLists[i];
                    for (int j = 0; j < cmdList.CmdBuffer.Size; j++)
                    {
                        ImDrawCmdPtr cmd = cmdList.CmdBuffer[j];
                        if (cmd.UserCallback != IntPtr.Zero)
                        {
                            // User callback, registered via ImDrawList::AddCallback()
                            // (ImDrawCallback_ResetRenderState is a special callback value used by the user to request the renderer to reset render state.)
                            // #define ImDrawCallback_ResetRenderState     (ImDrawCallback)(-8)
                            if (cmd.UserCallback == -8)
                            {
                                SetupRenderState(drawData, deviceContext);
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                        else
                        {
                            // Project scissor/clipping rectangles into framebuffer space
                            System.Numerics.Vector2 clipMin = new(cmd.ClipRect.X - clipOff.X, cmd.ClipRect.Y - clipOff.Y);
                            System.Numerics.Vector2 clipMax = new(cmd.ClipRect.Z - clipOff.X, cmd.ClipRect.W - clipOff.Y);
                            if (clipMax.X <= clipMin.X || clipMax.Y <= clipMin.Y)
                                continue;

                            // Apply scissor/clipping rectangle
                            Box2D<int> r = new Box2D<int>((int)clipMin.X, (int)clipMin.Y, (int)clipMax.X, (int)clipMax.Y);
                            deviceContext.RSSetScissorRects(1, ref r);

                            // Bind texture, Draw
                            ID3D11ShaderResourceView* texture_srv = (ID3D11ShaderResourceView*)cmd.GetTexID();
                            deviceContext.PSSetShaderResources(0, 1, in texture_srv);
                            uint nbIndexToDraw = cmd.ElemCount;
                            uint indexOffset = (uint)(cmd.IdxOffset + globalIdxOffset);
                            int baseVertexLocation = (int)(cmd.VtxOffset + globalVtxOffset);
                            deviceContext.DrawIndexed(nbIndexToDraw, indexOffset, baseVertexLocation);
                        }
                    }
                    globalIdxOffset += cmdList.IdxBuffer.Size;
                    globalVtxOffset += cmdList.VtxBuffer.Size;
                }
            }

            // Restore modified DX state
            deviceContext.RSSetScissorRects(old.ScissorRectsCount, old.ScissorRects);
            deviceContext.RSSetViewports(old.ViewportsCount, old.Viewports);
            deviceContext.RSSetState(old.RasterizerState);
            old.RasterizerState.Dispose();
            deviceContext.OMSetBlendState(old.BlendState, old.BlendFactor, old.SampleMask);
            old.BlendState.Dispose();
            deviceContext.OMSetDepthStencilState(old.DepthStencilState, old.StencilRef);
            old.DepthStencilState.Dispose();
            deviceContext.PSSetShaderResources(0, 1, ref old.PSShaderResource);
            old.PSShaderResource.Dispose();
            deviceContext.PSSetSamplers(0, 1, ref old.PSSampler);
            old.PSSampler.Dispose();
            deviceContext.PSSetShader(old.PixelShader, ref old.PSInstances, old.PSInstancesCount);
            old.PixelShader.Dispose();
            {
                ID3D11ClassInstance* pSInstancesPtrOrigin = old.PSInstances.Handle;
                for (uint i = 0; i < old.PSInstancesCount; i++)
                {
                    ID3D11ClassInstance* pSInstancesPtr = pSInstancesPtrOrigin + i;
                    if (pSInstancesPtr is not null)
                    {
                        (*pSInstancesPtr).Release();
                    }
                }
            }
            deviceContext.VSSetShader(old.VertexShader, ref old.VSInstances, old.VSInstancesCount); old.VertexShader.Dispose();
            deviceContext.VSSetConstantBuffers(0, 1, ref old.VSConstantBuffer); old.VSConstantBuffer.Dispose();
            deviceContext.GSSetShader(old.GeometryShader, ref old.GSInstances, old.GSInstancesCount); old.GeometryShader.Dispose();
            {
                ID3D11ClassInstance* vSInstancesPtrOrigin = old.VSInstances.Handle;
                for (uint i = 0; i < old.VSInstancesCount; i++)
                {
                    ID3D11ClassInstance* vSInstancesPtr = vSInstancesPtrOrigin + i;
                    if (vSInstancesPtr is not null)
                    {
                        (*vSInstancesPtr).Release();
                    }
                }
            }
            deviceContext.IASetPrimitiveTopology(old.PrimitiveTopology);
            deviceContext.IASetIndexBuffer(old.IndexBuffer, old.IndexBufferFormat, old.IndexBufferOffset); old.IndexBuffer.Dispose();
            deviceContext.IASetVertexBuffers(0, 1, ref old.VertexBuffer, ref old.VertexBufferStride, ref old.VertexBufferOffset); old.VertexBuffer.Dispose();
            deviceContext.IASetInputLayout(old.InputLayout); old.InputLayout.Dispose();
        }

        private unsafe bool CreateDeviceObjects(D3DCompiler shaderCompiler)
        {
            if (_backendRendererUserData.D3dDevice.Handle is null)
                return false;
            if (_backendRendererUserData.FontSampler.Handle is not null)
                InvalidateDeviceObjects();

            // By using D3DCompile() from <d3dcompiler.h> / d3dcompiler.lib, we introduce a dependency to a given version of d3dcompiler_XX.dll (see D3DCOMPILER_DLL_A)
            // If you would like to use this DX11 sample code but remove this dependency you can:
            //  1) compile once, save the compiled shader blobs into a file or source code and pass them to CreateVertexShader()/CreatePixelShader() [preferred solution]
            //  2) use code to detect any version of the DLL and grab a pointer to D3DCompile from the DLL.
            // See https://github.com/ocornut/imgui/pull/638 for sources and details.

            // Create the vertex shader
            if (!InitVertexShader(_backendRendererUserData.D3dDevice, shaderCompiler))
            {
                return false;
            }

            // Create the pixel shader
            if (!InitPixelShader(_backendRendererUserData.D3dDevice, shaderCompiler))
            {
                return false;
            }

            // Create the blending setup
            InitBlendingState(_backendRendererUserData.D3dDevice);

            // Create the rasterizer state
            InitRasterizerState(_backendRendererUserData.D3dDevice);

            // Create depth-stencil State
            InitDepthStencil(_backendRendererUserData.D3dDevice);

            CreateFontsTexture();

            return true;
        }

        private unsafe void CreateFontsTexture()
        {
            // Build texture atlas
            ImGuiIOPtr io = ImGui.GetIO();
            IntPtr pixels;
            int width, height, bytesPerPixel;
            io.Fonts.GetTexDataAsRGBA32(out pixels, out width, out height, out bytesPerPixel);

            // Upload texture to graphics system
            {
                Texture2DDesc desc = new()
                {
                    Width = (uint)width,
                    Height = (uint)height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.FormatR8G8B8A8Unorm,
                    SampleDesc = new SampleDesc(1, 0),
                    Usage = Usage.Default,
                    BindFlags = (uint)BindFlag.ShaderResource,
                    CPUAccessFlags = 0
                };

                ComPtr<ID3D11Texture2D> texture = default;
                SubresourceData subResource = new()
                {
                    PSysMem = (void*)pixels,
                    SysMemPitch = (uint)(width * bytesPerPixel),
                    SysMemSlicePitch = 0
                };
                SilkMarshal.ThrowHResult(
                    _backendRendererUserData.D3dDevice.CreateTexture2D(in desc, in subResource, ref texture)
                );

                // Set Debug Name
                const string fontTextureDebugName = "FontTexture";
                IntPtr namePtr2 = Marshal.StringToHGlobalAnsi(fontTextureDebugName);
                fixed (Guid* guidPtr = &DebugObjectName)
                {
                    texture.SetPrivateData(guidPtr, (uint)fontTextureDebugName.Length, (void*)namePtr2);
                }
                Marshal.FreeHGlobal(namePtr2);

                // Create texture view
                ShaderResourceViewDesc srvDesc = new()
                {
                    Format = Format.FormatR8G8B8A8Unorm,
                    ViewDimension = D3DSrvDimension.D3D11SrvDimensionTexture2D,
                    Texture2D = new Tex2DSrv()
                    {
                        MipLevels = desc.MipLevels,
                        MostDetailedMip = 0
                    }
                };

                _backendRendererUserData.D3dDevice.CreateShaderResourceView(texture, ref srvDesc, ref _backendRendererUserData.FontTextureView);
                texture.Dispose();

                // Set Debug Name
                const string fontTextureViewDebugName = "FontTextureView";
                IntPtr namePtr = Marshal.StringToHGlobalAnsi(fontTextureViewDebugName);
                fixed (Guid* guidPtr = &DebugObjectName)
                {
                    _backendRendererUserData.FontTextureView.SetPrivateData(guidPtr, (uint)fontTextureViewDebugName.Length, (void*)namePtr);
                }
                Marshal.FreeHGlobal(namePtr);
            }

            // Store our identifier
            io.Fonts.ClearTexData();
            io.Fonts.SetTexID((nint)_backendRendererUserData.FontTextureView.Handle);

            // Create texture sampler
            // (Bilinear sampling is required by default. Set 'io.Fonts->Flags |= ImFontAtlasFlags_NoBakedLines' or 'style.AntiAliasedLinesUseTex = false' to allow point/nearest sampling)
            {
                SamplerDesc desc = new()
                {
                    Filter = Filter.MinMagMipPoint,
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    MipLODBias = 0f,
                    ComparisonFunc = ComparisonFunc.Never,
                    MinLOD = 0f,
                    MaxLOD = 0f
                };

                _backendRendererUserData.D3dDevice.CreateSamplerState(ref desc, ref _backendRendererUserData.FontSampler);

                // Set Debug Name
                const string fontSamplerDebugName = "FontSampler";
                IntPtr namePtr = Marshal.StringToHGlobalAnsi(fontSamplerDebugName);
                fixed (Guid* guidPtr = &DebugObjectName)
                {
                    _backendRendererUserData.FontSampler.SetPrivateData(guidPtr, (uint)fontSamplerDebugName.Length, (void*)namePtr);
                }
                Marshal.FreeHGlobal(namePtr);
            }
        }

        /// <summary>
        /// Compilation et chargement du vertex shader
        /// </summary>
        /// <param name="device"></param>
        /// <param name="compiler"></param>
        private unsafe bool InitVertexShader(ComPtr<ID3D11Device> device, D3DCompiler compiler)
        {
            // Compilation et chargement du vertex shader
            ComPtr<ID3D10Blob> compilationBlob = default;
            ComPtr<ID3D10Blob> compilationErrors = default;
            string filePath = "shaders\\vs_imgui.hlsl";
            byte[] shaderCode = File.ReadAllBytes(filePath);
            string entryPoint = "main";
            string target = "vs_5_0";
            // #define D3DCOMPILE_ENABLE_STRICTNESS                    (1 << 11)
            uint flagStrictness = ((uint)1 << 11);
            // #define D3DCOMPILE_DEBUG (1 << 0)
            // #define D3DCOMPILE_SKIP_OPTIMIZATION                    (1 << 2)
#if DEBUG
            uint flagDebug = ((uint)1 << 0);
            uint flagSkipOptimization = ((uint)(1 << 2));
#else
            uint flagDebug = 0;
            uint flagSkipOptimization = 0;
#endif
            HResult hr = compiler.Compile
            (
                in shaderCode[0],
                (nuint)shaderCode.Length,
                filePath,
                ref Unsafe.NullRef<D3DShaderMacro>(),
                ref Unsafe.NullRef<ID3DInclude>(),
                entryPoint,
                target,
                flagStrictness | flagDebug | flagSkipOptimization,
                0,
                ref compilationBlob,
                ref compilationErrors
            );

            // Check for compilation errors.
            if (hr.IsFailure)
            {
                if (compilationErrors.Handle is not null)
                {
                    Console.WriteLine(SilkMarshal.PtrToString((nint)compilationErrors.GetBufferPointer()));
                }
                return false;
            }

            // Create vertex shader.
            hr = device.CreateVertexShader
                (
                    compilationBlob.GetBufferPointer(),
                    compilationBlob.GetBufferSize(),
                    ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                    ref _backendRendererUserData.VertexShader
                );

            if (hr.IsFailure)
            {
                return false;
            }

            // Créer l’organisation des sommets
            fixed (byte* semanticNamePosition = SilkMarshal.StringToMemory("POSITION", NativeStringEncoding.Ansi))
            fixed (byte* semanticNameTexCoord = SilkMarshal.StringToMemory("TEXCOORD", NativeStringEncoding.Ansi))
            fixed (byte* semanticNameColor = SilkMarshal.StringToMemory("COLOR", NativeStringEncoding.Ansi))
            {
                InputElementDesc[] inputElements = new[]
                {
                    new InputElementDesc(
                        semanticNamePosition, 0, Silk.NET.DXGI.Format.FormatR32G32Float, 0, Convert.ToUInt32(Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.pos))), InputClassification.PerVertexData, 0
                    ),
                    new InputElementDesc(
                        semanticNameTexCoord, 0, Silk.NET.DXGI.Format.FormatR32G32Float, 0, Convert.ToUInt32(Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.uv))), InputClassification.PerVertexData, 0
                    ),
                    new InputElementDesc(
                        semanticNameColor, 0, Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm, 0, Convert.ToUInt32(Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.col))), InputClassification.PerVertexData, 0
                    ),
                };

                hr = device.CreateInputLayout
                (
                    in inputElements[0],
                    (uint)inputElements.Length,
                    compilationBlob.GetBufferPointer(),
                    compilationBlob.GetBufferSize(),
                    ref _backendRendererUserData.InputLayout
                );
                compilationBlob.Dispose();
                compilationErrors.Dispose();
                if (hr.IsFailure)
                {
                    return false;
                }
            }

            bool result = CreateConstantBuffer<Matrix4X4<float>>(device, ref _backendRendererUserData.VertexConstantBuffer);
            if (!result)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Compilation et chargement du pixel shader
        /// </summary>
        /// <param name="device"></param>
        /// <param name="compiler"></param>
        private unsafe bool InitPixelShader(ComPtr<ID3D11Device> device, D3DCompiler compiler)
        {
            ComPtr<ID3D10Blob> compilationBlob = default;
            ComPtr<ID3D10Blob> compilationErrors = default;
            string filePath = "shaders\\ps_imgui.hlsl";
            byte[] shaderCode = File.ReadAllBytes(filePath);
            string entryPoint = "main";
            string target = "ps_5_0";
            // #define D3DCOMPILE_ENABLE_STRICTNESS                    (1 << 11)
            uint flagStrictness = ((uint)1 << 11);
            // #define D3DCOMPILE_DEBUG (1 << 0)
            // #define D3DCOMPILE_SKIP_OPTIMIZATION                    (1 << 2)
#if DEBUG
            uint flagDebug = ((uint)1 << 0);
            uint flagSkipOptimization = ((uint)(1 << 2));
#else
            uint flagDebug = 0;
            uint flagSkipOptimization = 0;
#endif
            HResult hr = compiler.Compile
            (
                in shaderCode[0],
                (nuint)shaderCode.Length,
                filePath,
                ref Unsafe.NullRef<D3DShaderMacro>(),
                ref Unsafe.NullRef<ID3DInclude>(),
                entryPoint,
                target,
                flagStrictness | flagDebug | flagSkipOptimization,
                0,
                ref compilationBlob,
                ref compilationErrors
            );

            // Check for compilation errors.
            if (hr.IsFailure)
            {
                if (compilationErrors.Handle is not null)
                {
                    Console.WriteLine(SilkMarshal.PtrToString((nint)compilationErrors.GetBufferPointer()));
                }
                return false;
            }

            // Create pixel shader.
            hr = device.CreatePixelShader
            (
                compilationBlob.GetBufferPointer(),
                compilationBlob.GetBufferSize(),
                ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                ref _backendRendererUserData.PixelShader
            );

            compilationBlob.Dispose();
            compilationErrors.Dispose();
            if (hr.IsFailure)
            {
                return false;
            }
            return true;
        }

        private unsafe bool InitBlendingState(ComPtr<ID3D11Device> device)
        {
            RenderTargetBlendDesc renderTargetBlendDesc = new()
            {
                BlendEnable = true,
                SrcBlend = Blend.SrcAlpha,
                DestBlend = Blend.InvSrcAlpha,
                BlendOp = BlendOp.Add,
                SrcBlendAlpha = Blend.SrcAlpha,
                DestBlendAlpha = Blend.InvSrcAlpha,
                BlendOpAlpha = BlendOp.Add,
                RenderTargetWriteMask = (byte)ColorWriteEnable.All

            };

            BlendDesc.RenderTargetBuffer renderTargetBuffer = new()
            {
                Element0 = renderTargetBlendDesc
            };

            BlendDesc desc = new()
            {
                AlphaToCoverageEnable = false,
                RenderTarget = renderTargetBuffer
            };
            device.CreateBlendState(in desc, ref _backendRendererUserData.BlendState);
            return true;
        }

        private unsafe bool InitRasterizerState(ComPtr<ID3D11Device> device)
        {
            RasterizerDesc desc = new()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                ScissorEnable = true,
                DepthClipEnable = false
            };
            device.CreateRasterizerState(in desc, ref _backendRendererUserData.RasterizerState);
            return true;
        }

        private unsafe bool InitDepthStencil(ComPtr<ID3D11Device> device)
        {
            DepthStencilopDesc frontFace = new()
            {
                StencilFailOp = StencilOp.Keep,
                StencilDepthFailOp = StencilOp.Keep,
                StencilPassOp = StencilOp.Keep,
                StencilFunc = ComparisonFunc.Never
            };

            DepthStencilDesc desc = new()
            {
                DepthEnable = false,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthFunc = ComparisonFunc.Always,
                StencilEnable = false,
                FrontFace = frontFace,
                BackFace = frontFace,
                StencilReadMask = 0,
                StencilWriteMask = 0
            };
            device.CreateDepthStencilState(in desc, ref _backendRendererUserData.DepthStencilState);
            return true;
        }

        private static unsafe void CreateVertexBuffer(ComPtr<ID3D11Device> device, uint size, ref ComPtr<ID3D11Buffer> buffer)
        {
            BufferDesc bufferDesc = new()
            {
                ByteWidth = size,
                Usage = Usage.Dynamic,
                BindFlags = (uint)BindFlag.VertexBuffer,
                CPUAccessFlags = (uint)CpuAccessFlag.Write,
                MiscFlags = 0
            };

            SilkMarshal.ThrowHResult(device.CreateBuffer(in bufferDesc, ref Unsafe.NullRef<SubresourceData>(), ref buffer));
        }

        private static unsafe void CreateIndexBuffer(ComPtr<ID3D11Device> device, uint size, ref ComPtr<ID3D11Buffer> buffer)
        {
            BufferDesc bufferDesc = new()
            {
                ByteWidth = size,
                Usage = Usage.Dynamic,
                BindFlags = (uint)BindFlag.IndexBuffer,
                CPUAccessFlags = (uint)CpuAccessFlag.Write
            };

            SilkMarshal.ThrowHResult(device.CreateBuffer(in bufferDesc, ref Unsafe.NullRef<SubresourceData>(), ref buffer));
        }

        private static unsafe bool CreateConstantBuffer<T>(ComPtr<ID3D11Device> device, ref ComPtr<ID3D11Buffer> buffer) where T : unmanaged
        {
            BufferDesc bufferDesc = new()
            {
                ByteWidth = (uint)(sizeof(T)),
                Usage = Usage.Dynamic,
                BindFlags = (uint)BindFlag.ConstantBuffer,
                CPUAccessFlags = (uint)CpuAccessFlag.Write,
                MiscFlags = 0
            };

            HResult hr = device.CreateBuffer(in bufferDesc, in Unsafe.NullRef<SubresourceData>(), ref buffer);
            return !hr.IsFailure;
        }

        private unsafe void SetupRenderState(ImDrawDataPtr drawData, ComPtr<ID3D11DeviceContext> deviceContext)
        {
            // Setup viewport
            Viewport viewPort = new()
            {
                Width = drawData.DisplaySize.X,
                Height = drawData.DisplaySize.Y,
                MinDepth = 0f,
                MaxDepth = 1f,
                TopLeftX = 0f,
                TopLeftY = 0f
            };
            deviceContext.RSSetViewports(1, ref viewPort);

            // Setup shader and vertex buffers
            uint stride = (uint)sizeof(ImDrawVert);
            uint offset = 0;
            deviceContext.IASetInputLayout(_backendRendererUserData.InputLayout);
            deviceContext.IASetVertexBuffers(0, 1, ref _backendRendererUserData.VertexBuffer, ref stride, ref offset);
            deviceContext.IASetIndexBuffer(_backendRendererUserData.IndexBuffer, sizeof(ImDrawIdx) == 2 ? Format.FormatR16Uint : Format.FormatR32Uint, 0);
            deviceContext.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
            deviceContext.VSSetShader(_backendRendererUserData.VertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            deviceContext.VSSetConstantBuffers(0, 1, ref _backendRendererUserData.VertexConstantBuffer);
            deviceContext.PSSetShader(_backendRendererUserData.PixelShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            deviceContext.PSSetSamplers(0, 1, ref _backendRendererUserData.FontSampler);
            // deviceContext.GSSetShader(Unsafe.NullRef<ComPtr<ID3D11GeometryShader>>(), ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            // deviceContext.HSSetShader(Unsafe.NullRef<ComPtr<ID3D11HullShader>>(), ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0); // In theory we should backup and restore this as well.. very infrequently used..
            // deviceContext.DSSetShader(Unsafe.NullRef<ComPtr<ID3D11DomainShader>>(), ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0); // In theory we should backup and restore this as well.. very infrequently used..
            // deviceContext.CSSetShader(Unsafe.NullRef<ComPtr<ID3D11ComputeShader>>(), ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0); // In theory we should backup and restore this as well.. very infrequently used..

            // Setup blend state
            float[] blendFactor = { 0f, 0f, 0f, 0f };
            deviceContext.OMSetBlendState(_backendRendererUserData.BlendState, blendFactor, 0xffffffff);
            deviceContext.OMSetDepthStencilState(_backendRendererUserData.DepthStencilState, 0);
            deviceContext.RSSetState(_backendRendererUserData.RasterizerState);
        }

        private unsafe void InvalidateDeviceObjects()
        {
            if (_backendRendererUserData.D3dDevice.Handle is null)
                return;

            if (_backendRendererUserData.FontSampler.Handle != null) { _backendRendererUserData.FontSampler.Dispose(); _backendRendererUserData.FontSampler = null; }
            if (_backendRendererUserData.FontTextureView.Handle != null) { _backendRendererUserData.FontTextureView.Dispose(); _backendRendererUserData.FontTextureView = null; ImGui.GetIO().Fonts.SetTexID(0); } // We copied data.FontTextureView to io.Fonts.TexID so let's clear that as well.
            if (_backendRendererUserData.IndexBuffer.Handle != null) { _backendRendererUserData.IndexBuffer.Dispose(); _backendRendererUserData.IndexBuffer = null; }
            if (_backendRendererUserData.VertexBuffer.Handle != null) { _backendRendererUserData.VertexBuffer.Dispose(); _backendRendererUserData.VertexBuffer = null; }
            if (_backendRendererUserData.BlendState.Handle != null) { _backendRendererUserData.BlendState.Dispose(); _backendRendererUserData.BlendState = null; }
            if (_backendRendererUserData.DepthStencilState.Handle != null) { _backendRendererUserData.DepthStencilState.Dispose(); _backendRendererUserData.DepthStencilState = null; }
            if (_backendRendererUserData.RasterizerState.Handle != null) { _backendRendererUserData.RasterizerState.Dispose(); _backendRendererUserData.RasterizerState = null; }
            if (_backendRendererUserData.PixelShader.Handle != null) { _backendRendererUserData.PixelShader.Dispose(); _backendRendererUserData.PixelShader = null; }
            if (_backendRendererUserData.VertexConstantBuffer.Handle != null) { _backendRendererUserData.VertexConstantBuffer.Dispose(); _backendRendererUserData.VertexConstantBuffer = null; }
            if (_backendRendererUserData.InputLayout.Handle != null) { _backendRendererUserData.InputLayout.Dispose(); _backendRendererUserData.InputLayout = null; }
            if (_backendRendererUserData.VertexShader.Handle != null) { _backendRendererUserData.VertexShader.Dispose(); _backendRendererUserData.VertexShader = null; }
        }

        public static Matrix4X4<T> CreateOrthographicOffCenterLH<T>(T left, T right, T bottom, T top, T zNearPlane, T zFarPlane) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            Matrix4X4<T> result = Matrix4X4.CreateOrthographicOffCenter(left, right, bottom, top, zNearPlane, zFarPlane);
            result.M33 = Scalar.Negate(result.M33);
            return result;
        }

        private bool _disposed = false;

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    // _backendRendererUserDataPtr.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                Marshal.FreeHGlobal(_backendRendererName);

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
