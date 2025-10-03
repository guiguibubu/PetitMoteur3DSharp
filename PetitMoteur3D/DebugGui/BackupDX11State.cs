using System;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;

namespace PetitMoteur3D.DebugGui
{
    /// <summary>
    /// // Backup DX state
    /// </summary>
    /// <remarks>
    /// Adapted from BACKUP_DX11_STATE struct in official ImGui code (https://github.com/ocornut/imgui/blob/master/backends/imgui_impl_dx11.cpp)
    /// </remarks>
    internal struct BackupDX11State : IComVtbl<BackupDX11State>, IDisposable
    {
        public uint ScissorRectsCount = D3D11.ViewportAndScissorrectObjectCountPerPipeline;
        public uint ViewportsCount = D3D11.ViewportAndScissorrectObjectCountPerPipeline;
        public Box2D<int>[] ScissorRects = new Box2D<int>[D3D11.ViewportAndScissorrectObjectCountPerPipeline];
        public Viewport[] Viewports = new Viewport[D3D11.ViewportAndScissorrectObjectCountPerPipeline];
        public ComPtr<ID3D11RasterizerState> RasterizerState = null;
        public ComPtr<ID3D11BlendState> BlendState = null;
        public float[] BlendFactor = new float[4];
        public uint SampleMask = 0;
        public uint StencilRef = 0;
        public ComPtr<ID3D11DepthStencilState> DepthStencilState = null;
        public ComPtr<ID3D11ShaderResourceView> PSShaderResource = null;
        public ComPtr<ID3D11SamplerState> PSSampler = null;
        public ComPtr<ID3D11PixelShader> PixelShader = null;
        public ComPtr<ID3D11VertexShader> VertexShader = null;
        public ComPtr<ID3D11GeometryShader> GeometryShader = null;
        public uint PSInstancesCount = MAX_COUNT_INSTANCES_SHADER;
        public uint VSInstancesCount = MAX_COUNT_INSTANCES_SHADER;
        public uint GSInstancesCount = MAX_COUNT_INSTANCES_SHADER;
        public ComPtr<ID3D11ClassInstance> PSInstances = null;
        public ComPtr<ID3D11ClassInstance> VSInstances = null;
        public ComPtr<ID3D11ClassInstance> GSInstances = null;
        public D3DPrimitiveTopology PrimitiveTopology = D3DPrimitiveTopology.D3D11PrimitiveTopologyUndefined;
        public ComPtr<ID3D11Buffer> IndexBuffer = null;
        public ComPtr<ID3D11Buffer> VertexBuffer = null;
        public ComPtr<ID3D11Buffer> VSConstantBuffer = null;
        public uint IndexBufferOffset = 0;
        public uint VertexBufferStride = 0;
        public uint VertexBufferOffset = 0;
        public Silk.NET.DXGI.Format IndexBufferFormat = Format.FormatUnknown;
        public ComPtr<ID3D11InputLayout> InputLayout = null;


        /// <summary>
        /// The maximum number of instances a shader can have is 256. https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-pssetshader
        /// </summary>
        private const int MAX_COUNT_INSTANCES_SHADER = 256;

        public BackupDX11State() { }

        unsafe void*** IComVtbl.AsVtblPtr()
        {
            return (void***)Unsafe.AsPointer(ref Unsafe.AsRef(ref this));
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
        private unsafe void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                RasterizerState.Dispose();
                BlendState.Dispose();
                DepthStencilState.Dispose();
                PSShaderResource.Dispose();
                PSSampler.Dispose();
                PixelShader.Dispose();
                {
                    ID3D11ClassInstance* pSInstancesPtrOrigin = PSInstances.Handle;
                    for (uint i = 0; i < PSInstancesCount; i++)
                    {
                        ID3D11ClassInstance* pSInstancesPtr = pSInstancesPtrOrigin + i;
                        if (pSInstancesPtr is not null)
                        {
                            (*pSInstancesPtr).Release();
                        }
                    }
                }
                VertexShader.Dispose();
                {
                    ID3D11ClassInstance* vSInstancesPtrOrigin = VSInstances.Handle;
                    for (uint i = 0; i < VSInstancesCount; i++)
                    {
                        ID3D11ClassInstance* vSInstancesPtr = vSInstancesPtrOrigin + i;
                        if (vSInstancesPtr is not null)
                        {
                            (*vSInstancesPtr).Release();
                        }
                    }
                }
                VSConstantBuffer.Dispose();
                GeometryShader.Dispose();
                IndexBuffer.Dispose();
                VertexBuffer.Dispose();
                InputLayout.Dispose();

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
