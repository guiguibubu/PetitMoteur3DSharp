using System;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D.Graphics;

internal sealed class D3D11Texture2D : IDisposable
{
    public ComPtr<ID3D11Texture2D> NativeHandle => _nativeHandle;
    public ref ComPtr<ID3D11Texture2D> NativeHandleRef => ref _nativeHandle;
    public string Name => _name;
    public Texture2DDesc Description => _description;

    private ComPtr<ID3D11Texture2D> _nativeHandle;
    private string _name;
    private Texture2DDesc _description;

    private bool _ressourceOwner;

    private bool _disposed;

    public D3D11Texture2D(ComPtr<ID3D11Texture2D> handle, bool ressourceOwner, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            _name = this.GetType().Name + "_" + Guid.NewGuid().ToString();
        }
        else
        {
            _name = name;
        }
        _nativeHandle = handle;
        SetDebugName();
        _description = new Texture2DDesc();
        UpdateDesc();
        _ressourceOwner = ressourceOwner;
        _disposed = false;
    }

    public void UpdateHandle(ComPtr<ID3D11Texture2D> handle)
    {
        if (_ressourceOwner)
        {
            _nativeHandle.Dispose();
        }
        _nativeHandle = handle;
        SetDebugName();
        UpdateDesc();
    }

    private unsafe void SetDebugName()
    {
        if (_nativeHandle.Handle is not null && !string.IsNullOrEmpty(Name))
        {
            // Set Debug Name
            using (GlobalMemory unmanagedName = SilkMarshal.StringToMemory(_name, NativeStringEncoding.Ansi))
            {
                nint namePtr = unmanagedName.Handle;
                fixed (Guid* guidPtr = &Windows.Win32.PInvoke.WKPDID_D3DDebugObjectName)
                {
                    _nativeHandle.SetPrivateData(guidPtr, (uint)_name.Length, namePtr.ToPointer());
                }
            }
        }
    }

    private void UpdateDesc()
    {
        _nativeHandle.GetDesc(ref _description);
    }

    private void Dispose(bool disposing)
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
            if (_ressourceOwner)
            {
                _nativeHandle.Dispose();
            }
            _disposed = true;
        }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~D3D11Texture2D()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
