using System;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D.Graphics;

internal sealed class D3D11GraphicDevice : IDisposable
{
    public ref readonly ComPtr<ID3D11Device> Device { get { return ref _device; } }
    public ref readonly ComPtr<ID3D11DeviceContext> DeviceContext { get { return ref _deviceContext; } }
    public bool DxVk { get; }

    public GraphicDeviceRessourceFactory RessourceFactory { get; }

    private ComPtr<ID3D11Device> _device;
    private ComPtr<ID3D11DeviceContext> _deviceContext;
    private readonly D3D11 _d3d11Api;

    private static readonly D3DFeatureLevel[] FEATURES_LEVELS = {
        D3DFeatureLevel.Level111,
        D3DFeatureLevel.Level110
    };

    private bool _disposed;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="forceDxvk">Whether or not to force use of DXVK on platforms where native DirectX implementations are available</param>
    public D3D11GraphicDevice(bool forceDxvk = false)
    {
        DxVk = forceDxvk;

        // Create our D3D11 logical device.
#pragma warning disable CS0618 // Type or member is obsolete
        _d3d11Api = D3D11.GetApi(DXSwapchainProvider.Win32, forceDxvk);
#pragma warning restore CS0618 // Type or member is obsolete
        InitDevice(_d3d11Api);

        RessourceFactory = new(this);

        _disposed = false;
    }

    public void SetMaximumFrameLatency(uint maxLatency)
    {
        using ComPtr<IDXGIDevice1> dxgiDevice = _device.QueryInterface<IDXGIDevice1>();
        SilkMarshal.ThrowHResult
        (
            dxgiDevice.SetMaximumFrameLatency(maxLatency)
        );
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
                Software: IntPtr.Zero,
                createDeviceFlags,
                in FEATURES_LEVELS[0],
                FeatureLevels: 2,
                D3D11.SdkVersion,
                ref _device,
                pFeatureLevel: null,
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
                PetitMoteur3D.Logging.Log.Information(msgStr);
            });
        }
#endif
    }

    ~D3D11GraphicDevice()
    {
        Dispose(disposing: false);
    }

    private unsafe void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                RessourceFactory.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null

            if (_deviceContext.Handle is not null)
            {
                _deviceContext.ClearState();
            }
            _deviceContext.Dispose();
            _device.Dispose();
            _d3d11Api.Dispose(); // we need to keep it because in Release build, it can fail to load DXGI ressources id d3d11 is dispose too soon
            _disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
