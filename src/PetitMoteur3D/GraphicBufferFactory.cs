using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D
{
    internal class GraphicBufferFactory
    {
        private readonly ComPtr<ID3D11Device> _device;

        public GraphicBufferFactory(ComPtr<ID3D11Device> device)
        {
            _device = device;
        }

        public unsafe ComPtr<ID3D11Buffer> CreateVertexBuffer<T>(T[] data, Usage usage = Usage.Default, CpuAccessFlag cpuAcccassFlags = CpuAccessFlag.None, string name = "") where T : unmanaged
        {
            BufferDesc bufferDesc = new()
            {
                ByteWidth = (uint)(data.Length * Marshal.SizeOf<T>()),
                Usage = usage,
                BindFlags = (uint)BindFlag.VertexBuffer,
                CPUAccessFlags = (uint)cpuAcccassFlags,
                MiscFlags = 0
            };

            return CreateBuffer(bufferDesc, data, name);
        }

        public unsafe ComPtr<ID3D11Buffer> CreateVertexBuffer<T>(uint nbElements, Usage usage = Usage.Default, CpuAccessFlag cpuAcccassFlags = CpuAccessFlag.None, string name = "") where T : unmanaged
        {
            BufferDesc bufferDesc = new()
            {
                ByteWidth = (uint)(nbElements * Marshal.SizeOf<T>()),
                Usage = usage,
                BindFlags = (uint)BindFlag.VertexBuffer,
                CPUAccessFlags = (uint)cpuAcccassFlags,
                MiscFlags = 0
            };

            return CreateBuffer(bufferDesc, name);
        }

        public unsafe ComPtr<ID3D11Buffer> CreateIndexBuffer<T>(T[] data, Usage usage = Usage.Default, CpuAccessFlag cpuAcccassFlags = CpuAccessFlag.None, string name = "") where T : unmanaged
        {
            BufferDesc bufferDesc = new()
            {
                ByteWidth = (uint)(data.Length * Marshal.SizeOf<T>()),
                Usage = usage,
                BindFlags = (uint)BindFlag.IndexBuffer,
                CPUAccessFlags = (uint)cpuAcccassFlags,
                StructureByteStride = (uint)(Marshal.SizeOf<T>())
            };
            return CreateBuffer(bufferDesc, data, name);
        }

        public unsafe ComPtr<ID3D11Buffer> CreateIndexBuffer<T>(uint nbElements, Usage usage = Usage.Default, CpuAccessFlag cpuAcccassFlags = CpuAccessFlag.None, string name = "") where T : unmanaged
        {
            BufferDesc bufferDesc = new()
            {
                ByteWidth = (uint)(nbElements * Marshal.SizeOf<T>()),
                Usage = usage,
                BindFlags = (uint)BindFlag.IndexBuffer,
                CPUAccessFlags = (uint)cpuAcccassFlags,
                StructureByteStride = (uint)(Marshal.SizeOf<T>())
            };
            return CreateBuffer(bufferDesc, name);
        }

        public unsafe ComPtr<ID3D11Buffer> CreateConstantBuffer<T>(Usage usage = Usage.Default, CpuAccessFlag cpuAcccessFlags = CpuAccessFlag.None, string name = "") where T : unmanaged
        {
            BufferDesc bufferDesc = new()
            {
                ByteWidth = (uint)(Marshal.SizeOf<T>(default)),
                Usage = usage,
                BindFlags = (uint)BindFlag.ConstantBuffer,
                CPUAccessFlags = (uint)cpuAcccessFlags,
                MiscFlags = 0
            };

            return CreateBuffer(bufferDesc, name);
        }

        private unsafe ComPtr<ID3D11Buffer> CreateBuffer(BufferDesc bufferDesc, string name = "") => CreateBuffer<byte>(bufferDesc, null, name);
        private unsafe ComPtr<ID3D11Buffer> CreateBuffer<T>(BufferDesc bufferDesc, T[]? data, string name = "") where T : unmanaged
        {
            ComPtr<ID3D11Buffer> buffer = default;
            if (data is null)
            {
                SilkMarshal.ThrowHResult(_device.CreateBuffer(in bufferDesc, ref Unsafe.NullRef<SubresourceData>(), ref buffer));
            }
            else
            {
                fixed (T* indexData = data)
                {
                    SubresourceData subresourceData = new()
                    {
                        PSysMem = indexData
                    };

                    SilkMarshal.ThrowHResult(_device.CreateBuffer(in bufferDesc, in subresourceData, ref buffer));
                }
            }
            if (!string.IsNullOrEmpty(name))
            {
                // Set Debug Name
                using (GlobalMemory unmanagedName = SilkMarshal.StringToMemory(name, NativeStringEncoding.Ansi))
                {
                    IntPtr namePtr = unmanagedName.Handle;
                    fixed (Guid* guidPtr = &Windows.Win32.PInvoke.WKPDID_D3DDebugObjectName)
                    {
                        buffer.SetPrivateData(guidPtr, (uint)name.Length, namePtr.ToPointer());
                    }
                }
            }
            return buffer;
        }
    }
}
