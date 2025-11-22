using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D.Graphics;

internal sealed class TextureFactory
{
    private readonly ComPtr<ID3D11Device> _device;

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
        Texture2DDesc textureDesc = CreateDefaultTextureDesc((uint)width, (uint)height);

        SubresourceData initialData = new()
        {
            PSysMem = pixels,
            SysMemPitch = (uint)(width * bytesPerPixel),
            SysMemSlicePitch = (uint)(width * bytesPerPixel * height)
        };

        // Create a view of the texture for the shader.
        ShaderResourceViewDesc srvDesc = CreateDefaultShaderResourceViewDesc(textureDesc);

        TextureBuilder textureBuilder = new(this, textureDesc);
        return textureBuilder
            .InitializeWith(initialData)
            .WithName(name)
            .WithShaderRessourceView(srvDesc)
            .Build();
    }

    public TextureBuilder CreateBuilder(Texture2DDesc textureDesc)
    {
        return new TextureBuilder(this, textureDesc);
    }

    public unsafe Texture CreateEmpty(string name, int width, int height)
    {
        Texture2DDesc textureDesc = CreateDefaultTextureDesc((uint)width, (uint)height);
        return CreateEmpty(name, width, height, in textureDesc);
    }

    public unsafe Texture CreateEmpty(string name, int width, int height, in Texture2DDesc textureDesc)
    {
        ShaderResourceViewDesc srvDesc = CreateDefaultShaderResourceViewDesc(textureDesc);
        return CreateEmpty(name, width, height, in textureDesc, in srvDesc);
    }

    public unsafe Texture CreateEmpty(string name, int width, int height, in Texture2DDesc textureDesc, in ShaderResourceViewDesc shaderResourceViewDesc)
    {
        TextureBuilder textureBuilder = new(this, textureDesc);
        return textureBuilder
            .WithName(name)
            .WithShaderRessourceView(shaderResourceViewDesc)
            .Build();
    }

    public ComPtr<ID3D11Texture2D> CreateTexture2D(in Texture2DDesc pDesc, in SubresourceData pInitialData)
    {
        ComPtr<ID3D11Texture2D> texture2D = default;
        SilkMarshal.ThrowHResult(
            _device.CreateTexture2D(in pDesc, in pInitialData, ref texture2D)
        );
        return texture2D;
    }

    public ComPtr<ID3D11Texture2D> CreateEmptyTexture2D(in Texture2DDesc pDesc)
    {
        ComPtr<ID3D11Texture2D> texture2D = default;
        SilkMarshal.ThrowHResult(
            _device.CreateTexture2D(in pDesc, in Unsafe.NullRef<SubresourceData>(), ref texture2D)
        );
        return texture2D;
    }

    public ComPtr<ID3D11ShaderResourceView> CreateShaderResourceView(ComPtr<ID3D11Resource> pResource, in ShaderResourceViewDesc pDesc)
    {
        ComPtr<ID3D11ShaderResourceView> textureView = default;
        SilkMarshal.ThrowHResult
            (
                _device.CreateShaderResourceView
                (
                    pResource,
                    in pDesc,
                    ref textureView
                )
            );
        return textureView;
    }

    public ComPtr<ID3D11ShaderResourceView> CreateShaderResourceView(ComPtr<ID3D11Texture2D> pResource, in ShaderResourceViewDesc pDesc)
    {
        ComPtr<ID3D11ShaderResourceView> textureView = default;
        SilkMarshal.ThrowHResult
            (
                _device.CreateShaderResourceView
                (
                    pResource,
                    in pDesc,
                    ref textureView
                )
            );
        return textureView;
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
                nint namePtr = unmanagedName.Handle;
                fixed (Guid* guidPtr = &Windows.Win32.PInvoke.WKPDID_D3DDebugObjectName)
                {
                    sampler.SetPrivateData(guidPtr, (uint)name.Length, namePtr.ToPointer());
                }
            }
        }
        return sampler;
    }

    public ComPtr<ID3D11DepthStencilView> CreateDepthStencilView(ComPtr<ID3D11Texture2D> pResource, in DepthStencilViewDesc pDesc)
    {
        ComPtr<ID3D11DepthStencilView> depthStencilView = default;
        SilkMarshal.ThrowHResult(
            _device.CreateDepthStencilView(pResource, in pDesc, ref depthStencilView)
        );
        return depthStencilView;
    }

    private static Texture2DDesc CreateDefaultTextureDesc(uint width, uint height)
    {
        return new Texture2DDesc()
        {
            Width = width,
            Height = height,
            Format = Format.FormatB8G8R8A8Unorm,
            MipLevels = 1,
            BindFlags = (uint)BindFlag.ShaderResource,
            Usage = Usage.Default,
            CPUAccessFlags = 0,
            MiscFlags = (uint)ResourceMiscFlag.None,
            SampleDesc = new SampleDesc(1, 0),
            ArraySize = 1
        };
    }

    private static ShaderResourceViewDesc CreateDefaultShaderResourceViewDesc(Texture2DDesc textureDesc)
    {
        return new ShaderResourceViewDesc()
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
    }
}