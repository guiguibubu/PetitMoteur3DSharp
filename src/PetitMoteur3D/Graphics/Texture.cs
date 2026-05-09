using System;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D.Graphics;

internal sealed class Texture : IDisposable
{
    public string Name { get; private set; }
    public uint Width { get; private set; }
    public uint Height { get; private set; }
    public D3D11Texture2D TextureRessource { get { return _texture; } }
    private D3D11Texture2D _texture;
    public D3D11ShaderResourceView? ShaderRessourceView { get { return _shaderRessourceView; } }
    private D3D11ShaderResourceView? _shaderRessourceView;
    public D3D11DepthStencilView? DepthStencilView { get { return _depthStencilView; } }
    private D3D11DepthStencilView? _depthStencilView;
    public D3D11RenderTargetView? RenderTargetView { get { return _renderTargetView; } }
    private D3D11RenderTargetView? _renderTargetView;

    public bool TextureOwner => _textureOwner;

    private readonly bool _textureOwner;

    private bool _disposed;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="width"></param>
    /// <param name="heigth"></param>
    /// <param name="texture">Released when Texture is disposed</param>
    public Texture(string name, D3D11Texture2D texture, bool textureOwner = true)
    {
        ArgumentNullException.ThrowIfNull(texture);

        Name = name;
        Width = texture.Description.Width;
        Height = texture.Description.Height;
        _texture = texture;
        _shaderRessourceView = null;
        _depthStencilView = null;
        _renderTargetView = null;

        _textureOwner = textureOwner;

        _disposed = false;
    }

    public void SetTextureRessource(D3D11Texture2D textureRessource)
    {
        ArgumentNullException.ThrowIfNull(textureRessource);
        if (_textureOwner)
        {
            _texture.Dispose();
        }
        _texture = textureRessource;
        Width = _texture.Description.Width;
        Height = _texture.Description.Height;
    }

    public void SetTextureRessource(ComPtr<ID3D11Texture2D> textureRessource)
    {
        _texture.UpdateHandle(textureRessource);
        Width = _texture.Description.Width;
        Height = _texture.Description.Height;
    }

    public void SetTextureView(D3D11ShaderResourceView? textureView)
    {
        if (_shaderRessourceView is not null)
        {
            _shaderRessourceView.Dispose();
        }
        _shaderRessourceView = textureView;
    }

    public void SetTextureView(ComPtr<ID3D11ShaderResourceView> textureView)
    {
        if (_shaderRessourceView is null)
        {
            _shaderRessourceView = new D3D11ShaderResourceView(textureView, Name + "_ShaderRessourceView");
        }
        else
        {
            _shaderRessourceView.UpdateHandle(textureView);
        }
    }

    public void SetTextureDepthStencilView(D3D11DepthStencilView? textureDepthStencilView)
    {
        if (_depthStencilView is not null)
        {
            _depthStencilView.Dispose();
        }
        _depthStencilView = textureDepthStencilView;
    }

    public void SetTextureDepthStencilView(ComPtr<ID3D11DepthStencilView> textureDepthStencilView)
    {
        if (_depthStencilView is null)
        {
            _depthStencilView = new D3D11DepthStencilView(textureDepthStencilView, Name + "_DepthStencilView");
        }
        else
        {
            _depthStencilView.UpdateHandle(textureDepthStencilView);
        }
    }

    public void SetTextureRenderTargetView(D3D11RenderTargetView? textureRenderTargetView)
    {
        if (_renderTargetView is not null)
        {
            _renderTargetView.Dispose();
        }
        _renderTargetView = textureRenderTargetView;
    }

    public void SetTextureRenderTargetView(ComPtr<ID3D11RenderTargetView> textureRenderTargetView)
    {
        if (_renderTargetView is null)
        {
            _renderTargetView = new D3D11RenderTargetView(textureRenderTargetView, Name + "_RenderTargetView");
        }
        else
        {
            _renderTargetView.UpdateHandle(textureRenderTargetView);
        }
    }

    public void ReleaseViews()
    {
        _shaderRessourceView?.Dispose();
        _depthStencilView?.Dispose();
        _renderTargetView?.Dispose();
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
            ReleaseViews();

            // Note disposing has been done.
            _disposed = true;
        }
    }
}