using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D
{
    internal class TextureFactory
    {
        private ComPtr<ID3D11Device> _device;

        public TextureFactory(ComPtr<ID3D11Device> device)
        {
            _device = device;
        }

        public unsafe Texture FromFile(string fileName)
        {
            // Load the image using any applicable library.
            SixLabors.ImageSharp.Formats.DecoderOptions decoderOptions = new();
            SixLabors.ImageSharp.Configuration customConfig = decoderOptions.Configuration;
            customConfig.PreferContiguousImageBuffers = true;
            using SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Bgra32> imgBmp = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Bgra32>(
                decoderOptions, fileName);

            int imgageWidth = imgBmp.Width;
            int imgageHeigth = imgBmp.Height;
            int bytesPerPixel = Marshal.SizeOf<SixLabors.ImageSharp.PixelFormats.Bgra32>();

            if (!imgBmp.DangerousTryGetSinglePixelMemory(out Memory<SixLabors.ImageSharp.PixelFormats.Bgra32> bmpData))
            {
                // Copy pixel data row-by-row, as a contiguous block is not available.
                int size = imgageWidth * imgageHeigth;
                SixLabors.ImageSharp.PixelFormats.Bgra32[] colDat = new SixLabors.ImageSharp.PixelFormats.Bgra32[size * 4];
                imgBmp.CopyPixelDataTo(colDat.AsSpan());
                bmpData = new Memory<SixLabors.ImageSharp.PixelFormats.Bgra32>(colDat);
            }

            Texture texture;

            using (MemoryHandle bitmapData = bmpData.Pin())
            {
                texture = Create(fileName, bitmapData.Pointer, imgageWidth, imgageHeigth, bytesPerPixel);
            }

            return texture;
        }

        public unsafe Texture Create(string name, nint pixels, int width, int height, int bytesPerPixel) => Create(name, pixels.ToPointer(), width, height, bytesPerPixel);

        public unsafe Texture Create(string name, void* pixels, int width, int height, int bytesPerPixel)
        {
            Texture2DDesc textureDesc = new()
            {
                Width = (uint)width,
                Height = (uint)height,
                Format = Format.FormatB8G8R8A8Unorm,
                MipLevels = 1,
                BindFlags = (uint)BindFlag.ShaderResource,
                Usage = Usage.Default,
                CPUAccessFlags = 0,
                MiscFlags = (uint)ResourceMiscFlag.None,
                SampleDesc = new SampleDesc(1, 0),
                ArraySize = 1
            };

            ComPtr<ID3D11Texture2D> texture = default;
            ComPtr<ID3D11ShaderResourceView> textureView = default;
            try
            {
                SubresourceData subresourceData = new()
                {
                    PSysMem = pixels,
                    SysMemPitch = (uint)(width * Marshal.SizeOf<SixLabors.ImageSharp.PixelFormats.Bgra32>()),
                    SysMemSlicePitch = (uint)(width * Marshal.SizeOf<SixLabors.ImageSharp.PixelFormats.Bgra32>() * height)
                };

                SilkMarshal.ThrowHResult
                (
                    _device.CreateTexture2D
                    (
                        in textureDesc,
                        in subresourceData,
                        ref texture
                    )
                );

                // Create a view of the texture for the shader.
                ShaderResourceViewDesc srvDesc = new()
                {
                    Format = textureDesc.Format,
                    ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2D,
                    Anonymous = new ShaderResourceViewDescUnion
                    {
                        Texture2D =
                        {
                            MostDetailedMip = 0,
                            MipLevels = 1
                        }
                    }
                };


                SilkMarshal.ThrowHResult
                (
                    _device.CreateShaderResourceView
                    (
                        texture,
                        in srvDesc,
                        ref textureView
                    )
                );
            }
            finally
            {
                texture.Dispose();
            }

            return new Texture(name, width, height, textureView);
        }

        public unsafe ComPtr<ID3D11SamplerState> CreateSampler(SamplerDesc desc, string? name = null)
        {
            ComPtr<ID3D11SamplerState> sampler = default;
            SilkMarshal.ThrowHResult(_device.CreateSamplerState(ref desc, ref sampler));
            if (!string.IsNullOrEmpty(name))
            {
                // Set Debug Name
                using (GlobalMemory unmanagedName = SilkMarshal.StringToMemory(name, NativeStringEncoding.Ansi))
                {
                    IntPtr namePtr = unmanagedName.Handle;
                    fixed (Guid* guidPtr = &Windows.Win32.PInvoke.WKPDID_D3DDebugObjectName)
                    {
                        sampler.SetPrivateData(guidPtr, (uint)name.Length, namePtr.ToPointer());
                    }
                }
            }
            return sampler;
        }
    }
}