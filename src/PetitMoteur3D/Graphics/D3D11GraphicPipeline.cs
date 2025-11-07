//#define USE_RENDERDOC
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using PetitMoteur3D.Graphics.Stages;
using PetitMoteur3D.Window;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D.Graphics;

public class D3D11GraphicPipeline
{
    public ref ComPtr<IDXGISwapChain1> Swapchain { get { return ref _swapchain; } }
    public ref ComPtr<ID3D11RenderTargetView> RenderTargetView { get { return ref _renderTargetView; } }
    public ref ComPtr<ID3D11DepthStencilView> DepthStencilView { get { return ref _depthStencilView; } }
    public ref ComPtr<ID3D11RasterizerState> SolidCullBackRS { get { return ref _solidCullBackRS; } }
    public ref ComPtr<ID3D11RasterizerState> WireFrameCullBackRS { get { return ref _wireFrameCullBackRS; } }

    internal D3D11GraphicDevice GraphicDevice => _graphicDevice;

    internal InputAssemblerStage InputAssemblerStage { get; init; }
    internal VertexShaderStage VertexShaderStage { get; init; }
    internal GeometryShaderStage GeometryShaderStage { get; init; }
    internal RasterizerStage RasterizerStage { get; init; }
    internal PixelShaderStage PixelShaderStage { get; init; }
    internal OutputMergerStage OutputMergerStage { get; init; }

    internal GraphicPipelineRessourceFactory RessourceFactory { get; }

    private ComPtr<IDXGISwapChain1> _swapchain;
    private ComPtr<ID3D11RenderTargetView> _renderTargetView;
    private ComPtr<ID3D11DepthStencilView> _depthStencilView;
    private ComPtr<ID3D11Texture2D> _depthTexture;
    private ComPtr<ID3D11RasterizerState> _solidCullBackRS;
    private ComPtr<ID3D11RasterizerState> _wireFrameCullBackRS;

    private readonly D3D11GraphicDevice _graphicDevice;
    private readonly GraphicDeviceRessourceFactory _graphicRessourceFactory;

    private SwapChainDesc1 _swapchainDescription;

    private Size _currentSize;
    private readonly float[] _backgroundColour = new[] { 0.0f, 0.5f, 0.0f, 1.0f };

    private bool _isSwapChainComposition;
#if USE_RENDERDOC
    private readonly Evergine.Bindings.RenderDoc.RenderDoc _renderDoc;
#endif

    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    /// <param name="forceDxvk">Whether or not to force use of DXVK on platforms where native DirectX implementations are available</param>
    internal unsafe D3D11GraphicPipeline(D3D11GraphicDevice graphicDevice, IWindow window)
    {
        _graphicDevice = graphicDevice;
        _graphicRessourceFactory = graphicDevice.RessourceFactory;
        RessourceFactory = new GraphicPipelineRessourceFactory(graphicDevice);
        _isSwapChainComposition = false;

        // Initialisation de la swapchain
        InitSwapChain(window, graphicDevice.DxVk);

        // Initialisation des stages
        InputAssemblerStage = new InputAssemblerStage(_graphicDevice.DeviceContext);
        VertexShaderStage = new VertexShaderStage(_graphicDevice.DeviceContext);
        GeometryShaderStage = new GeometryShaderStage(_graphicDevice.DeviceContext);
        RasterizerStage = new RasterizerStage(_graphicDevice.DeviceContext);
        PixelShaderStage = new PixelShaderStage(_graphicDevice.DeviceContext);
        OutputMergerStage = new OutputMergerStage(_graphicDevice.DeviceContext);


        InitView(window);

        RasterizerDesc rsSolidDesc = new()
        {
            FillMode = FillMode.Solid,
            CullMode = CullMode.Back,
            FrontCounterClockwise = false
        };
        _solidCullBackRS = RessourceFactory.CreateRasterizerState(in rsSolidDesc);
        RasterizerDesc rsWireDesc = new()
        {
            FillMode = FillMode.Wireframe,
            CullMode = CullMode.None,
            FrontCounterClockwise = false
        };
        _wireFrameCullBackRS = RessourceFactory.CreateRasterizerState(in rsWireDesc);

        RasterizerStage.SetState(_solidCullBackRS);
        
        _currentSize = new Size();

#if USE_RENDERDOC
        Evergine.Bindings.RenderDoc.RenderDoc.Load(out _renderDoc);
        _renderDoc.API.SetActiveWindow(new IntPtr(_device.Handle), _window.Native!.DXHandle!.Value);
        _renderDoc.API.SetActiveWindow(new IntPtr(_device.Handle), _window.Native!.Win32!.Value.Hwnd);
        _renderDoc.API.SetCaptureFilePathTemplate(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "capture"));
        LogHelper.Log("[PetitMoteur3D] Render doc file path : " + _renderDoc.API.GetCaptureFilePathTemplate());
#endif
    }

    unsafe ~D3D11GraphicPipeline()
    {
        // passer en mode fenêtré
        _swapchain.SetFullscreenState(false, ref Unsafe.NullRef<IDXGIOutput>());

        _solidCullBackRS.Dispose();
        _depthStencilView.Dispose();
        _depthTexture.Dispose();
        _renderTargetView.Dispose();
        _swapchain.Dispose();
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
        _graphicDevice.DeviceContext.ClearState();

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

    public void GetBackgroundColour(out System.Numerics.Vector4 backgroundColour)
    {
        backgroundColour = new System.Numerics.Vector4(_backgroundColour[0], _backgroundColour[1], _backgroundColour[2], _backgroundColour[3]);
    }

    public void SetBackgroundColour(float r, float g, float b, float a)
    {
        _backgroundColour[0] = r;
        _backgroundColour[1] = g;
        _backgroundColour[2] = b;
        _backgroundColour[3] = a;
    }

    public void DrawIndexed(uint IndexCount, uint StartIndexLocation, int BaseVertexLocation)
    {
        _graphicDevice.DeviceContext.DrawIndexed(IndexCount, StartIndexLocation, BaseVertexLocation);
    }

    private void SetRenderTarget(bool clear = true)
    {
        // Tell the output merger about our render target view.
        _graphicDevice.DeviceContext.OMSetRenderTargets(1, ref _renderTargetView, _depthStencilView);
        if (clear)
        {
            ClearRenderTarget();
        }
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
            _graphicDevice.SetMaximumFrameLatency(1);
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

        // Create the swapchain.
        if (windowPtr != nint.Zero)
        {
            SwapChainFullscreenDesc swapChainFullscreenDesc = new()
            {
                Windowed = true
            };
            _swapchain = _graphicRessourceFactory.CreateSwapChainForHwnd(windowPtr, in swapChainDesc, in swapChainFullscreenDesc, ref Unsafe.NullRef<IDXGIOutput>());
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
            _swapchain = _graphicRessourceFactory.CreateSwapChainForComposition(in swapChainDesc, ref Unsafe.NullRef<IDXGIOutput>());
        }
    }

    private unsafe void InitView(IWindow window)
    {
        InitView(window.Size.Width, window.Size.Height);
    }

    private void InitView(float width, float height)
    {
        // Create « render target view » 
        // Obtain the framebuffer for the swapchain's backbuffer.
        InitRenderTargetView();

        // Create de depth stenci view
        InitDepthBuffer((uint)width, (uint)height);

        SetRenderTarget();

        // Set the rasterizer state with the current viewport.
        Viewport viewport = new(0, 0, width, height, 0, 1);
        RasterizerStage.SetViewports(1, in viewport);
    }

    private unsafe void InitRenderTargetView()
    {
        using (ComPtr<ID3D11Texture2D> framebuffer = _swapchain.GetBuffer<ID3D11Texture2D>(0))
        {
            _renderTargetView = _graphicRessourceFactory.CreateRenderTargetView(framebuffer, in Unsafe.NullRef<RenderTargetViewDesc>());
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

        _depthTexture = _graphicRessourceFactory.CreateTexture2D(in depthTextureDesc, in Unsafe.NullRef<SubresourceData>());

        DepthStencilViewDesc descDSView = new()
        {
            Format = depthTextureDesc.Format,
            ViewDimension = DsvDimension.Texture2D,
            Texture2D = new Tex2DDsv() { MipSlice = 0 }
        };

        _depthStencilView = _graphicRessourceFactory.CreateDepthStencilView(_depthTexture, in descDSView);
    }

    private unsafe void ClearRenderTarget()
    {
        // On efface la surface de rendu
        _graphicDevice.DeviceContext.ClearRenderTargetView(_renderTargetView, _backgroundColour.AsSpan());
        // On ré-initialise le tampon de profondeur
        _graphicDevice.DeviceContext.ClearDepthStencilView(_depthStencilView, (uint)ClearFlag.Depth, 1.0f, 0);
    }
}
