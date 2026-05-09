using System;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D.Graphics;

internal sealed class D3D11SwapChain : IDisposable
{
    public uint Width => _description.Width;
    public uint Height => _description.Height;
    public Format Format => _description.Format;
    public bool Stereo => _description.Stereo;
    public SampleDesc SampleDesc => _description.SampleDesc;
    public uint BufferUsage => _description.BufferUsage;
    public uint BufferCount => _description.BufferCount;
    public Scaling Scaling => _description.Scaling;
    public SwapEffect SwapEffect => _description.SwapEffect;
    public AlphaMode AlphaMode => _description.AlphaMode;
    public uint Flags => _description.Flags;

    public Texture BackBuffer => _backBufferTexture;

    public ComPtr<IDXGISwapChain1> NativeHandle => _nativeHandle;

    private ComPtr<IDXGISwapChain1> _nativeHandle;
    private SwapChainDesc1 _description;
    private Texture _backBufferTexture;

    private readonly TextureManager _textureManager;

    private const uint NbMaxBuffers = Windows.Win32.PInvoke.DXGI_MAX_SWAP_CHAIN_BUFFERS;

    private bool _disposed;

    public D3D11SwapChain(ComPtr<IDXGISwapChain1> handle, TextureManager textureManager)
    {
        _nativeHandle = handle;
        _textureManager = textureManager;
        _description = new SwapChainDesc1();
        UpdateDesc();
        _backBufferTexture = InitBackBuffer();
        _disposed = false;
    }

    public void Present(uint syncInterval, uint flags)
    {
        SilkMarshal.ThrowHResult(
            _nativeHandle.Present(syncInterval, flags)
        );
    }

    public void ResizeBuffers(uint bufferCount, uint width, uint height, Format newFormat, uint flags)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bufferCount, NbMaxBuffers, nameof(bufferCount));
        SilkMarshal.ThrowHResult(
            _nativeHandle.ResizeBuffers(bufferCount, width, height, newFormat, flags)
        );
        UpdateDesc();
        UpdateBackBuffer();
    }

    public void ResizeBuffers(uint width, uint height)
    {
        //  According  to  documentation https://learn.microsoft.com/en-us/windows/win32/api/dxgi/nf-dxgi-idxgiswapchain-resizebuffers#remarks
        // You can't resize a swap chain unless you release all outstanding references to its back buffers. You must release all of its direct and indirect references on the back buffers in order for ResizeBuffers to succeed. 
        _backBufferTexture.ReleaseViews();
        ResizeBuffers(0, width, height, Format.FormatUnknown, 0);
    }

    private void UpdateDesc()
    {
        _nativeHandle.GetDesc1(ref _description);
    }

    private Texture InitBackBuffer()
    {
        // Create « render target view » 
        // Obtain the framebuffer for the swapchain's backbuffer.
        Texture texture;
        using (ComPtr<ID3D11Texture2D> framebuffer = _nativeHandle.GetBuffer<ID3D11Texture2D>(0))
        {
            texture = _textureManager.Factory
                .CreateBuilder(new D3D11Texture2D(framebuffer, false, "SwapChain_BackBuffer_Texture"))
                .WithRenderTargetView()
                .WithName("SwapChain_BackBuffer")
                .Build();
        }
        return texture;
    }

    private void UpdateBackBuffer()
    {
        // Create « render target view » 
        // Obtain the framebuffer for the swapchain's backbuffer.
        using (ComPtr<ID3D11Texture2D> framebuffer = _nativeHandle.GetBuffer<ID3D11Texture2D>(0))
        {
            _backBufferTexture.SetTextureRessource(framebuffer);
            _textureManager.Factory.UpdateViews(_backBufferTexture);
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            // passer en mode fenêtré
            _backBufferTexture.Dispose();
            _nativeHandle.SetFullscreenState(false, ref Unsafe.NullRef<IDXGIOutput>());
            _nativeHandle.Dispose();
            _disposed = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~D3D11SwapChain()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
