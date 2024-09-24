using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using System;
using System.Runtime.CompilerServices;

namespace PetitMoteur3D
{
    internal class DeviceD3D11
    {
        public ComPtr<ID3D11Device> Device { get { return _device; } }
        public ComPtr<ID3D11DeviceContext> DeviceContext { get { return _deviceContext; } }
        public ComPtr<IDXGISwapChain1> Swapchain { get { return _swapchain; } }
        public ComPtr<ID3D11RenderTargetView> RenderTargetView { get { return _renderTargetView; } }
        public D3DCompiler ShaderCompiler { get { return _compiler; } }

        private ComPtr<ID3D11Device> _device;
        private ComPtr<ID3D11DeviceContext> _deviceContext;
        private ComPtr<IDXGISwapChain1> _swapchain;
        private ComPtr<ID3D11RenderTargetView> _renderTargetView;
        private D3DCompiler _compiler;

        private readonly float[] _backgroundColour = new[] { 0.0f, 0.5f, 0.0f, 1.0f };

        public unsafe DeviceD3D11(Silk.NET.Windowing.IWindow window)
        {
            //Whether or not to force use of DXVK on platforms where native DirectX implementations are available
            const bool forceDxvk = false;

            DXGI dxgi = DXGI.GetApi(window, forceDxvk);
            D3D11 d3d11 = D3D11.GetApi(window, forceDxvk);
            _compiler = D3DCompiler.GetApi();

            uint createDeviceFlags = 0;
#if DEBUG
            createDeviceFlags |= (uint)CreateDeviceFlag.Debug;
#endif

            // Create our D3D11 logical device.
            SilkMarshal.ThrowHResult
            (
                d3d11.CreateDevice
                (
                    default(ComPtr<IDXGIAdapter>),
                    D3DDriverType.Hardware,
                    default,
                    createDeviceFlags,
                    null,
                    0,
                    D3D11.SdkVersion,
                    ref _device,
                    null,
                    ref _deviceContext
                )
            );

            //This is not supported under DXVK 
            //TODO: PR a stub into DXVK for this maybe?
            if (OperatingSystem.IsWindows())
            {
                // Log debug messages for this device (given that we've enabled the debug flag). Don't do this in release code!
                _device.SetInfoQueueCallback(msg => Console.WriteLine(SilkMarshal.PtrToString((nint)msg.PDescription)));
            }

            // Create our swapchain description.
            SwapChainDesc1 swapChainDesc = new SwapChainDesc1
            {
                BufferCount = 1,
                Format = Format.FormatB8G8R8A8Unorm,
                BufferUsage = DXGI.UsageRenderTargetOutput,
                SampleDesc = new SampleDesc(1, 0)
            };

            // Create our DXGI factory to allow us to create a swapchain. 
            ComPtr<IDXGIFactory2> factory = dxgi.CreateDXGIFactory<IDXGIFactory2>();

            // Create the swapchain.
            SilkMarshal.ThrowHResult
            (
                factory.CreateSwapChainForHwnd
                (
                    _device,
                    window.Native!.DXHandle!.Value,
                    in swapChainDesc,
                    null,
                    ref Unsafe.NullRef<IDXGIOutput>(),
                    ref _swapchain
                )
            );

            // Create « render target view » 
            // Obtain the framebuffer for the swapchain's backbuffer.
            using (ComPtr<ID3D11Texture2D> framebuffer = _swapchain.GetBuffer<ID3D11Texture2D>(0))
            {
                SilkMarshal.ThrowHResult(_device.CreateRenderTargetView(framebuffer, null, ref _renderTargetView));
            }

            // Tell the output merger about our render target view.
            _deviceContext.OMSetRenderTargets(1, ref _renderTargetView, ref Unsafe.NullRef<ID3D11DepthStencilView>());

            // Set the rasterizer state with the current viewport.
            Viewport viewport = new(0, 0, window.FramebufferSize.X, window.FramebufferSize.Y, 0, 1);
            _deviceContext.RSSetViewports(1, in viewport);
        }

        unsafe ~DeviceD3D11()
        {
            if (_deviceContext.Handle is not null) { 
                _deviceContext.ClearState(); 
            }

            _renderTargetView.Release();
            _deviceContext.Release();
            _swapchain.Release();
            _device.Release();
            _compiler.Dispose();
        }

        public void Resize(Vector2D<int> size)
        {
            SilkMarshal.ThrowHResult(
                _swapchain.ResizeBuffers(0, (uint)size.X, (uint)size.Y, Format.FormatB8G8R8A8Unorm, 0)
            );
        }

        public void Clear()
        {
            _deviceContext.ClearRenderTargetView(_renderTargetView, ref _backgroundColour[0]);
        }
    }
}
