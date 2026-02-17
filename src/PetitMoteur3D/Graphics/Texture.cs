using System;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D.Graphics;

internal sealed class Texture : IDisposable
{
    public string Name { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public ComPtr<ID3D11Texture2D> TextureRessource { get { return _texture; } }
    private readonly ComPtr<ID3D11Texture2D> _texture;
    public ComPtr<ID3D11ShaderResourceView> ShaderRessourceView { get { return _shaderRessourceView; } }
    private ComPtr<ID3D11ShaderResourceView> _shaderRessourceView;
    public ComPtr<ID3D11DepthStencilView> TextureDepthStencilView { get { return _textureDepthStencilView; } }
    private ComPtr<ID3D11DepthStencilView> _textureDepthStencilView;
    public ComPtr<ID3D11RenderTargetView> RenderTargetView { get { return _renderTargetView; } }
    public ref ComPtr<ID3D11RenderTargetView> RenderTargetViewRef { get { return ref _renderTargetView; } }
    private ComPtr<ID3D11RenderTargetView> _renderTargetView;

    private bool _textureOwner;

    private bool _disposed;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="width"></param>
    /// <param name="heigth"></param>
    /// <param name="texture">Released when Texture is disposed</param>
    public unsafe Texture(string name, int width, int heigth, ComPtr<ID3D11Texture2D> texture, bool textureOwner = true)
    {
        Name = name;
        Width = width;
        Height = heigth;
        _texture = texture;
        _shaderRessourceView = null;
        _textureDepthStencilView = null;
        _renderTargetView = null;

        _textureOwner = textureOwner;

        if (!string.IsNullOrEmpty(Name))
        {
            // Set Debug Name
            using (GlobalMemory unmanagedName = SilkMarshal.StringToMemory(name, NativeStringEncoding.Ansi))
            {
                nint namePtr = unmanagedName.Handle;
                fixed (Guid* guidPtr = &Windows.Win32.PInvoke.WKPDID_D3DDebugObjectName)
                {
                    _texture.SetPrivateData(guidPtr, (uint)name.Length, namePtr.ToPointer());
                }
            }
        }
        _disposed = false;
    }

    public void SetTextureView(ComPtr<ID3D11ShaderResourceView> textureView)
    {
        _shaderRessourceView = textureView;
    }
    public void SetTextureDepthStencilView(ComPtr<ID3D11DepthStencilView> textureDepthStencilView)
    {
        _textureDepthStencilView = textureDepthStencilView;
    }
    public void SetTextureRenderTargetView(ComPtr<ID3D11RenderTargetView> textureRenderTargetView)
    {
        _renderTargetView = textureRenderTargetView;
    }

    ~Texture()
    {
        Dispose(disposing: false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        // This object will be cleaned up by the Dispose method.
        // Therefore, you should call GC.SuppressFinalize to
        // take this object off the finalization queue
        // and prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
    }

    // Dispose(bool disposing) executes in two distinct scenarios.
    // If disposing equals true, the method has been called directly
    // or indirectly by a user's code. Managed and unmanaged resources
    // can be disposed.
    // If disposing equals false, the method has been called by the
    // runtime from inside the finalizer and you should not reference
    // other objects. Only unmanaged resources can be disposed.
    private void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (!_disposed)
        {
            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources.
            }

            // Call the appropriate methods to clean up
            // unmanaged resources here.
            // If disposing is false,
            // only the following code is executed.
            if (_textureOwner)
            {
                _texture.Dispose();
            }
            _shaderRessourceView.Dispose();
            _textureDepthStencilView.Dispose();
            _renderTargetView.Dispose();

            // Note disposing has been done.
            _disposed = true;
        }
    }
}