using System;
using System.Buffers;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D
{
    internal class Texture : IDisposable
    {
        public string FileName { get; private set; }
        public ComPtr<ID3D11ShaderResourceView> TextureView { get { return _textureView; } }
        private readonly ComPtr<ID3D11ShaderResourceView> _textureView;

        public unsafe Texture(string fileName, ComPtr<ID3D11Device> device)
        {
            FileName = fileName;
            // Load the image using any applicable library.
            SixLabors.ImageSharp.Formats.DecoderOptions decoderOptions = new();
            SixLabors.ImageSharp.Configuration customConfig = decoderOptions.Configuration;
            customConfig.PreferContiguousImageBuffers = true;
            using SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Bgra32> imgBmp = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Bgra32>(
                decoderOptions, fileName);

            Texture2DDesc textureDesc = new()
            {
                Width = (uint)imgBmp.Width,
                Height = (uint)imgBmp.Height,
                Format = Format.FormatB8G8R8A8Unorm,
                MipLevels = 1,
                BindFlags = (uint)BindFlag.ShaderResource,
                Usage = Usage.Default,
                CPUAccessFlags = 0,
                MiscFlags = (uint)ResourceMiscFlag.None,
                SampleDesc = new SampleDesc(1, 0),
                ArraySize = 1
            };

            if (!imgBmp.DangerousTryGetSinglePixelMemory(out Memory<SixLabors.ImageSharp.PixelFormats.Bgra32> bmpData))
            {
                // Copy pixel data row-by-row, as a contiguous block is not available.
                int size = imgBmp.Width * imgBmp.Height;
                SixLabors.ImageSharp.PixelFormats.Bgra32[] colDat = new SixLabors.ImageSharp.PixelFormats.Bgra32[size * 4];
                imgBmp.CopyPixelDataTo(colDat.AsSpan());
                bmpData = new Memory<SixLabors.ImageSharp.PixelFormats.Bgra32>(colDat);
            }

            ComPtr<ID3D11Texture2D> texture = default;

            using (MemoryHandle bitmapData = bmpData.Pin())
            {
                SubresourceData subresourceData = new()
                {
                    PSysMem = bitmapData.Pointer,
                    SysMemPitch = (uint)imgBmp.Width * sizeof(int),
                    SysMemSlicePitch = (uint)(imgBmp.Width * sizeof(int) * imgBmp.Height)
                };

                SilkMarshal.ThrowHResult
                (
                    device.CreateTexture2D
                    (
                        in textureDesc,
                        in subresourceData,
                        ref texture
                    )
                );
            }

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
                device.CreateShaderResourceView
                (
                    texture,
                    in srvDesc,
                    ref _textureView
                )
            );
        }

        ~Texture()
        {
            Dispose(disposing: false);
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
                    _textureView.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}