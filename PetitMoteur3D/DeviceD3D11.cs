using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using System;
using System.Runtime.CompilerServices;
using Evergine.Bindings.RenderDoc;
using System.IO;
using Silk.NET.Windowing;

namespace PetitMoteur3D
{
    internal class DeviceD3D11
    {
        public ComPtr<ID3D11Device> Device { get { return _device; } }
        public ComPtr<ID3D11DeviceContext> DeviceContext { get { return _deviceContext; } }
        public ComPtr<IDXGISwapChain1> Swapchain { get { return _swapchain; } }
        public ComPtr<ID3D11RenderTargetView> RenderTargetView { get { return _renderTargetView; } }
        public ComPtr<ID3D11DepthStencilView> DepthStencilView { get { return _depthStencilView; } }
        public D3DCompiler ShaderCompiler { get { return _compiler; } }

        private ComPtr<ID3D11Device> _device;
        private ComPtr<ID3D11DeviceContext> _deviceContext;
        private ComPtr<IDXGISwapChain1> _swapchain;
        private ComPtr<ID3D11RenderTargetView> _renderTargetView;
        private ComPtr<ID3D11DepthStencilView> _depthStencilView;
        private ComPtr<ID3D11Texture2D> _depthTexture;
        private D3DCompiler _compiler;

        private Silk.NET.Windowing.IWindow _window;

        private readonly float[] _backgroundColour = new[] { 0.0f, 0.5f, 0.0f, 1.0f };

        private readonly RenderDoc _renderDoc;

        public unsafe DeviceD3D11(Silk.NET.Windowing.IWindow window)
        {
            _window = window;

            //Whether or not to force use of DXVK on platforms where native DirectX implementations are available
            const bool forceDxvk = false;

            _compiler = D3DCompiler.GetApi();

            uint createDeviceFlags = 0;
#if DEBUG
            createDeviceFlags |= (uint)CreateDeviceFlag.Debug;
#endif

            // Create our D3D11 logical device.
            D3D11 d3d11 = D3D11.GetApi(_window, forceDxvk);

            D3DFeatureLevel[] featureLevels = {
                D3DFeatureLevel.Level111,
                D3DFeatureLevel.Level110
            };
            SilkMarshal.ThrowHResult
            (
                d3d11.CreateDevice
                (
                    default(ComPtr<IDXGIAdapter>),
                    D3DDriverType.Hardware,
                    default,
                    createDeviceFlags,
                    in featureLevels[0],
                    2,
                    D3D11.SdkVersion,
                    ref _device,
                    null,
                    ref _deviceContext
                )
            );
            d3d11.Dispose();

            //This is not supported under DXVK 
            //TODO: PR a stub into DXVK for this maybe?
            if (OperatingSystem.IsWindows())
            {
                // Log debug messages for this device (given that we've enabled the debug flag). Don't do this in release code!
                _device.SetInfoQueueCallback(msg => Console.WriteLine(SilkMarshal.PtrToString((nint)msg.PDescription)));
            }

            // Create our swapchain description.
            SwapChainDesc1 swapChainDesc = new()
            {
                Width = (uint)window.Size.X,
                Height = (uint)window.Size.Y,
                BufferCount = 1,
                Format = Format.FormatB8G8R8A8Unorm,
                BufferUsage = DXGI.UsageRenderTargetOutput,
                SampleDesc = new SampleDesc(1, 0),
                Flags = (uint)SwapChainFlag.AllowModeSwitch
            };

            SwapChainFullscreenDesc swapChainFullscreenDesc = new()
            {
                Windowed = true
            };

            // Create our DXGI factory to allow us to create a swapchain. 
            DXGI dxgi = DXGI.GetApi(_window, forceDxvk);
            ComPtr<IDXGIFactory2> factory = dxgi.CreateDXGIFactory<IDXGIFactory2>();
            dxgi.Dispose();

            // Create the swapchain.
            SilkMarshal.ThrowHResult
            (
                factory.CreateSwapChainForHwnd
                (
                    _device,
                    _window.Native!.DXHandle!.Value,
                    in swapChainDesc,
                    in swapChainFullscreenDesc,
                    ref Unsafe.NullRef<IDXGIOutput>(),
                    ref _swapchain
                )
            );

            // Create « render target view » 
            // Obtain the framebuffer for the swapchain's backbuffer.
            InitRenderTargetView();

            // Create de dpeth stenci view
            InitDepthBuffer(window);

            // Tell the output merger about our render target view.
            _deviceContext.OMSetRenderTargets(1, ref _renderTargetView, _depthStencilView);
            _deviceContext.ClearRenderTargetView(_renderTargetView, ref _backgroundColour[0]);

            // Set the rasterizer state with the current viewport.
            Viewport viewport = new(0, 0, _window.FramebufferSize.X, _window.FramebufferSize.Y, 0, 1);
            _deviceContext.RSSetViewports(1, in viewport);

            RenderDoc.Load(out _renderDoc);
            _renderDoc.API.SetActiveWindow(new IntPtr(_device.Handle), _window.Native!.DXHandle!.Value);
            _renderDoc.API.SetActiveWindow(new IntPtr(_device.Handle), _window.Native!.Win32!.Value.Hwnd);
            _renderDoc.API.SetCaptureFilePathTemplate(Path.Combine(Directory.GetCurrentDirectory(), "capture"));
            System.Console.WriteLine("Render doc file path : " + _renderDoc.API.GetCaptureFilePathTemplate());
        }

        unsafe ~DeviceD3D11()
        {
            if (_deviceContext.Handle is not null)
            {
                _deviceContext.ClearState();
            }

            _depthStencilView.Dispose();
            _depthTexture.Dispose();
            _renderTargetView.Dispose();
            _deviceContext.Dispose();
            _swapchain.Dispose();
            _device.Dispose();
            _compiler.Dispose();
        }

        public unsafe void BeforePresent()
        {
            _renderTargetView.Dispose();
            // Create « render target view » 
            // Obtain the framebuffer for the swapchain's backbuffer.
            InitRenderTargetView();
            // Tell the output merger about our render target view.
            _deviceContext.OMSetRenderTargets(1, ref _renderTargetView, _depthStencilView);
            _deviceContext.ClearRenderTargetView(_renderTargetView, ref _backgroundColour[0]);
            _deviceContext.ClearDepthStencilView(_depthStencilView, (uint)ClearFlag.Depth, 1.0f, 0);
        }

        public void Present()
        {
            SilkMarshal.ThrowHResult(
                _swapchain.Present(0, 0)
            );
        }

        public void Resize(Vector2D<int> size)
        {
            _deviceContext.ClearState();
            SilkMarshal.ThrowHResult(
                _swapchain.ResizeBuffers(0, 0, 0, Format.FormatB8G8R8A8Unorm, (uint)SwapChainFlag.AllowModeSwitch)
            );
            ModeDesc desc = new((uint)size.X, (uint)size.Y, null, Format.FormatB8G8R8A8Unorm);
            SilkMarshal.ThrowHResult(
                    _swapchain.ResizeTarget(ref desc)
            );
            // Set the rasterizer state with the current viewport.
            Viewport viewport = new(0, 0, (uint)size.X, (uint)size.Y, 0, 1);
            _deviceContext.RSSetViewports(1, in viewport);
        }

        public bool IsFrameCapturing()
        {
            return _renderDoc.API.IsFrameCapturing() == 1;
        }

        public unsafe void StartFrameCapture()
        {
            System.Console.WriteLine("StartFrameCapture");
            _renderDoc.API.StartFrameCapture((nint)_device.Handle, _window.Native!.DXHandle!.Value);
        }

        public unsafe void EndFrameCapture()
        {
            System.Console.WriteLine("EndFrameCapture");
            uint errorCode = _renderDoc.API.EndFrameCapture((nint)_device.Handle, _window.Native!.DXHandle!.Value);
            if (errorCode == 0)
            {
                System.Console.WriteLine("EndFrameCapture fail to capture");
            }
        }

        private unsafe void InitRenderTargetView()
        {
            using (ComPtr<ID3D11Texture2D> framebuffer = _swapchain.GetBuffer<ID3D11Texture2D>(0))
            {
                SilkMarshal.ThrowHResult(_device.CreateRenderTargetView(framebuffer, null, ref _renderTargetView));
            }
        }

        private unsafe void InitDepthBuffer(IWindow window)
        {
            Texture2DDesc depthTextureDesc = new()
            {
                Width = (uint)window.Size.X,
                Height = (uint)window.Size.Y,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.FormatD24UnormS8Uint,
                SampleDesc = new SampleDesc(1, 0),
                Usage = Usage.Default,
                BindFlags = (uint)BindFlag.DepthStencil,
                CPUAccessFlags = 0,
                MiscFlags = 0
            };

            SilkMarshal.ThrowHResult(
                _device.CreateTexture2D(ref depthTextureDesc, ref Unsafe.NullRef<SubresourceData>(), ref _depthTexture)
            );

            DepthStencilViewDesc descDSView = new()
            {
                Format = depthTextureDesc.Format,
                ViewDimension = DsvDimension.Texture2D,
                Texture2D = new Tex2DDsv() { MipSlice = 0 }
            };
            SilkMarshal.ThrowHResult(
                _device.CreateDepthStencilView(_depthTexture, in descDSView, ref _depthStencilView)
            );
        }
    }
}
