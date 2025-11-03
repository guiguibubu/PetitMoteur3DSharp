//#define USE_RENDERDOC
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using PetitMoteur3D.Window;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;

namespace PetitMoteur3D;

public class DeviceD3D11
{
    public ref ComPtr<ID3D11Device> Device { get { return ref _device; } }
    public ref ComPtr<ID3D11DeviceContext> DeviceContext { get { return ref _deviceContext; } }
    public ref ComPtr<IDXGISwapChain1> Swapchain { get { return ref _swapchain; } }
    public ref ComPtr<ID3D11RenderTargetView> RenderTargetView { get { return ref _renderTargetView; } }
    public ref ComPtr<ID3D11DepthStencilView> DepthStencilView { get { return ref _depthStencilView; } }
    public ref ComPtr<ID3D11RasterizerState> SolidCullBackRS { get { return ref _solidCullBackRS; } }
    public ref ComPtr<ID3D11RasterizerState> WireFrameCullBackRS { get { return ref _wireFrameCullBackRS; } }
    public D3DCompiler ShaderCompiler { get { return _compiler; } }

    private ComPtr<ID3D11Device> _device;
    private ComPtr<ID3D11DeviceContext> _deviceContext;
    private ComPtr<IDXGISwapChain1> _swapchain;
    private ComPtr<ID3D11RenderTargetView> _renderTargetView;
    private ComPtr<ID3D11DepthStencilView> _depthStencilView;
    private ComPtr<ID3D11Texture2D> _depthTexture;
    private ComPtr<ID3D11RasterizerState> _solidCullBackRS;
    private ComPtr<ID3D11RasterizerState> _wireFrameCullBackRS;

    private SwapChainDesc1 _swapchainDescription;

    private readonly D3DCompiler _compiler;
    private readonly D3D11 _d3d11Api;

    private static readonly D3DFeatureLevel[] FEATURES_LEVELS = {
        D3DFeatureLevel.Level111,
        D3DFeatureLevel.Level110
    };


    private readonly IWindow _window;
    private Size _currentSize = new();
    private readonly float[] _backgroundColour = new[] { 0.0f, 0.5f, 0.0f, 1.0f };

    private bool _isSwapChainComposition = false;
#if USE_RENDERDOC
    private readonly Evergine.Bindings.RenderDoc.RenderDoc _renderDoc;
#endif

    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    /// <param name="forceDxvk">Whether or not to force use of DXVK on platforms where native DirectX implementations are available</param>
    public unsafe DeviceD3D11(IWindow window, bool forceDxvk = false)
    {
        _window = window;

        _compiler = D3DCompiler.GetApi();

        // Create our D3D11 logical device.
#pragma warning disable CS0618 // Type or member is obsolete
        _d3d11Api = D3D11.GetApi(DXSwapchainProvider.Win32, forceDxvk);
#pragma warning restore CS0618 // Type or member is obsolete

        InitDevice(_d3d11Api);

        // Initialisation de la swapchain
        InitSwapChain(window, forceDxvk);

        InitView(window);

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

#if USE_RENDERDOC
        Evergine.Bindings.RenderDoc.RenderDoc.Load(out _renderDoc);
        _renderDoc.API.SetActiveWindow(new IntPtr(_device.Handle), _window.Native!.DXHandle!.Value);
        _renderDoc.API.SetActiveWindow(new IntPtr(_device.Handle), _window.Native!.Win32!.Value.Hwnd);
        _renderDoc.API.SetCaptureFilePathTemplate(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "capture"));
        LogHelper.Log("[PetitMoteur3D] Render doc file path : " + _renderDoc.API.GetCaptureFilePathTemplate());
#endif
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
        _d3d11Api.Dispose();
    }

    public unsafe void BeforePresent()
    {
        if (_swapchainDescription.SwapEffect == SwapEffect.FlipSequential)
        {
            SetRenderTarget();
        }
        ClearRenderTarget();
    }

    public void Present()
    {
        uint syncInterval = _swapchainDescription.SwapEffect == SwapEffect.FlipSequential ? 1u : 0u;
        SilkMarshal.ThrowHResult(
            _swapchain.Present(syncInterval, 0)
        );
    }

    public unsafe void Resize(ref readonly Size size)
    {
        if (size.Width == _currentSize.Width && size.Height == _currentSize.Height)
        {
            return;
        }
        _deviceContext.ClearState();

        _renderTargetView.Dispose();
        _depthTexture.Dispose();
        _depthStencilView.Dispose();

        if (_isSwapChainComposition)
        {
            SilkMarshal.ThrowHResult(
                _swapchain.ResizeBuffers(_swapchainDescription.BufferCount, (uint)size.Width, (uint)size.Height, Format.FormatB8G8R8A8Unorm, (uint)SwapChainFlag.AllowModeSwitch)
            );
        }
        else
        {
            SilkMarshal.ThrowHResult(
                _swapchain.ResizeBuffers(_swapchainDescription.BufferCount, 0, 0, Format.FormatB8G8R8A8Unorm, (uint)SwapChainFlag.AllowModeSwitch)
            );
        }

        InitView(size.Width, size.Height);

        _currentSize = size;
    }

#if USE_RENDERDOC
    public bool IsFrameCapturing()
    {
        return _renderDoc.API.IsFrameCapturing() == 1;
    }

    public unsafe void StartFrameCapture()
    {
        LogHelper.Log("[PetitMoteur3D] StartFrameCapture");
        _renderDoc.API.StartFrameCapture((nint)_device.Handle, _window.Native!.DXHandle!.Value);
    }

    public unsafe void EndFrameCapture()
    {
        LogHelper.Log("[PetitMoteur3D] EndFrameCapture");
        uint errorCode = _renderDoc.API.EndFrameCapture((nint)_device.Handle, _window.Native!.DXHandle!.Value);
        if (errorCode == 0)
        {
            LogHelper.Log("[PetitMoteur3D] EndFrameCapture fail to capture");
        }
    }
#endif

    public void GetBackgroundColour(out Vector4D<float> backgroundColour)
    {
        backgroundColour = new Vector4D<float>(_backgroundColour[0], _backgroundColour[1], _backgroundColour[2], _backgroundColour[3]);
    }

    public void SetBackgroundColour(float r, float g, float b, float a)
    {
        _backgroundColour[0] = r;
        _backgroundColour[1] = g;
        _backgroundColour[2] = b;
        _backgroundColour[3] = a;
    }

    public void GetRasterizerState(out ComPtr<ID3D11RasterizerState> result)
    {
        result = null;
        _deviceContext.RSGetState(ref result);
    }

    public unsafe void SetRasterizerState(ref readonly ComPtr<ID3D11RasterizerState> rsState)
    {
        if (rsState.Handle is null)
        {
            return;
        }
        _deviceContext.RSSetState(rsState);
    }

    private unsafe void InitDevice(D3D11 d3d11Api)
    {
        uint createDeviceFlags = (uint)CreateDeviceFlag.BgraSupport;
        createDeviceFlags |= (uint)CreateDeviceFlag.Singlethreaded;
        createDeviceFlags |= (uint)CreateDeviceFlag.PreventInternalThreadingOptimizations;
#if DEBUG
        createDeviceFlags |= (uint)CreateDeviceFlag.Debug;
#endif
        SilkMarshal.ThrowHResult
        (
            d3d11Api.CreateDevice
            (
                default(ComPtr<IDXGIAdapter>),
                D3DDriverType.Hardware,
                0,
                createDeviceFlags,
                in FEATURES_LEVELS[0],
                2,
                D3D11.SdkVersion,
                ref _device,
                null,
                ref _deviceContext
            )
        );

#if DEBUG
        //This is not supported under DXVK 
        //TODO: PR a stub into DXVK for this maybe?
        if (OperatingSystem.IsWindows())
        {
            // Log debug messages for this device (given that we've enabled the debug flag). Don't do this in release code!
            _device.SetInfoQueueCallback(msg =>
            {
                string? msgStr = SilkMarshal.PtrToString((nint)msg.PDescription);
                LogHelper.Log(msgStr);
                //System.Diagnostics.Debugger.Break();
            });
        }
#endif
    }

    private unsafe void InitSwapChain(IWindow window, bool forceDxvk)
    {
        if (window is ICompositionWindow)
        {
            _isSwapChainComposition = true;
            InitSwapChain((uint)window.Size.Width, (uint)window.Size.Height, windowPtr: 0, forceDxvk);
            // Ensure that DXGI does not queue more than one frame at a time. This both reduces 
            // latency and ensures that the application will only render after each VSync, minimizing 
            // power consumption.
            ComPtr<IDXGIDevice1> dxgiDevice = Device.QueryInterface<IDXGIDevice1>();
            dxgiDevice.SetMaximumFrameLatency(1);
        }
        else
        {
            InitSwapChain((uint)window.Size.Width, (uint)window.Size.Height, window.NativeHandle!.Value, forceDxvk);
        }
        _swapchainDescription = new();
        _swapchain.GetDesc1(ref _swapchainDescription);
    }

    private unsafe void InitSwapChain(uint width, uint heigth, nint windowPtr, bool forceDxvk)
    {
        // Create our swapchain description.
        SwapChainDesc1 swapChainDesc = new()
        {
            Width = width,
            Height = heigth,
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
#pragma warning disable CS0618 // Type or member is obsolete
        DXGI dxgi = DXGI.GetApi(DXSwapchainProvider.Win32, forceDxvk);
#pragma warning restore CS0618 // Type or member is obsolete
        ComPtr<IDXGIFactory2> factory = dxgi.CreateDXGIFactory<IDXGIFactory2>();
        dxgi.Dispose();

        // Create the swapchain.
        if (windowPtr != IntPtr.Zero)
        {
            SilkMarshal.ThrowHResult
            (
                factory.CreateSwapChainForHwnd
                (
                    _device,
                    windowPtr,
                    in swapChainDesc,
                    in swapChainFullscreenDesc,
                    ref Unsafe.NullRef<IDXGIOutput>(),
                    ref _swapchain
                )
            );
        }
        else
        {
            // https://learn.microsoft.com/en-us/windows/win32/api/dxgi1_2/nf-dxgi1_2-idxgifactory2-createswapchainforcomposition
            // You must specify the DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL value in the SwapEffect member of DXGI_SWAP_CHAIN_DESC1 because CreateSwapChainForComposition supports only flip presentation model.
            // You must also specify the DXGI_SCALING_STRETCH value in the Scaling member of DXGI_SWAP_CHAIN_DESC1.
            swapChainDesc.SwapEffect = SwapEffect.FlipSequential;
            swapChainDesc.Scaling = Scaling.Stretch;
            // https://learn.microsoft.com/en-us/windows/win32/api/dxgi1_2/ns-dxgi1_2-dxgi_swap_chain_desc1
            // A value that describes the number of buffers in the swap chain. When you create a full-screen swap chain, you typically include the front buffer in this value.
            swapChainDesc.BufferCount = 2;
            //swapChainDesc.Flags = 0;
            //swapChainDesc.AlphaMode = AlphaMode.Premultiplied;
            SilkMarshal.ThrowHResult
            (
                factory.CreateSwapChainForComposition
                (
                    _device,
                    in swapChainDesc,
                    ref Unsafe.NullRef<IDXGIOutput>(),
                    ref _swapchain
                )
            );
        }

        factory.Dispose();
    }

    private unsafe void InitView(IWindow window)
    {
        InitView(window.Size.Width, window.Size.Height);
    }

    private unsafe void InitView(float width, float height)
    {
        // Create « render target view » 
        // Obtain the framebuffer for the swapchain's backbuffer.
        InitRenderTargetView();

        // Create de depth stenci view
        InitDepthBuffer((uint)width, (uint)height);

        SetRenderTarget();

        // Set the rasterizer state with the current viewport.
        Viewport viewport = new(0, 0, width, height, 0, 1);
        _deviceContext.RSSetViewports(1, in viewport);
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

        InitDepthBuffer((uint)window.Size.Width, (uint)window.Size.Height);
    }

    private unsafe void InitDepthBuffer(uint width, uint height)
    {
        Texture2DDesc depthTextureDesc = new()
        {
            Width = width,
            Height = height,
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

    private unsafe void ClearRenderTarget()
    {
        // On efface la surface de rendu
        _deviceContext.ClearRenderTargetView(_renderTargetView, _backgroundColour.AsSpan());
        // On ré-initialise le tampon de profondeur
        _deviceContext.ClearDepthStencilView(_depthStencilView, (uint)ClearFlag.Depth, 1.0f, 0);
    }

    private void SetRenderTarget(bool clear = true)
    {
        // Tell the output merger about our render target view.
        _deviceContext.OMSetRenderTargets(1, ref _renderTargetView, _depthStencilView);
        if (clear)
        {
            ClearRenderTarget();
        }
    }
}
