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
        public ComPtr<ID3D11RasterizerState> SolidCullBackRS { get { return _solidCullBackRS; } }
        public ComPtr<ID3D11RasterizerState> WireFrameCullBackRS { get { return _wireFrameCullBackRS; } }
        public D3DCompiler ShaderCompiler { get { return _compiler; } }

        private ComPtr<ID3D11Device> _device;
        private ComPtr<ID3D11DeviceContext> _deviceContext;
        private ComPtr<IDXGISwapChain1> _swapchain;
        private ComPtr<ID3D11RenderTargetView> _renderTargetView;
        private ComPtr<ID3D11DepthStencilView> _depthStencilView;
        private ComPtr<ID3D11Texture2D> _depthTexture;
        public ComPtr<ID3D11RasterizerState> _solidCullBackRS;
        public ComPtr<ID3D11RasterizerState> _wireFrameCullBackRS;
        private readonly D3DCompiler _compiler;

        private readonly Silk.NET.Windowing.IWindow _window;

        private readonly float[] _backgroundColour = new[] { 0.0f, 0.5f, 0.0f, 1.0f };

        private readonly RenderDoc _renderDoc;

        public unsafe DeviceD3D11(Silk.NET.Windowing.IWindow window)
        {
            _window = window;

            //Whether or not to force use of DXVK on platforms where native DirectX implementations are available
            const bool forceDxvk = false;

            _compiler = D3DCompiler.GetApi();

            InitDevice(forceDxvk);

            // Initialisatio de la swapchain
            InitSwapChain(window, forceDxvk);

            // Create « render target view » 
            // Obtain the framebuffer for the swapchain's backbuffer.
            InitRenderTargetView();

            // Create de depth stenci view
            InitDepthBuffer(window);

            // Tell the output merger about our render target view.
            _deviceContext.OMSetRenderTargets(1, ref _renderTargetView, _depthStencilView);
            _deviceContext.ClearRenderTargetView(_renderTargetView, _backgroundColour.AsSpan());
            _deviceContext.ClearDepthStencilView(_depthStencilView, (uint)ClearFlag.Depth, 1.0f, 0);

            // Set the rasterizer state with the current viewport.
            Viewport viewport = new(0, 0, _window.FramebufferSize.X, _window.FramebufferSize.Y, 0, 1);
            _deviceContext.RSSetViewports(1, in viewport);

            RasterizerDesc rsSolidDesc = new()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Back,
                FrontCounterClockwise = false
            };
            SilkMarshal.ThrowHResult(_device.CreateRasterizerState(in rsSolidDesc, ref _solidCullBackRS));
            RasterizerDesc rsWireDesc = new()
            {
                FillMode = FillMode.Wireframe,
                CullMode = CullMode.None,
                FrontCounterClockwise = false
            };
            SilkMarshal.ThrowHResult(_device.CreateRasterizerState(in rsWireDesc, ref _wireFrameCullBackRS));
            _deviceContext.RSSetState(_solidCullBackRS);

            RenderDoc.Load(out _renderDoc);
            _renderDoc.API.SetActiveWindow(new IntPtr(_device.Handle), _window.Native!.DXHandle!.Value);
            _renderDoc.API.SetActiveWindow(new IntPtr(_device.Handle), _window.Native!.Win32!.Value.Hwnd);
            _renderDoc.API.SetCaptureFilePathTemplate(Path.Combine(Directory.GetCurrentDirectory(), "capture"));
            System.Console.WriteLine("Render doc file path : " + _renderDoc.API.GetCaptureFilePathTemplate());
        }

        unsafe ~DeviceD3D11()
        {
            // passer en mode fenêtré
            _swapchain.SetFullscreenState(false, ref Unsafe.NullRef<IDXGIOutput>());

            if (_deviceContext.Handle is not null)
            {
                _deviceContext.ClearState();
            }

            _solidCullBackRS.Dispose();
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
            // On efface la surface de rendu
            _deviceContext.ClearRenderTargetView(_renderTargetView, _backgroundColour.AsSpan());
            // On ré-initialise le tampon de profondeur
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

        public Vector4D<float> GetBackgroundColour()
        {
            return new Vector4D<float>(_backgroundColour[0], _backgroundColour[1], _backgroundColour[2], _backgroundColour[3]);
        }

        public void SetBackgroundColour(float r, float g, float b, float a)
        {
            _backgroundColour[0] = r;
            _backgroundColour[1] = g;
            _backgroundColour[2] = b;
            _backgroundColour[3] = a;
        }

        public ComPtr<ID3D11RasterizerState> GetRasterizerState()
        {
            ComPtr<ID3D11RasterizerState> result = null;
            _deviceContext.RSGetState(ref result);
            return result;
        }

        public unsafe void SetRasterizerState(ComPtr<ID3D11RasterizerState> rsState)
        {
            if (rsState.Handle is null)
            {
                return;
            }
            _deviceContext.RSSetState(rsState);
        }

        private unsafe void InitDevice(bool forceDxvk)
        {
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

#if DEBUG
            //This is not supported under DXVK 
            //TODO: PR a stub into DXVK for this maybe?
            if (OperatingSystem.IsWindows())
            {
                // Log debug messages for this device (given that we've enabled the debug flag). Don't do this in release code!
                _device.SetInfoQueueCallback(msg => Console.WriteLine(SilkMarshal.PtrToString((nint)msg.PDescription)));
            }
#endif
        }

        private unsafe void InitSwapChain(IWindow window, bool forceDxvk)
        {
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

            factory.Dispose();
        }

        private unsafe void InitRenderTargetView()
        {
            using (ComPtr<ID3D11Texture2D> framebuffer = _swapchain.GetBuffer<ID3D11Texture2D>(0))
            {
                SilkMarshal.ThrowHResult(_device.CreateRenderTargetView(framebuffer, ref Unsafe.NullRef<RenderTargetViewDesc>(), ref _renderTargetView));
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
