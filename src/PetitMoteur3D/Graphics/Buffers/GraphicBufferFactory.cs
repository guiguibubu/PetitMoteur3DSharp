using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Buffers;

internal sealed class GraphicBufferFactory
{
    private readonly D3D11GraphicDevice _graphicDevice;

    public GraphicBufferFactory(D3D11GraphicDevice graphicDevice)
    {
        _graphicDevice = graphicDevice;
    }

    public VertexBuffer CreateVertexBuffer<T>(T[] data, Usage usage = Usage.Default, CpuAccessFlag cpuAcccassFlags = CpuAccessFlag.None, string name = "") where T : unmanaged
    {
        GraphicBuffer buffer = CreateBuffer(BindFlag.VertexBuffer, data, usage, cpuAcccassFlags, name);
        return new VertexBuffer(buffer);
    }

    public VertexBuffer CreateVertexBuffer<T>(uint nbElements, Usage usage = Usage.Default, CpuAccessFlag cpuAcccassFlags = CpuAccessFlag.None, string name = "") where T : unmanaged
    {
        GraphicBuffer buffer = CreateEmptyBuffer<T>(BindFlag.VertexBuffer, nbElements, usage, cpuAcccassFlags, name);
        return new VertexBuffer(buffer);
    }

    public unsafe IndexBuffer CreateIndexBuffer<T>(T[] data, Usage usage = Usage.Default, CpuAccessFlag cpuAcccassFlags = CpuAccessFlag.None, string name = "") where T : unmanaged
    {
        GraphicBuffer buffer = CreateBuffer(BindFlag.IndexBuffer, data, usage, cpuAcccassFlags, name);
        Silk.NET.DXGI.Format format = sizeof(T) == 2 ? Silk.NET.DXGI.Format.FormatR16Uint : Silk.NET.DXGI.Format.FormatR32Uint;
        return new IndexBuffer(buffer, format);
    }

    public unsafe IndexBuffer CreateIndexBuffer<T>(uint nbElements, Usage usage = Usage.Default, CpuAccessFlag cpuAcccassFlags = CpuAccessFlag.None, string name = "") where T : unmanaged
    {
        GraphicBuffer buffer = CreateEmptyBuffer<T>(BindFlag.IndexBuffer, nbElements, usage, cpuAcccassFlags, name);
        Silk.NET.DXGI.Format format = sizeof(T) == 2 ? Silk.NET.DXGI.Format.FormatR16Uint : Silk.NET.DXGI.Format.FormatR32Uint;
        return new IndexBuffer(buffer, format);
    }

    public ConstantBuffer CreateConstantBuffer<T>(Usage usage = Usage.Default, CpuAccessFlag cpuAcccessFlags = CpuAccessFlag.None, string name = "") where T : unmanaged
    {
        GraphicBuffer buffer = CreateEmptyBuffer<T>(BindFlag.ConstantBuffer, nbElements: 1, usage, cpuAcccessFlags, name);
        return new ConstantBuffer(buffer);
    }

    private unsafe GraphicBuffer CreateEmptyBuffer<T>(BindFlag bindFlag, uint nbElements, Usage usage, CpuAccessFlag cpuAcccessFlags, string name = "") where T : unmanaged
    {
        uint stride = (uint)Marshal.SizeOf<T>();
        BufferDesc bufferDesc = new()
        {
            ByteWidth = stride * nbElements,
            Usage = usage,
            BindFlags = (uint)bindFlag,
            CPUAccessFlags = (uint)cpuAcccessFlags,
            StructureByteStride = stride,
            MiscFlags = 0
        };
        ComPtr<ID3D11Buffer> bufferData = CreateBufferDX11<T>(in bufferDesc, null, name);
        return new GraphicBuffer(_graphicDevice, bufferData, bindFlag, nbElements, stride, name);
    }

    private unsafe GraphicBuffer CreateBuffer<T>(BindFlag bindFlag, T[] data, Usage usage, CpuAccessFlag cpuAcccessFlags, string name = "") where T : unmanaged
    {
        uint stride = (uint)Marshal.SizeOf<T>();
        uint nbElements = (uint)data.Length;
        BufferDesc bufferDesc = new()
        {
            ByteWidth = stride * nbElements,
            Usage = usage,
            BindFlags = (uint)bindFlag,
            CPUAccessFlags = (uint)cpuAcccessFlags,
            StructureByteStride = stride,
            MiscFlags = 0
        };
        ComPtr<ID3D11Buffer> bufferData = CreateBufferDX11(in bufferDesc, data, name);
        return new GraphicBuffer(_graphicDevice, bufferData, bindFlag, nbElements, stride, name);
    }

    private unsafe ComPtr<ID3D11Buffer> CreateBufferDX11<T>(in BufferDesc bufferDesc, T[]? data, string name = "") where T : unmanaged
    {
        ComPtr<ID3D11Buffer> buffer = default;
        if (data is null)
        {
            SilkMarshal.ThrowHResult(_graphicDevice.Device.CreateBuffer(in bufferDesc, ref Unsafe.NullRef<SubresourceData>(), ref buffer));
        }
        else
        {
            fixed (T* indexData = data)
            {
                SubresourceData subresourceData = new()
                {
                    PSysMem = indexData
                };

                SilkMarshal.ThrowHResult(_graphicDevice.Device.CreateBuffer(in bufferDesc, in subresourceData, ref buffer));
            }
        }
        if (!string.IsNullOrEmpty(name))
        {
            // Set Debug Name
            using (GlobalMemory unmanagedName = SilkMarshal.StringToMemory(name, NativeStringEncoding.Ansi))
            {
                nint namePtr = unmanagedName.Handle;
                fixed (Guid* guidPtr = &Windows.Win32.PInvoke.WKPDID_D3DDebugObjectName)
                {
                    buffer.SetPrivateData(guidPtr, (uint)name.Length, namePtr.ToPointer());
                }
            }
        }
        return buffer;
    }
}
