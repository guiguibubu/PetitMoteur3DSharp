using System;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D
{
    internal class GraphicPipelineFactory
    {
        private readonly ComPtr<ID3D11Device> _device;

        public GraphicPipelineFactory(ComPtr<ID3D11Device> device)
        {
            _device = device;
        }

        public unsafe ComPtr<ID3D11BlendState> CreateBlendState(BlendDesc desc, string name = "")
        {
            ComPtr<ID3D11BlendState> blendState = default;
            SilkMarshal.ThrowHResult(_device.CreateBlendState(in desc, ref blendState));
            if (!string.IsNullOrEmpty(name))
            {
                // Set Debug Name
                using (GlobalMemory unmanagedName = SilkMarshal.StringToMemory(name, NativeStringEncoding.Ansi))
                {
                    IntPtr namePtr = unmanagedName.Handle;
                    fixed (Guid* guidPtr = &D3DCommonGuids.DebugObjectName)
                    {
                        blendState.SetPrivateData(guidPtr, (uint)name.Length, (void*)namePtr);
                    }
                }
            }
            return blendState;
        }
        public unsafe ComPtr<ID3D11DepthStencilState> CreateDepthStencilState(DepthStencilDesc desc, string name = "")
        {
            ComPtr<ID3D11DepthStencilState> depthStencilState = default;
            SilkMarshal.ThrowHResult(_device.CreateDepthStencilState(in desc, ref depthStencilState));
            if (!string.IsNullOrEmpty(name))
            {
                // Set Debug Name
                using (GlobalMemory unmanagedName = SilkMarshal.StringToMemory(name, NativeStringEncoding.Ansi))
                {
                    IntPtr namePtr = unmanagedName.Handle;
                    fixed (Guid* guidPtr = &D3DCommonGuids.DebugObjectName)
                    {
                        depthStencilState.SetPrivateData(guidPtr, (uint)name.Length, (void*)namePtr);
                    }
                }
            }
            return depthStencilState;
        }

        public unsafe ComPtr<ID3D11RasterizerState> CreateRasterizerState(RasterizerDesc desc, string name = "")
        {
            ComPtr<ID3D11RasterizerState> rasterizerState = default;
            SilkMarshal.ThrowHResult(_device.CreateRasterizerState(in desc, ref rasterizerState));
            if (!string.IsNullOrEmpty(name))
            {
                // Set Debug Name
                using (GlobalMemory unmanagedName = SilkMarshal.StringToMemory(name, NativeStringEncoding.Ansi))
                {
                    IntPtr namePtr = unmanagedName.Handle;
                    fixed (Guid* guidPtr = &D3DCommonGuids.DebugObjectName)
                    {
                        rasterizerState.SetPrivateData(guidPtr, (uint)name.Length, (void*)namePtr);
                    }
                }
            }
            return rasterizerState;
        }
    }
}
