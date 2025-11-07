using System;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D.Graphics;

internal sealed class GraphicDeviceRessourceFactory
{
    public GraphicBufferFactory BufferFactory => _bufferFactory;
    public ShaderManager ShaderManager => _shaderManager;
    public TextureManager TextureManager => _textureManager;

    private readonly GraphicBufferFactory _bufferFactory;
    private readonly ShaderManager _shaderManager;
    private readonly TextureManager _textureManager;
    private readonly D3D11GraphicDevice _graphicDevice;

    public GraphicDeviceRessourceFactory(D3D11GraphicDevice graphicDevice)
        : this(graphicDevice, new GraphicBufferFactory(graphicDevice.Device), new ShaderManager(graphicDevice.Device), new TextureManager(graphicDevice.Device))
    { }

    public GraphicDeviceRessourceFactory(D3D11GraphicDevice graphicDevice, GraphicBufferFactory bufferFactory, ShaderManager shaderManager, TextureManager textureManager)
    {
        _bufferFactory = bufferFactory;
        _shaderManager = shaderManager;
        _textureManager = textureManager;
        _graphicDevice = graphicDevice;
    }

    public ComPtr<IDXGISwapChain1> CreateSwapChainForHwnd(IntPtr windowPtr, in SwapChainDesc1 swapChainDesc, in SwapChainFullscreenDesc swapChainFullscreenDesc, ref IDXGIOutput pRestrictToOutput)
    {
        // Create our DXGI factory to allow us to create a swapchain. 
#pragma warning disable CS0618 // Type or member is obsolete
        using DXGI dxgi = DXGI.GetApi(DXSwapchainProvider.Win32, _graphicDevice.DxVk);
#pragma warning restore CS0618 // Type or member is obsolete
        using ComPtr<IDXGIFactory2> factory = dxgi.CreateDXGIFactory<IDXGIFactory2>();

        ComPtr<IDXGISwapChain1> swapChain = default;
        SilkMarshal.ThrowHResult
        (
            factory.CreateSwapChainForHwnd
            (
                _graphicDevice.Device,
                windowPtr,
                in swapChainDesc,
                in swapChainFullscreenDesc,
                ref Unsafe.NullRef<IDXGIOutput>(),
                ref swapChain
            )
        );

        return swapChain;
    }

    public ComPtr<IDXGISwapChain1> CreateSwapChainForComposition(in SwapChainDesc1 swapChainDesc, ref IDXGIOutput pRestrictToOutputs)
    {
        // Create our DXGI factory to allow us to create a swapchain. 
#pragma warning disable CS0618 // Type or member is obsolete
        using DXGI dxgi = DXGI.GetApi(DXSwapchainProvider.Win32, _graphicDevice.DxVk);
#pragma warning restore CS0618 // Type or member is obsolete
        using ComPtr<IDXGIFactory2> factory = dxgi.CreateDXGIFactory<IDXGIFactory2>();

        ComPtr<IDXGISwapChain1> swapChain = default;

        SilkMarshal.ThrowHResult
        (
            factory.CreateSwapChainForComposition
            (
                _graphicDevice.Device,
                in swapChainDesc,
                ref Unsafe.NullRef<IDXGIOutput>(),
                ref swapChain
            )
        );

        return swapChain;
    }

    public ComPtr<ID3D11RenderTargetView> CreateRenderTargetView(ComPtr<ID3D11Texture2D> pResource, in RenderTargetViewDesc pDesc)
    {
        ComPtr<ID3D11RenderTargetView> renderTargetView = default;
        SilkMarshal.ThrowHResult(
            _graphicDevice.Device.CreateRenderTargetView(pResource, ref Unsafe.NullRef<RenderTargetViewDesc>(), ref renderTargetView)
        );
        return renderTargetView;
    }

    public ComPtr<ID3D11Texture2D> CreateTexture2D(in Texture2DDesc pDesc, in SubresourceData pInitialData)
    {
        ComPtr<ID3D11Texture2D> texture2D = default;
        SilkMarshal.ThrowHResult(
            _graphicDevice.Device.CreateTexture2D(in pDesc, ref Unsafe.NullRef<SubresourceData>(), ref texture2D)
        );
        return texture2D;
    }

    public ComPtr<ID3D11DepthStencilView> CreateDepthStencilView(ComPtr<ID3D11Texture2D> pResource, in DepthStencilViewDesc pDesc)
    {
        ComPtr<ID3D11DepthStencilView> depthStencilView = default;
        SilkMarshal.ThrowHResult(
            _graphicDevice.Device.CreateDepthStencilView(pResource, in pDesc, ref depthStencilView)
        );
        return depthStencilView;
    }
}
