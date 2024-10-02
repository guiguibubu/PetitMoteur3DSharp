using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using Silk.NET.Core;
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
    internal class ImGuiImplDX11
    {
        /// <summary>
        /// Singleton
        /// </summary>
        private unsafe readonly nint _backendRendererName;
        private ImGuiImplDX11Data _backendRendererUserData;
        private ComPtr<ImGuiImplDX11Data> _backendRendererUserDataPtr;

        private readonly DeviceD3D11 _renderDevice;

        public unsafe ImGuiImplDX11(DeviceD3D11 renderDevice)
        {
            _renderDevice = renderDevice;
            _backendRendererName = Marshal.StringToHGlobalAuto("imgui_impl_dx11");
            _backendRendererUserData = new();
            _backendRendererUserDataPtr = (ImGuiImplDX11Data*)Unsafe.AsPointer(ref _backendRendererUserData);
        }

        unsafe ~ImGuiImplDX11()
        {
            Marshal.FreeHGlobal(_backendRendererName);
            _backendRendererUserDataPtr.Dispose();
        }

        public unsafe bool Init(ComPtr<ID3D11Device> device, ComPtr<ID3D11DeviceContext> deviceContext)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            if (io.BackendRendererUserData != IntPtr.Zero)
            {
                throw new InvalidOperationException("Already initialized a renderer backend!");
            }

            // Setup backend capabilities flags
            io.BackendRendererUserData = (nint)_backendRendererUserDataPtr.GetAddressOf();
            io.NativePtr->BackendRendererName = (byte*)_backendRendererName;
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;  // We can honor the ImDrawCmd::VtxOffset field, allowing for large meshes.

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

            return true;
        }

        public unsafe void Shutdown()
        {
            ComPtr<ImGuiImplDX11Data> backendRendererUserDataPtr = GetBackendData();
            if (backendRendererUserDataPtr.Handle is null)
            {
                throw new InvalidOperationException("No renderer backend to shutdown, or already shutdown?");
            }

            InvalidateDeviceObjects();
            ImGuiImplDX11Data backendRendererUserData = backendRendererUserDataPtr.Get();
            backendRendererUserData.Factory.Dispose();
            backendRendererUserData.D3dDevice.Dispose();
            backendRendererUserData.D3dDeviceContext.Dispose();

            ImGuiIOPtr io = ImGui.GetIO();
            io.NativePtr->BackendRendererName = null;
            io.BackendRendererUserData = IntPtr.Zero;
            io.BackendFlags &= ~ImGuiBackendFlags.RendererHasVtxOffset;
            backendRendererUserDataPtr.Dispose();
        }

        public unsafe void NewFrame()
        {
            ComPtr<ImGuiImplDX11Data> backendRendererUserDataPtr = GetBackendData();
            if (backendRendererUserDataPtr.Handle is null)
            {
                throw new InvalidOperationException("Context or backend not initialized! Did you call ImGuiImplDX11.Init()?");
            }

            if (backendRendererUserDataPtr.Get().FontSampler.Handle is null)
                CreateDeviceObjects(_renderDevice.ShaderCompiler);
        }

        public unsafe void RenderDrawData(ImDrawDataPtr drawData)
        {
            // Avoid rendering when minimized
            if (drawData.DisplaySize.X <= 0.0f || drawData.DisplaySize.Y <= 0.0f)
                return;

            ComPtr<ImGuiImplDX11Data> backendRendererPtr = GetBackendData();
            ImGuiImplDX11Data backendRenderer = backendRendererPtr.Get();
            ComPtr<ID3D11DeviceContext> deviceContext = backendRenderer.D3dDeviceContext;

            // Create and grow vertex/index buffers if needed
            if (backendRenderer.VertexBuffer.Handle is null || backendRenderer.VertexBufferSize < drawData.TotalVtxCount)
            {
                if (backendRenderer.VertexBuffer.Handle is not null) { backendRenderer.VertexBuffer.Dispose(); backendRenderer.VertexBuffer = null; }
                backendRenderer.VertexBufferSize = drawData.TotalVtxCount + 5000;
                CreateVertexBuffer(backendRenderer.D3dDevice, (uint)(backendRenderer.VertexBufferSize * sizeof(ImDrawVert)), ref backendRenderer.VertexBuffer);
            }
            if (backendRenderer.IndexBuffer.Handle is null || backendRenderer.IndexBufferSize < drawData.TotalIdxCount)
            {
                if (backendRenderer.IndexBuffer.Handle is not null) { backendRenderer.IndexBuffer.Dispose(); backendRenderer.IndexBuffer = null; }
                backendRenderer.IndexBufferSize = drawData.TotalIdxCount + 10000;
                CreateIndexBuffer(backendRenderer.D3dDevice, (uint)(backendRenderer.IndexBufferSize * sizeof(ushort)), ref backendRenderer.IndexBuffer);
            }

            // Upload vertex/index data into a single contiguous GPU buffer
            MappedSubresource vertexResource = default;
            MappedSubresource indexResource = default;
            SilkMarshal.ThrowHResult(
                deviceContext.Map(backendRenderer.VertexBuffer, 0, Map.WriteDiscard, 0, ref vertexResource)
            );
            SilkMarshal.ThrowHResult(
                deviceContext.Map(backendRenderer.IndexBuffer, 0, Map.WriteDiscard, 0, ref indexResource)
            );
            ImDrawVert* vertexDest = (ImDrawVert*)(vertexResource.PData);
            uint* indexDest = (uint*)(indexResource.PData);
            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[n];
                //MemoryCopy (void* source, void* destination, long destinationSizeInBytes, long sourceBytesToCopy)
                System.Buffer.MemoryCopy((void*)cmdList.VtxBuffer.Data, vertexDest, backendRenderer.VertexBufferSize, cmdList.VtxBuffer.Size * sizeof(ImDrawVert));
                System.Buffer.MemoryCopy((void*)cmdList.IdxBuffer.Data, indexDest, backendRenderer.IndexBufferSize, cmdList.IdxBuffer.Size * sizeof(ushort));
                vertexDest += cmdList.VtxBuffer.Size;
                indexDest += cmdList.IdxBuffer.Size;
            }
            deviceContext.Unmap(backendRenderer.VertexBuffer, 0);
            deviceContext.Unmap(backendRenderer.IndexBuffer, 0);

            // Setup orthographic projection matrix into our constant buffer
            // Our visible imgui space lies from drawData->DisplayPos (top left) to drawData->DisplayPos+data_data->DisplaySize (bottom right). DisplayPos is (0,0) for single viewport apps.
            {
                MappedSubresource mappedResource = default;
                SilkMarshal.ThrowHResult(
                    deviceContext.Map(backendRenderer.VertexConstantBuffer, 0, Map.WriteDiscard, 0, ref mappedResource)
                );
                void* constantBufferPtr = mappedResource.PData;
                float left = drawData.DisplayPos.X;
                float right = drawData.DisplayPos.X + drawData.DisplaySize.X;
                float top = drawData.DisplayPos.Y;
                float bottom = drawData.DisplayPos.Y + drawData.DisplaySize.Y;
                Matrix4X4<float> mvp = CreateOrthographicOffCenterLH(left, right, bottom, top, 1, 3);
                System.Buffer.MemoryCopy(Unsafe.AsPointer(ref mvp), constantBufferPtr, (uint)sizeof(Matrix4X4<float>), (uint)sizeof(Matrix4X4<float>));
                deviceContext.Unmap(backendRenderer.VertexConstantBuffer, 0);
            }

            BackupDX11State old = new();
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
            // Setup desired DX state
            SetupRenderState(drawData, deviceContext);

            // Render command lists
            // (Because we merged all buffers into a single one, we maintain our own offset into them)
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

        // Backend data stored in io.BackendRendererUserData to allow support for multiple Dear ImGui contexts
        // It is STRONGLY preferred that you use docking branch with multi-viewports (== single Dear ImGui context + multiple windows) instead of multiple Dear ImGui contexts.
        private static unsafe ComPtr<ImGuiImplDX11Data> GetBackendData()
        {
            return ImGui.GetCurrentContext() != IntPtr.Zero ? (ImGuiImplDX11Data*)ImGui.GetIO().BackendRendererUserData : (ImGuiImplDX11Data*)Unsafe.AsPointer(ref Unsafe.NullRef<ImGuiImplDX11Data>());
        }

        private unsafe bool CreateDeviceObjects(D3DCompiler shaderCompiler)
        {
            ComPtr<ImGuiImplDX11Data> backendRendererUserDataPtr = GetBackendData();
            ImGuiImplDX11Data backendRendererUserData = backendRendererUserDataPtr.Get();
            if (backendRendererUserData.D3dDevice.Handle is null)
                return false;
            if (backendRendererUserData.FontSampler.Handle is not null)
                InvalidateDeviceObjects();

            // By using D3DCompile() from <d3dcompiler.h> / d3dcompiler.lib, we introduce a dependency to a given version of d3dcompiler_XX.dll (see D3DCOMPILER_DLL_A)
            // If you would like to use this DX11 sample code but remove this dependency you can:
            //  1) compile once, save the compiled shader blobs into a file or source code and pass them to CreateVertexShader()/CreatePixelShader() [preferred solution]
            //  2) use code to detect any version of the DLL and grab a pointer to D3DCompile from the DLL.
            // See https://github.com/ocornut/imgui/pull/638 for sources and details.

            // Create the vertex shader
            if (!InitVertexShader(backendRendererUserData.D3dDevice, shaderCompiler))
            {
                return false;
            }

            // Create the pixel shader
            if (!InitPixelShader(backendRendererUserData.D3dDevice, shaderCompiler))
            {
                return false;
            }

            // Create the blending setup
            InitBlendingState(backendRendererUserData.D3dDevice);

            // Create the rasterizer state
            InitRasterizerState(backendRendererUserData.D3dDevice);

            // Create depth-stencil State
            InitDepthStencil(backendRendererUserData.D3dDevice);

            CreateFontsTexture();

            return true;
        }

        private unsafe void CreateFontsTexture()
        {
            // Build texture atlas
            ImGuiIOPtr io = ImGui.GetIO();
            ComPtr<ImGuiImplDX11Data> backendRendererUserDataPtr = GetBackendData();
            ImGuiImplDX11Data backendRendererUserData = backendRendererUserDataPtr.Get();
            byte* pixels;
            int width, height;
            io.Fonts.GetTexDataAsRGBA32(out pixels, out width, out height);

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
                    PSysMem = pixels,
                    SysMemPitch = desc.Width * 4,
                    SysMemSlicePitch = 0
                };
                SilkMarshal.ThrowHResult(
                    backendRendererUserData.D3dDevice.CreateTexture2D(in desc, in subResource, ref texture)
                );

                // Create texture view
                ShaderResourceViewDesc srvDesc = new()
                {
                    Format = Format.FormatR8G8B8A8Unorm,
                    ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2D,
                    Texture2D = new Tex2DSrv()
                    {
                        MipLevels = desc.MipLevels,
                        MostDetailedMip = 0
                    }
                };

                _backendRendererUserData.D3dDevice.CreateShaderResourceView(texture, ref srvDesc, ref _backendRendererUserData.FontTextureView);
                texture.Dispose();
            }

            // Store our identifier
            io.Fonts.SetTexID((nint)_backendRendererUserData.FontTextureView.Handle);

            // Create texture sampler
            // (Bilinear sampling is required by default. Set 'io.Fonts->Flags |= ImFontAtlasFlags_NoBakedLines' or 'style.AntiAliasedLinesUseTex = false' to allow point/nearest sampling)
            {
                SamplerDesc desc = new()
                {
                    Filter = Filter.MinMagMipLinear,
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    MipLODBias = 0f,
                    ComparisonFunc = ComparisonFunc.Always,
                    MinLOD = 0f,
                    MaxLOD = 0f
                };

                _backendRendererUserData.D3dDevice.CreateSamplerState(ref desc, ref _backendRendererUserData.FontSampler);
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
            fixed (byte* semanticNamePosition = SilkMarshal.StringToMemory("POSITION"))
            fixed (byte* semanticNameTexCoord = SilkMarshal.StringToMemory("TEXCOORD"))
            fixed (byte* semanticNameColor = SilkMarshal.StringToMemory("COLOR"))
            {
                InputElementDesc[] inputElements = new[]
                {
                    new InputElementDesc(
                        semanticNamePosition, 0, Silk.NET.DXGI.Format.FormatR32G32Float, 0, 0, InputClassification.PerVertexData, 0
                    ),
                    new InputElementDesc(
                        semanticNameTexCoord, 0, Silk.NET.DXGI.Format.FormatR32G32Float, 0, (uint)sizeof(System.Numerics.Vector2), InputClassification.PerVertexData, 0
                    ),
                    new InputElementDesc(
                        semanticNameColor, 0, Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm, 0, (uint)(2 * sizeof(System.Numerics.Vector2)), InputClassification.PerVertexData, 0
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
                SrcBlendAlpha = Blend.One,
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
                DepthClipEnable = true
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
                StencilFunc = ComparisonFunc.Always
            };

            DepthStencilDesc desc = new()
            {
                DepthEnable = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthFunc = ComparisonFunc.Always,
                StencilEnable = false,
                FrontFace = frontFace,
                BackFace = frontFace
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
                Usage = Usage.Default,
                BindFlags = (uint)BindFlag.ConstantBuffer,
                CPUAccessFlags = 0,
                MiscFlags = 0
            };

            HResult hr = device.CreateBuffer(in bufferDesc, in Unsafe.NullRef<SubresourceData>(), ref buffer);
            return !hr.IsFailure;
        }

        private static unsafe void SetupRenderState(ImDrawDataPtr drawData, ComPtr<ID3D11DeviceContext> deviceContext)
        {
            ComPtr<ImGuiImplDX11Data> backendRendererPtr = GetBackendData();
            ImGuiImplDX11Data backendRenderer = backendRendererPtr.Get();

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
            deviceContext.IASetInputLayout(backendRenderer.InputLayout);
            deviceContext.IASetVertexBuffers(0, 1, ref backendRenderer.VertexBuffer, ref stride, ref offset);
            deviceContext.IASetIndexBuffer(backendRenderer.IndexBuffer, sizeof(ushort) == 2 ? Format.FormatR16Uint : Format.FormatR32Uint, 0);
            deviceContext.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
            deviceContext.VSSetShader(backendRenderer.VertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            deviceContext.VSSetConstantBuffers(0, 1, ref backendRenderer.VertexConstantBuffer);
            deviceContext.PSSetShader(backendRenderer.PixelShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            deviceContext.PSSetSamplers(0, 1, ref backendRenderer.FontSampler);
            deviceContext.GSSetShader(Unsafe.NullRef<ComPtr<ID3D11GeometryShader>>(), ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            deviceContext.HSSetShader(Unsafe.NullRef<ComPtr<ID3D11HullShader>>(), ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0); // In theory we should backup and restore this as well.. very infrequently used..
            deviceContext.DSSetShader(Unsafe.NullRef<ComPtr<ID3D11DomainShader>>(), ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0); // In theory we should backup and restore this as well.. very infrequently used..
            deviceContext.CSSetShader(Unsafe.NullRef<ComPtr<ID3D11ComputeShader>>(), ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0); // In theory we should backup and restore this as well.. very infrequently used..

            // Setup blend state
            float[] blendFactor = { 0f, 0f, 0f, 0f };
            deviceContext.OMSetBlendState(backendRenderer.BlendState, blendFactor, 0xffffffff);
            deviceContext.OMSetDepthStencilState(backendRenderer.DepthStencilState, 0);
            deviceContext.RSSetState(backendRenderer.RasterizerState);
        }

        private static unsafe void InvalidateDeviceObjects()
        {
            ComPtr<ImGuiImplDX11Data> bdPtr = GetBackendData();
            ImGuiImplDX11Data bd = bdPtr.Get();
            if (bd.D3dDevice.Handle is null)
                return;

            if (bd.FontSampler.Handle != null) { bd.FontSampler.Dispose(); bd.FontSampler = null; }
            if (bd.FontTextureView.Handle != null) { bd.FontTextureView.Dispose(); bd.FontTextureView = null; ImGui.GetIO().Fonts.SetTexID(0); } // We copied data.FontTextureView to io.Fonts.TexID so let's clear that as well.
            if (bd.IndexBuffer.Handle != null) { bd.IndexBuffer.Dispose(); bd.IndexBuffer = null; }
            if (bd.VertexBuffer.Handle != null) { bd.VertexBuffer.Dispose(); bd.VertexBuffer = null; }
            if (bd.BlendState.Handle != null) { bd.BlendState.Dispose(); bd.BlendState = null; }
            if (bd.DepthStencilState.Handle != null) { bd.DepthStencilState.Dispose(); bd.DepthStencilState = null; }
            if (bd.RasterizerState.Handle != null) { bd.RasterizerState.Dispose(); bd.RasterizerState = null; }
            if (bd.PixelShader.Handle != null) { bd.PixelShader.Dispose(); bd.PixelShader = null; }
            if (bd.VertexConstantBuffer.Handle != null) { bd.VertexConstantBuffer.Dispose(); bd.VertexConstantBuffer = null; }
            if (bd.InputLayout.Handle != null) { bd.InputLayout.Dispose(); bd.InputLayout = null; }
            if (bd.VertexShader.Handle != null) { bd.VertexShader.Dispose(); bd.VertexShader = null; }
        }

        public static Matrix4X4<T> CreateOrthographicOffCenterLH<T>(T left, T right, T bottom, T top, T zNearPlane, T zFarPlane) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            Matrix4X4<T> result = Matrix4X4.CreateOrthographicOffCenter(left, right, bottom, top, zNearPlane, zFarPlane);
            result.M33 = Scalar.Negate(result.M33);
            return result;
        }
    }
}