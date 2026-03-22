using System;
using System.Runtime.CompilerServices;
using PetitMoteur3D.Graphics.Buffers;
using PetitMoteur3D.Graphics.Shaders;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D.Graphics;

internal sealed class GraphicDeviceRessourceFactory : IDisposable
{
    public GraphicBufferFactory BufferFactory => _bufferFactory;
    public ShaderFactory ShaderFactory => _shaderFactory;
    public D3D11SwapChainFactory SwapChainFactory => _swapChainFactory;
    public TextureManager TextureManager => _textureManager;

    private readonly GraphicBufferFactory _bufferFactory;
    private readonly ShaderFactory _shaderFactory;
    private readonly D3D11SwapChainFactory _swapChainFactory;
    private readonly TextureManager _textureManager;
    private bool _disposed;

    public GraphicDeviceRessourceFactory(D3D11GraphicDevice graphicDevice)
        : this(graphicDevice, new GraphicBufferFactory(graphicDevice), new ShaderFactory(new ShaderManager(graphicDevice.Device)), new TextureManager(graphicDevice.Device))
    { }

    public GraphicDeviceRessourceFactory(D3D11GraphicDevice graphicDevice, GraphicBufferFactory bufferFactory, ShaderFactory shaderFactory, TextureManager textureManager)
    {
        _bufferFactory = bufferFactory;
        _shaderFactory = shaderFactory;
        _swapChainFactory = new D3D11SwapChainFactory(graphicDevice, textureManager);
        _textureManager = textureManager;
        _disposed = false;
    }

    public TextureBuilder CreateBuilder(Texture2DDesc textureDesc)
    {
        return new TextureBuilder(_textureManager.Factory, textureDesc);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _shaderFactory.Dispose();
                _textureManager.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~GraphicDeviceRessourceFactory()
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
