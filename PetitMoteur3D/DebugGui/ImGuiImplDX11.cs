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
        private readonly nint _backendRendererName;
        private ImGuiImplDX11Data _backendRendererUserData;
        private bool _backendInitialized = false;

        private readonly DeviceD3D11 _renderDevice;
        private readonly GraphicDeviceRessourceFactory _graphicDeviceRessourceFactory;
        private readonly GraphicPipelineFactory _pipelineFactory;

        private static readonly IObjectPool<System.Numerics.Vector2> _vector2Pool = ObjectPoolFactory.Create(new Vector2Resetter());
        private static readonly IObjectPool<Box2D<int>> _box2DPool = ObjectPoolFactory.Create(new Box2DResetter<int>());
        private static readonly IObjectPool<SamplerDesc> _shaderDescPool = ObjectPoolFactory.Create<SamplerDesc>(new DX11SamplerDescResetter());
        private static readonly IObjectPool<BackupDX11State> _backupDX11StatePool = ObjectPoolFactory.Create<BackupDX11State>();

        public ImGuiImplDX11(DeviceD3D11 renderDevice, GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, GraphicPipelineFactory pipelineFactory)
        {
            _renderDevice = renderDevice;
            _backendRendererName = Marshal.StringToHGlobalAuto("imgui_impl_dx11");
            _backendRendererUserData = new();
            _graphicDeviceRessourceFactory = graphicDeviceRessourceFactory;
            _pipelineFactory = pipelineFactory;
        }

        ~ImGuiImplDX11()
        {
            Dispose(disposing: false);
        }

        public bool Init(ImGuiIOPtr io)
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
                CreateDeviceObjects(_graphicDeviceRessourceFactory, _pipelineFactory);
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
                _backendRendererUserData.VertexBuffer = _graphicDeviceRessourceFactory.BufferFactory.CreateVertexBuffer<ImDrawVert>((uint)_backendRendererUserData.VertexBufferSize, Usage.Dynamic, CpuAccessFlag.Write, name: "ImGuiVertexBuffer");
            }
            if (_backendRendererUserData.IndexBuffer.Handle is null || _backendRendererUserData.IndexBufferSize < drawData.TotalIdxCount)
            {
                if (_backendRendererUserData.IndexBuffer.Handle is not null) { _backendRendererUserData.IndexBuffer.Dispose(); _backendRendererUserData.IndexBuffer = null; }
                _backendRendererUserData.IndexBufferSize = drawData.TotalIdxCount + 10000;
                _backendRendererUserData.IndexBuffer = _graphicDeviceRessourceFactory.BufferFactory.CreateIndexBuffer<ImDrawIdx>((uint)_backendRendererUserData.IndexBufferSize, Usage.Dynamic, CpuAccessFlag.Write, "ImGuiIndexBuffer");
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
            BackupDX11State oldDxState = GetCurrentDX11State(deviceContext);

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
                            System.Numerics.Vector2 clipMin = _vector2Pool.Get();
                            clipMin.X = cmd.ClipRect.X - clipOff.X;
                            clipMin.Y = cmd.ClipRect.Y - clipOff.Y;

                            System.Numerics.Vector2 clipMax = _vector2Pool.Get();
                            clipMax.X = cmd.ClipRect.Z - clipOff.X;
                            clipMax.Y = cmd.ClipRect.W - clipOff.Y;
                            if (clipMax.X <= clipMin.X || clipMax.Y <= clipMin.Y)
                                continue;

                            // Apply scissor/clipping rectangle
                            Box2D<int> r = _box2DPool.Get();
                            r.Min.X = (int)clipMin.X;
                            r.Min.Y = (int)clipMin.Y;
                            r.Max.X = (int)clipMax.X;
                            r.Max.Y = (int)clipMax.Y;
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
            SetDX11State(deviceContext, oldDxState);

            oldDxState.Dispose();
        }

        private unsafe bool CreateDeviceObjects(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, GraphicPipelineFactory pipelineFactory)
        {
            if (_backendRendererUserData.D3dDevice.Handle is null)
                return false;
            if (_backendRendererUserData.FontSampler.Handle is not null)
                InvalidateDeviceObjects();

            // Create the vertex shader
            if (!InitVertexShader(graphicDeviceRessourceFactory.ShaderManager, graphicDeviceRessourceFactory.BufferFactory))
            {
                return false;
            }

            // Create the pixel shader
            if (!InitPixelShader(graphicDeviceRessourceFactory.ShaderManager))
            {
                return false;
            }

            // Create the blending setup
            InitBlendingState(pipelineFactory);

            // Create the rasterizer state
            InitRasterizerState(pipelineFactory);

            // Create depth-stencil State
            InitDepthStencil(pipelineFactory);

            CreateFontsTexture(graphicDeviceRessourceFactory.TextureManager);

            return true;
        }

        private unsafe void CreateFontsTexture(TextureManager textureManager)
        {
            // Build texture atlas
            ImGuiIOPtr io = ImGui.GetIO();
            IntPtr pixels;
            int width, height, bytesPerPixel;
            io.Fonts.GetTexDataAsRGBA32(out pixels, out width, out height, out bytesPerPixel);

            // Upload texture to graphics system
            {
                Texture texture = textureManager.GetOrCreateTexture("ImGuiFontTextureView", pixels, width, height, bytesPerPixel);
                _backendRendererUserData.FontTextureView = texture.TextureView;
            }

            // Store our identifier
            io.Fonts.ClearTexData();
            io.Fonts.SetTexID((nint)_backendRendererUserData.FontTextureView.Handle);

            // Create texture sampler
            // (Bilinear sampling is required by default. Set 'io.Fonts->Flags |= ImFontAtlasFlags_NoBakedLines' or 'style.AntiAliasedLinesUseTex = false' to allow point/nearest sampling)
            {
                SamplerDesc desc = _shaderDescPool.Get();
                desc.Filter = Filter.MinMagMipLinear;
                desc.AddressU = TextureAddressMode.Wrap;
                desc.AddressV = TextureAddressMode.Wrap;
                desc.AddressW = TextureAddressMode.Wrap;
                desc.MipLODBias = 0f;
                desc.MaxAnisotropy = 1;
                desc.ComparisonFunc = ComparisonFunc.Always;
                desc.MinLOD = 0f;
                desc.MaxLOD = float.MaxValue;

                _backendRendererUserData.FontSampler = textureManager.Factory.CreateSampler(desc, "ImGuiFontSampler");
            }
        }

        /// <summary>
        /// Compilation et chargement du vertex shader
        /// </summary>
        /// <param name="shaderManager"></param>
        private bool InitVertexShader(ShaderManager shaderManager, GraphicBufferFactory bufferFactory)
        {
            // Compilation et chargement du vertex shader
            string filePath = "shaders\\vs_imgui.hlsl";
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

            uint compilationFlags = flagStrictness | flagDebug | flagSkipOptimization;
            ShaderCodeFile shaderFile = new
            (
                filePath,
                entryPoint,
                target,
                compilationFlags,
                name: "ImGuiVertexShader"
            );
            shaderManager.GetOrLoadVertexShaderAndLayout(shaderFile, ImDrawVertInputLayout.InputLayoutDesc, ref _backendRendererUserData.VertexShader, ref _backendRendererUserData.InputLayout);

            _backendRendererUserData.VertexConstantBuffer = bufferFactory.CreateConstantBuffer<Matrix4X4<float>>(Usage.Dynamic, CpuAccessFlag.Write, name: "ImGuiVertexConstantBuffer");
            return true;
        }

        /// <summary>
        /// Compilation et chargement du pixel shader
        /// </summary>
        /// <param name="shaderManager"></param>
        private bool InitPixelShader(ShaderManager shaderManager)
        {
            string filePath = "shaders\\ps_imgui.hlsl";
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
            uint compilationFlags = flagStrictness | flagDebug | flagSkipOptimization;
            ShaderCodeFile shaderFile = new
            (
                filePath,
                entryPoint,
                target,
                compilationFlags,
                name: "ImGuiPixelShader"
            );
            _backendRendererUserData.PixelShader = shaderManager.GetOrLoadPixelShader(shaderFile);
            return true;
        }

        private bool InitBlendingState(GraphicPipelineFactory graphicPipelineFactory)
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
            _backendRendererUserData.BlendState = graphicPipelineFactory.CreateBlendState(desc, name: "ImGuiBlendState");
            return true;
        }

        private bool InitRasterizerState(GraphicPipelineFactory graphicPipelineFactory)
        {
            RasterizerDesc desc = new()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                ScissorEnable = true,
                DepthClipEnable = false
            };
            _backendRendererUserData.RasterizerState = graphicPipelineFactory.CreateRasterizerState(desc, name: "ImGuiRasterizerState");
            return true;
        }

        private bool InitDepthStencil(GraphicPipelineFactory graphicPipelineFactory)
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
            _backendRendererUserData.DepthStencilState = graphicPipelineFactory.CreateDepthStencilState(desc, name: "ImGuiDepthStencilState");
            return true;
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

        private BackupDX11State GetCurrentDX11State(ComPtr<ID3D11DeviceContext> deviceContext)
        {
            BackupDX11State curentState = _backupDX11StatePool.Get();
            deviceContext.RSGetScissorRects(ref curentState.ScissorRectsCount, ref curentState.ScissorRects.Value);
            deviceContext.RSGetViewports(ref curentState.ViewportsCount, ref curentState.Viewports.Value);
            deviceContext.RSGetState(ref curentState.RasterizerState);
            deviceContext.OMGetBlendState(ref curentState.BlendState, ref curentState.BlendFactor.Value, ref curentState.SampleMask);
            deviceContext.OMGetDepthStencilState(ref curentState.DepthStencilState, ref curentState.StencilRef);
            deviceContext.PSGetShaderResources(0, 1, ref curentState.PSShaderResource);
            deviceContext.PSGetSamplers(0, 1, ref curentState.PSSampler);
            deviceContext.PSGetShader(ref curentState.PixelShader, ref curentState.PSInstances, ref curentState.PSInstancesCount);
            deviceContext.VSGetShader(ref curentState.VertexShader, ref curentState.VSInstances, ref curentState.VSInstancesCount);
            deviceContext.VSGetConstantBuffers(0, 1, ref curentState.VSConstantBuffer);
            deviceContext.GSGetShader(ref curentState.GeometryShader, ref curentState.GSInstances, ref curentState.GSInstancesCount);
            deviceContext.IAGetPrimitiveTopology(ref curentState.PrimitiveTopology);
            deviceContext.IAGetIndexBuffer(ref curentState.IndexBuffer, ref curentState.IndexBufferFormat, ref curentState.IndexBufferOffset);
            deviceContext.IAGetVertexBuffers(0, 1, ref curentState.VertexBuffer, ref curentState.VertexBufferStride, ref curentState.VertexBufferOffset);
            deviceContext.IAGetInputLayout(ref curentState.InputLayout);
            return curentState;
        }

        private unsafe void SetDX11State(ComPtr<ID3D11DeviceContext> deviceContext, BackupDX11State newDxState)
        {
            deviceContext.RSSetScissorRects(newDxState.ScissorRectsCount, newDxState.ScissorRects);
            deviceContext.RSSetViewports(newDxState.ViewportsCount, newDxState.Viewports);
            deviceContext.RSSetState(newDxState.RasterizerState);
            deviceContext.OMSetBlendState(newDxState.BlendState, newDxState.BlendFactor, newDxState.SampleMask);
            deviceContext.OMSetDepthStencilState(newDxState.DepthStencilState, newDxState.StencilRef);
            deviceContext.PSSetShaderResources(0, 1, ref newDxState.PSShaderResource);
            deviceContext.PSSetSamplers(0, 1, ref newDxState.PSSampler);
            deviceContext.PSSetShader(newDxState.PixelShader, ref newDxState.PSInstances, newDxState.PSInstancesCount);
            deviceContext.VSSetShader(newDxState.VertexShader, ref newDxState.VSInstances, newDxState.VSInstancesCount);
            deviceContext.VSSetConstantBuffers(0, 1, ref newDxState.VSConstantBuffer);
            deviceContext.GSSetShader(newDxState.GeometryShader, ref newDxState.GSInstances, newDxState.GSInstancesCount);
            deviceContext.IASetPrimitiveTopology(newDxState.PrimitiveTopology);
            deviceContext.IASetIndexBuffer(newDxState.IndexBuffer, newDxState.IndexBufferFormat, newDxState.IndexBufferOffset);
            deviceContext.IASetVertexBuffers(0, 1, ref newDxState.VertexBuffer, ref newDxState.VertexBufferStride, ref newDxState.VertexBufferOffset);
            deviceContext.IASetInputLayout(newDxState.InputLayout);
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
