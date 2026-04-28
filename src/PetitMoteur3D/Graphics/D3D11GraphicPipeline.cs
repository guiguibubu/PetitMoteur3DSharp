//#define USE_RENDERDOC
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using PetitMoteur3D.Graphics.Stages;
using PetitMoteur3D.Window;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D.Graphics;

internal sealed class D3D11GraphicPipeline : IDisposable
{
    public D3D11SwapChain SwapChain { get { return _swapchain; } }

    public ComPtr<ID3D11RasterizerState> SolidCullFrontRS { get { return _solidCullFrontRS; } }
    public ComPtr<ID3D11RasterizerState> SolidCullBackRS { get { return _solidCullBackRS; } }
    public ComPtr<ID3D11RasterizerState> WireFrameCullBackRS { get { return _wireFrameCullBackRS; } }

    public ComPtr<ID3D11DepthStencilState> DefaultDSS { get { return _defaultDSS; } }
    public ComPtr<ID3D11DepthStencilState> ReadonlyDSS { get { return _readonlyDSS; } }
    public ComPtr<ID3D11DepthStencilState> ReadonlyGreaterDSS { get { return _readonlyGreaterDSS; } }

    internal D3D11GraphicDevice GraphicDevice => _graphicDevice;
    internal ComPtr<ID3D11DeviceContext> DeviceContext => _deviceContext;
    internal InputAssemblerStage InputAssemblerStage { get; init; }
    internal VertexShaderStage VertexShaderStage { get; init; }
    internal GeometryShaderStage GeometryShaderStage { get; init; }
    internal RasterizerStage RasterizerStage { get; init; }
    internal PixelShaderStage PixelShaderStage { get; init; }
    internal OutputMergerStage OutputMergerStage { get; init; }

    internal GraphicPipelineRessourceFactory RessourceFactory { get; }

    private ComPtr<ID3D11RasterizerState> _solidCullFrontRS;
    private ComPtr<ID3D11RasterizerState> _solidCullBackRS;
    private ComPtr<ID3D11RasterizerState> _wireFrameCullBackRS;
    private ComPtr<ID3D11DepthStencilState> _defaultDSS;
    private ComPtr<ID3D11DepthStencilState> _readonlyDSS;
    private ComPtr<ID3D11DepthStencilState> _readonlyGreaterDSS;

    private readonly D3D11GraphicDevice _graphicDevice;
    private readonly ComPtr<ID3D11DeviceContext> _deviceContext;
    private readonly GraphicDeviceRessourceFactory _graphicRessourceFactory;

    private D3D11SwapChain _swapchain;

    private Size _currentSize;
    private readonly float[] _backgroundColour = new[] { 0.0f, 0.5f, 0.0f, 1.0f };

    private bool _isSwapChainComposition;

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
        _deviceContext = graphicDevice.ImmediateContext;
        _graphicRessourceFactory = graphicDevice.RessourceFactory;
        RessourceFactory = new GraphicPipelineRessourceFactory(graphicDevice);
        _isSwapChainComposition = false;

        // Initialisation de la swapchain
        InitSwapChain(window, graphicDevice.DxVk);

        // Initialisation des stages
        InputAssemblerStage = new InputAssemblerStage(_deviceContext);
        VertexShaderStage = new VertexShaderStage(_deviceContext);
        GeometryShaderStage = new GeometryShaderStage(_deviceContext);
        RasterizerStage = new RasterizerStage(_deviceContext);
        PixelShaderStage = new PixelShaderStage(_deviceContext);
        OutputMergerStage = new OutputMergerStage(_deviceContext);

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

    public void Present()
    {
        uint syncInterval = _swapchain.SwapEffect == SwapEffect.FlipSequential ? 1u : 0u;
        _swapchain.Present(syncInterval, flags: 0);
    }

    public unsafe void Resize(ref readonly Size size)
    {
        if (size.Width == _currentSize.Width && size.Height == _currentSize.Height)
        {
            return;
        }
        _deviceContext.ClearState();

        if (_isSwapChainComposition)
        {
            _swapchain.ResizeBuffers((uint)size.Width, (uint)size.Height);
        }
        else
        {
            _swapchain.ResizeBuffers(0, 0);
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

    public void DrawIndexed(uint indexCount, uint startIndexLocation, int baseVertexLocation)
    {
        _deviceContext.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
    }

    public void ResetViewport()
    {
        SetViewport(_currentSize.Width, _currentSize.Height);
    }

    [MemberNotNull(nameof(_swapchain))]
    private unsafe void InitSwapChain(IWindow window, bool forceDxvk)
    {
        if (window is ICompositionWindow)
        {
            _isSwapChainComposition = true;
            _swapchain = CreateSwapChain((uint)window.Size.Width, (uint)window.Size.Height, windowPtr: 0, forceDxvk);
            // Ensure that DXGI does not queue more than one frame at a time. This both reduces 
            // latency and ensures that the application will only render after each VSync, minimizing 
            // power consumption.
            _graphicDevice.SetMaximumFrameLatency(1);
        }
        else
        {
            _swapchain = CreateSwapChain((uint)window.Size.Width, (uint)window.Size.Height, window.NativeHandle!.Value, forceDxvk);
        }
    }

    private D3D11SwapChain CreateSwapChain(uint width, uint heigth, nint windowPtr, bool forceDxvk)
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
            return _graphicRessourceFactory.SwapChainFactory.CreateSwapChainForHwnd(windowPtr, in swapChainDesc, in swapChainFullscreenDesc, ref Unsafe.NullRef<IDXGIOutput>());
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
            return _graphicRessourceFactory.SwapChainFactory.CreateSwapChainForComposition(in swapChainDesc, ref Unsafe.NullRef<IDXGIOutput>());
        }
    }

    [MemberNotNull(nameof(_currentSize))]
    private unsafe void InitView(IWindow window)
    {
        InitView(window.Size.Width, window.Size.Height);
    }

    [MemberNotNull(nameof(_currentSize))]
    private void InitView(float width, float height)
    {
        SetViewport((int)width, (int)height);

        _currentSize = new Size(width: (int)width, height: (int)height);
    }

    private void SetViewport(int width, int height)
    {
        // Set the rasterizer state with the current viewport.
        Viewport viewport = new(0, 0, width, height, 0, 1);
        RasterizerStage.SetViewports(1, in viewport);
    }

    public void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null

            _swapchain.Dispose();
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
