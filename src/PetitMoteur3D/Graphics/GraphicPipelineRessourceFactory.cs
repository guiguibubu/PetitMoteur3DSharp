using System;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics;

internal sealed class GraphicPipelineRessourceFactory
{
    private readonly D3D11GraphicDevice _graphicDevice;

    public GraphicPipelineRessourceFactory(D3D11GraphicDevice graphicDevice)
    {
        _graphicDevice = graphicDevice;
    }

    public unsafe ComPtr<ID3D11BlendState> CreateBlendState(BlendDesc desc, string name = "")
    {
        ComPtr<ID3D11BlendState> blendState = default;
        SilkMarshal.ThrowHResult(_graphicDevice.Device.CreateBlendState(in desc, ref blendState));
        if (!string.IsNullOrEmpty(name))
        {
            // Set Debug Name
            using (GlobalMemory unmanagedName = SilkMarshal.StringToMemory(name, NativeStringEncoding.Ansi))
            {
                nint namePtr = unmanagedName.Handle;
                fixed (Guid* guidPtr = &Windows.Win32.PInvoke.WKPDID_D3DDebugObjectName)
                {
                    blendState.SetPrivateData(guidPtr, (uint)name.Length, namePtr.ToPointer());
                }
            }
        }
        return blendState;
    }
    public unsafe ComPtr<ID3D11DepthStencilState> CreateDepthStencilState(DepthStencilDesc desc, string name = "")
    {
        ComPtr<ID3D11DepthStencilState> depthStencilState = default;
        SilkMarshal.ThrowHResult(_graphicDevice.Device.CreateDepthStencilState(in desc, ref depthStencilState));
        if (!string.IsNullOrEmpty(name))
        {
            // Set Debug Name
            using (GlobalMemory unmanagedName = SilkMarshal.StringToMemory(name, NativeStringEncoding.Ansi))
            {
                nint namePtr = unmanagedName.Handle;
                fixed (Guid* guidPtr = &Windows.Win32.PInvoke.WKPDID_D3DDebugObjectName)
                {
                    depthStencilState.SetPrivateData(guidPtr, (uint)name.Length, namePtr.ToPointer());
                }
            }
        }
        return depthStencilState;
    }

    public unsafe ComPtr<ID3D11RasterizerState> CreateRasterizerState(in RasterizerDesc desc, string name = "")
    {
        ComPtr<ID3D11RasterizerState> rasterizerState = default;
        SilkMarshal.ThrowHResult(_graphicDevice.Device.CreateRasterizerState(in desc, ref rasterizerState));
        if (!string.IsNullOrEmpty(name))
        {
            // Set Debug Name
            using (GlobalMemory unmanagedName = SilkMarshal.StringToMemory(name, NativeStringEncoding.Ansi))
            {
                nint namePtr = unmanagedName.Handle;
                fixed (Guid* guidPtr = &Windows.Win32.PInvoke.WKPDID_D3DDebugObjectName)
                {
                    rasterizerState.SetPrivateData(guidPtr, (uint)name.Length, namePtr.ToPointer());
                }
            }
        }
        return rasterizerState;
    }

    public void Map(ComPtr<ID3D11Buffer> pResource, uint Subresource, Map MapType, uint MapFlags, ref MappedSubresource pMappedResource)
    {
        SilkMarshal.ThrowHResult(_graphicDevice.DeviceContext.Map(pResource, Subresource, MapType, MapFlags, ref pMappedResource));
    }

    public void Unmap(ComPtr<ID3D11Buffer> pResource, uint Subresource)
    {
        _graphicDevice.DeviceContext.Unmap(pResource, Subresource);
    }

    public void UpdateSubresource<T>(ComPtr<ID3D11Buffer> pDstResource, uint DstSubresource, in Box pDstBox, in T pSrcData, uint SrcRowPitch, uint SrcDepthPitch) where T : unmanaged
    {
        _graphicDevice.DeviceContext.UpdateSubresource(pDstResource, DstSubresource, in pDstBox, in pSrcData, SrcRowPitch, SrcDepthPitch);
    }
}
