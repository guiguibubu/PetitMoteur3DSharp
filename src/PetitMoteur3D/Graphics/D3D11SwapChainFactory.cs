using System;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D.Graphics;

internal class D3D11SwapChainFactory
{
    private readonly ComPtr<ID3D11Device> _device;
    private readonly TextureManager _textureManager;
    private readonly bool _dxVk;

    public D3D11SwapChainFactory(D3D11GraphicDevice graphicDevice, TextureManager textureManager)
    {
        _device = graphicDevice.Device;
        _textureManager = textureManager;
        _dxVk = graphicDevice.DxVk;
    }

    public D3D11SwapChain CreateSwapChainForHwnd(IntPtr windowPtr, in SwapChainDesc1 swapChainDesc, in SwapChainFullscreenDesc swapChainFullscreenDesc, ref IDXGIOutput pRestrictToOutput)
    {
        // Create our DXGI factory to allow us to create a swapchain. 
#pragma warning disable CS0618 // Type or member is obsolete
        using DXGI dxgi = DXGI.GetApi(DXSwapchainProvider.Win32, _dxVk);
#pragma warning restore CS0618 // Type or member is obsolete
        using ComPtr<IDXGIFactory2> factory = dxgi.CreateDXGIFactory<IDXGIFactory2>();

        ComPtr<IDXGISwapChain1> swapChain = default;
        SilkMarshal.ThrowHResult
        (
            factory.CreateSwapChainForHwnd
            (
                _device,
                windowPtr,
                in swapChainDesc,
                in swapChainFullscreenDesc,
                ref pRestrictToOutput,
                ref swapChain
            )
        );

        return new D3D11SwapChain(swapChain, _textureManager);
    }

    public D3D11SwapChain CreateSwapChainForComposition(in SwapChainDesc1 swapChainDesc, ref IDXGIOutput pRestrictToOutputs)
    {
        // Create our DXGI factory to allow us to create a swapchain. 
#pragma warning disable CS0618 // Type or member is obsolete
        using DXGI dxgi = DXGI.GetApi(DXSwapchainProvider.Win32, _dxVk);
#pragma warning restore CS0618 // Type or member is obsolete
        using ComPtr<IDXGIFactory2> factory = dxgi.CreateDXGIFactory<IDXGIFactory2>();

        ComPtr<IDXGISwapChain1> swapChain = default;

        SilkMarshal.ThrowHResult
        (
            factory.CreateSwapChainForComposition
            (
                _device,
                in swapChainDesc,
                ref Unsafe.NullRef<IDXGIOutput>(),
                ref swapChain
            )
        );

        return new D3D11SwapChain(swapChain, _textureManager);
    }
}
