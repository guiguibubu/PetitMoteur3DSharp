//#define USE_RENDERDOC
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using PetitMoteur3D.Graphics.Shaders;
using PetitMoteur3D.Graphics.Stages;
using PetitMoteur3D.Window;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D.Graphics;

public class D3D11GraphicPipeline : IDisposable
{
    public ComPtr<IDXGISwapChain1> Swapchain { get { return _swapchain; } }
    
    public ComPtr<ID3D11RasterizerState> SolidCullFrontRS { get { return _solidCullFrontRS; } }
    public ComPtr<ID3D11RasterizerState> SolidCullBackRS { get { return _solidCullBackRS; } }
    public ComPtr<ID3D11RasterizerState> WireFrameCullBackRS { get { return _wireFrameCullBackRS; } }

    public ComPtr<ID3D11DepthStencilState> DefaultDSS { get { return _defaultDSS; } }
    public ComPtr<ID3D11DepthStencilState> ReadonlyDSS { get { return _readonlyDSS; } }
    public ComPtr<ID3D11DepthStencilState> ReadonlyGreaterDSS { get { return _readonlyGreaterDSS; } }

    public RenderTargetType RenderTargetType { get { return _renderTargetType; } }
    public ComPtr<ID3D11ShaderResourceView> GeometryBufferDiffuseSRV { get { return _diffuseGeometryBuffer.ShaderRessourceView; } }
    public ComPtr<ID3D11ShaderResourceView> GeometryBufferSpecularSRV { get { return _specularGeometryBuffer.ShaderRessourceView; } }
    public ComPtr<ID3D11ShaderResourceView> GeometryBufferNormalSRV { get { return _normalGeometryBuffer.ShaderRessourceView; } }

    internal D3D11GraphicDevice GraphicDevice => _graphicDevice;
    internal RenderPassFactory ShaderFactory => _shaderFactory;

    internal InputAssemblerStage InputAssemblerStage { get; init; }
    internal VertexShaderStage VertexShaderStage { get; init; }
    internal GeometryShaderStage GeometryShaderStage { get; init; }
    internal RasterizerStage RasterizerStage { get; init; }
    internal PixelShaderStage PixelShaderStage { get; init; }
    internal OutputMergerStage OutputMergerStage { get; init; }

    internal GraphicPipelineRessourceFactory RessourceFactory { get; }

    private ComPtr<IDXGISwapChain1> _swapchain;
    private Texture _backBufferTexture;
    private Texture _depthTexture;
    private Texture _lightAccumulationGeometryBuffer;
    private Texture _diffuseGeometryBuffer;
    private Texture _specularGeometryBuffer;
    private Texture _normalGeometryBuffer;
    private ComPtr<ID3D11RenderTargetView>[] _geometryBuffersRenderTargets;

    private ComPtr<ID3D11RasterizerState> _solidCullFrontRS;
    private ComPtr<ID3D11RasterizerState> _solidCullBackRS;
    private ComPtr<ID3D11RasterizerState> _wireFrameCullBackRS;
    private ComPtr<ID3D11DepthStencilState> _defaultDSS;
    private ComPtr<ID3D11DepthStencilState> _readonlyDSS;
    private ComPtr<ID3D11DepthStencilState> _readonlyGreaterDSS;

    private readonly D3D11GraphicDevice _graphicDevice;
    private readonly GraphicDeviceRessourceFactory _graphicRessourceFactory;
    private readonly RenderPassFactory _shaderFactory;

    private SwapChainDesc1 _swapchainDescription;

    private Size _currentSize;
    private readonly float[] _backgroundColour = new[] { 0.0f, 0.5f, 0.0f, 1.0f };

    private bool _isSwapChainComposition;
    private RenderTargetType _renderTargetType;
    private bool _disposed;
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
        _renderTargetType = RenderTargetType.NoRenderTarget;

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

        RasterizerDesc rsSolidBackDesc = new()
        {
            FillMode = FillMode.Solid,
            CullMode = CullMode.Back,
            FrontCounterClockwise = false
        };
        _solidCullBackRS = RessourceFactory.CreateRasterizerState(in rsSolidBackDesc, "SolidCullBack_RasterizerState");
        RasterizerDesc rsSolidFrontDesc = new()
        {
            FillMode = FillMode.Solid,
            CullMode = CullMode.Front,
            FrontCounterClockwise = false
        };
        _solidCullFrontRS = RessourceFactory.CreateRasterizerState(in rsSolidFrontDesc, "SolidCullFront_RasterizerState");
        RasterizerDesc rsWireDesc = new()
        {
            FillMode = FillMode.Wireframe,
            CullMode = CullMode.None,
            FrontCounterClockwise = false
        };
        _wireFrameCullBackRS = RessourceFactory.CreateRasterizerState(in rsWireDesc, "WireFrameCullBack_RasterizerState");

        // Default value from
        // https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ns-d3d11-d3d11_depth_stencil_desc
        DepthStencilDesc defaultDepthdesc = new()
        {
            DepthEnable = true,
            DepthWriteMask = DepthWriteMask.All,
            DepthFunc = ComparisonFunc.Less,
            StencilEnable = false,
            StencilReadMask = (byte)Windows.Win32.PInvoke.D3D11_DEFAULT_STENCIL_READ_MASK,
            StencilWriteMask = (byte)Windows.Win32.PInvoke.D3D11_DEFAULT_STENCIL_WRITE_MASK,
            FrontFace = new DepthStencilopDesc()
            {
                StencilFunc = ComparisonFunc.Always,
                StencilDepthFailOp = StencilOp.Keep,
                StencilPassOp = StencilOp.Keep,
                StencilFailOp = StencilOp.Keep
            },
            BackFace = new DepthStencilopDesc()
            {
                StencilFunc = ComparisonFunc.Always,
                StencilDepthFailOp = StencilOp.Keep,
                StencilPassOp = StencilOp.Keep,
                StencilFailOp = StencilOp.Keep
            }
        };
        _defaultDSS = RessourceFactory.CreateDepthStencilState(defaultDepthdesc, "Default_DeptStencilState");
        DepthStencilDesc readOnlyDepthDesc = defaultDepthdesc;
        readOnlyDepthDesc.DepthWriteMask = DepthWriteMask.Zero;
        _readonlyDSS = RessourceFactory.CreateDepthStencilState(readOnlyDepthDesc, "Readonly_DeptStencilState");
        DepthStencilDesc readOnlyGreaterDepthDesc = readOnlyDepthDesc;
        readOnlyDepthDesc.DepthFunc = ComparisonFunc.Greater;
        _readonlyGreaterDSS = RessourceFactory.CreateDepthStencilState(readOnlyDepthDesc, "ReadonlyGreater_DeptStencilState");

        RasterizerStage.SetState(_solidCullBackRS);

        _shaderFactory = new RenderPassFactory(this);

        _disposed = false;
#if USE_RENDERDOC
        Evergine.Bindings.RenderDoc.RenderDoc.Load(out _renderDoc);
        _renderDoc.API.SetActiveWindow(new IntPtr(_device.Handle), _window.Native!.DXHandle!.Value);
        _renderDoc.API.SetActiveWindow(new IntPtr(_device.Handle), _window.Native!.Win32!.Value.Hwnd);
        _renderDoc.API.SetCaptureFilePathTemplate(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "capture"));
        LogHelper.Log("[PetitMoteur3D] Render doc file path : " + _renderDoc.API.GetCaptureFilePathTemplate());
#endif
    }

    ~D3D11GraphicPipeline()
    {
        Dispose(disposing: false);
    }

    public unsafe void BeforePresent()
    {
        if (_swapchainDescription.SwapEffect == SwapEffect.FlipSequential)
        {
            //TODO: valdiate if WinUi still works 
            //SetRenderTarget(RenderTargetType.BackBuffer, clear: false);
        }
        ClearRenderTargets();
        ClearDepthStencil();
        SetRenderTarget(RenderTargetType.BackBuffer, clear: false);
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

        _backBufferTexture.Dispose();
        _lightAccumulationGeometryBuffer.Dispose();
        _diffuseGeometryBuffer.Dispose();
        _specularGeometryBuffer.Dispose();
        _normalGeometryBuffer.Dispose();
        _geometryBuffersRenderTargets = [];
        _depthTexture.Dispose();

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

    public void SetRenderTarget(RenderTargetType renderTargetType, bool clear = true, bool renderTargetOnly = false)
    {
        if (_renderTargetType != renderTargetType)
        {
            // Tell the output merger about our render target view.
            ComPtr<ID3D11RenderTargetView>[] renderTargetViews;
            _renderTargetType = renderTargetType;
            // Unbind current Render Targets and DepthStencil
            OutputMergerStage.UnbindRenderTargets();

            if (_renderTargetType == RenderTargetType.NoRenderTarget)
            {
                return;
            }

            switch (renderTargetType)
            {
                case RenderTargetType.BackBuffer:
                    renderTargetViews = [_backBufferTexture.RenderTargetViewRef];
                    break;
                case RenderTargetType.GeometryBuffers:
                    renderTargetViews = _geometryBuffersRenderTargets;
                    break;
                default:
                    throw new NotSupportedException($"Only support BackBuffer and GeometryBuffers");
            }
            if (renderTargetOnly)
            {
                OutputMergerStage.SetRenderTarget(in renderTargetViews);
            }
            else
            {
                OutputMergerStage.SetRenderTarget(in renderTargetViews, _depthTexture.TextureDepthStencilView);
            }
        }

        if (clear)
        {
            ClearRenderTarget(_renderTargetType);
            if (!renderTargetOnly)
            {
                ClearDepthStencil();
            }
        }
    }

    public void ResetViewport()
    {
        SetViewport(_currentSize.Width, _currentSize.Height);
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

    [MemberNotNull(nameof(_depthTexture))]
    [MemberNotNull(nameof(_currentSize))]
    private unsafe void InitView(IWindow window)
    {
        InitView(window.Size.Width, window.Size.Height);
    }

    [MemberNotNull(nameof(_depthTexture))]
    [MemberNotNull(nameof(_currentSize))]
    private void InitView(float width, float height)
    {
        // Create « render target view » 
        // Obtain the framebuffer for the swapchain's backbuffer.
        InitRenderTargetView();
        InitGeometryBuffers((int)width, (int)height);

        // Create de depth stenci view
        InitDepthBuffer((uint)width, (uint)height);

        SetRenderTarget(_renderTargetType);

        SetViewport((int)width, (int)height);

        _currentSize = new Size(width: (int)width, height: (int)height);
    }

    private void SetViewport(int width, int height)
    {
        // Set the rasterizer state with the current viewport.
        Viewport viewport = new(0, 0, width, height, 0, 1);
        RasterizerStage.SetViewports(1, in viewport);
    }

    private unsafe void InitRenderTargetView()
    {
        using (ComPtr<ID3D11Texture2D> framebuffer = _swapchain.GetBuffer<ID3D11Texture2D>(0))
        {
            Texture2DDesc desc = new();
            framebuffer.GetDesc(ref desc);
            _backBufferTexture = _graphicRessourceFactory.TextureManager.Factory
                .CreateBuilder(desc, framebuffer)
                .WithRenderTargetView()
                .WithName("GraphicPipeline_BackBuffer")
                .Build();
        }
    }

    [MemberNotNull(nameof(_lightAccumulationGeometryBuffer))]
    [MemberNotNull(nameof(_diffuseGeometryBuffer))]
    [MemberNotNull(nameof(_specularGeometryBuffer))]
    [MemberNotNull(nameof(_normalGeometryBuffer))]
    [MemberNotNull(nameof(_geometryBuffersRenderTargets))]
    private unsafe void InitGeometryBuffers(int width, int height)
    {
        Texture2DDesc colorTextureDesc = new()
        {
            Width = (uint)width,
            Height = (uint)height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm,
            SampleDesc = new Silk.NET.DXGI.SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)(BindFlag.RenderTarget | BindFlag.ShaderResource),
            CPUAccessFlags = 0,
            MiscFlags = 0
        };

        ShaderResourceViewDesc colorTextureShaderResourceViewDesc = new()
        {
            Format = Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm,
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


        TextureManager textureManager = _graphicRessourceFactory.TextureManager;

        _lightAccumulationGeometryBuffer = textureManager.GetOrCreateTexture($"GraphicPipeline_LightAccumulationGeometryBuffer", colorTextureDesc,
            (TextureBuilder builder) => builder
            .WithShaderRessourceView(colorTextureShaderResourceViewDesc)
            .WithRenderTargetView());
        _diffuseGeometryBuffer = textureManager.GetOrCreateTexture($"GraphicPipeline_DiffuseGeometryBuffer", colorTextureDesc,
            (TextureBuilder builder) => builder
            .WithShaderRessourceView(colorTextureShaderResourceViewDesc)
            .WithRenderTargetView());
        _specularGeometryBuffer = textureManager.GetOrCreateTexture($"GraphicPipeline_SpecularGeometryBuffer", colorTextureDesc,
            (TextureBuilder builder) => builder
            .WithShaderRessourceView(colorTextureShaderResourceViewDesc)
            .WithRenderTargetView());

        Texture2DDesc normalTextureDesc = new()
        {
            Width = (uint)width,
            Height = (uint)height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Silk.NET.DXGI.Format.FormatR32G32B32A32Float,
            SampleDesc = new Silk.NET.DXGI.SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)(BindFlag.RenderTarget | BindFlag.ShaderResource),
            CPUAccessFlags = 0,
            MiscFlags = 0
        };

        ShaderResourceViewDesc normalTextureShaderResourceViewDesc = new()
        {
            Format = Silk.NET.DXGI.Format.FormatR32G32B32A32Float,
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

        _normalGeometryBuffer = textureManager.GetOrCreateTexture($"GraphicPipeline_NormalGeometryBuffer", normalTextureDesc,
            (TextureBuilder builder) => builder
            .WithShaderRessourceView(normalTextureShaderResourceViewDesc)
            .WithRenderTargetView());

        _geometryBuffersRenderTargets = new ComPtr<ID3D11RenderTargetView>[] { _lightAccumulationGeometryBuffer.RenderTargetView, _diffuseGeometryBuffer.RenderTargetView, _specularGeometryBuffer.RenderTargetView, _normalGeometryBuffer.RenderTargetView };
    }

    [MemberNotNull(nameof(_depthTexture))]
    private void InitDepthBuffer(uint width, uint height)
    {
        Texture2DDesc depthTextureDesc = new()
        {
            Width = width,
            Height = height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.FormatR24G8Typeless,
            SampleDesc = new SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)(BindFlag.DepthStencil | BindFlag.ShaderResource),
            CPUAccessFlags = 0,
            MiscFlags = 0
        };

        DepthStencilViewDesc descDSView = new()
        {
            Format = Format.FormatD24UnormS8Uint,
            ViewDimension = DsvDimension.Texture2D,
            Texture2D = new Tex2DDsv() { MipSlice = 0 }
        };

        _depthTexture = _graphicRessourceFactory.TextureManager.GetOrCreateTexture("GraphicPipeline_DepthTexture", depthTextureDesc,
            builder => builder
            .WithDepthStencilView(descDSView)
            .WithName("GraphicPipeline_DepthTexture"));
    }

    private unsafe void ClearRenderTarget(RenderTargetType renderTargetType)
    {
        if (renderTargetType == RenderTargetType.NoRenderTarget)
        {
            return;
        }

        ComPtr<ID3D11RenderTargetView>[] renderTargetViews;
        switch (renderTargetType)
        {
            case RenderTargetType.BackBuffer:
                renderTargetViews = [_backBufferTexture.RenderTargetView]; break;
            case RenderTargetType.GeometryBuffers:
                renderTargetViews = _geometryBuffersRenderTargets; break;
            default:
                throw new NotSupportedException($"Only support BackBuffer and GeometryBuffers");
        }

        // On efface la surface de rendu
        foreach (ComPtr<ID3D11RenderTargetView> renderTargetView in renderTargetViews)
        {
            _graphicDevice.DeviceContext.ClearRenderTargetView(renderTargetView, _backgroundColour.AsSpan());
        }
    }

    private unsafe void ClearRenderTargets()
    {
        ComPtr<ID3D11RenderTargetView>[] renderTargetViews = [
            _backBufferTexture.RenderTargetView,
            _lightAccumulationGeometryBuffer.RenderTargetView,
            _diffuseGeometryBuffer.RenderTargetView,
            _specularGeometryBuffer.RenderTargetView,
            _normalGeometryBuffer.RenderTargetView
            ];

        // On efface la surface de rendu
        foreach (ComPtr<ID3D11RenderTargetView> renderTargetView in renderTargetViews)
        {
            _graphicDevice.DeviceContext.ClearRenderTargetView(renderTargetView, _backgroundColour.AsSpan());
        }
    }

    private unsafe void ClearDepthStencil()
    {
        // On ré-initialise le tampon de profondeur
        _graphicDevice.DeviceContext.ClearDepthStencilView(_depthTexture.TextureDepthStencilView, (uint)ClearFlag.Depth, 1.0f, 0);
    }

    protected virtual void Dispose(bool disposing)
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
            _swapchain.SetFullscreenState(false, ref Unsafe.NullRef<IDXGIOutput>());

            _swapchain.Dispose();
            _backBufferTexture.Dispose();
            _depthTexture.Dispose();
            _lightAccumulationGeometryBuffer.Dispose();
            _diffuseGeometryBuffer.Dispose();
            _specularGeometryBuffer.Dispose();
            _normalGeometryBuffer.Dispose();
            _geometryBuffersRenderTargets = [];
            _solidCullFrontRS.Dispose();
            _solidCullBackRS.Dispose();
            _defaultDSS.Dispose();
            _readonlyDSS.Dispose();
            _readonlyGreaterDSS.Dispose();
            _wireFrameCullBackRS.Dispose();

            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~D3D11GraphicPipeline()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
