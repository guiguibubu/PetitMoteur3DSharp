using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics;

internal sealed class TextureManager : IDisposable
{
    public TextureFactory Factory => _textureFactory;

    private readonly TextureFactory _textureFactory;
    private readonly Dictionary<string, Texture> _textures;
    private bool _disposed;

    public TextureManager(ComPtr<ID3D11Device> device)
    {
        _textureFactory = new TextureFactory(device);
        _textures = new Dictionary<string, Texture>();
        _disposed = false;
    }

    public Texture GetOrLoadTexture(string fileName)
    {
        if (TryGet(fileName, out Texture? texture))
        {
            return texture;
        }
        else
        {
            Texture newTexture = _textureFactory.FromFile(fileName);
            _textures.Add(fileName, newTexture);
            return newTexture;
        }
    }

    public Texture GetOrCreateTexture(string name, nint pixels, int width, int height, int bytesPerPixel)
    {
        if (TryGet(name, out Texture? texture))
        {
            return texture;
        }
        else
        {
            Texture newTexture = _textureFactory.Create(name, pixels, width, height, bytesPerPixel);
            _textures.Add(name, newTexture);
            return newTexture;
        }
    }

    public Texture GetOrCreateTexture(string name, Texture2DDesc textureDesc, Func<TextureBuilder, TextureBuilder> builderFunc)
    {
        if (TryGet(name, out Texture? texture))
        {
            return texture;
        }
        else
        {
            Texture newTexture = builderFunc.Invoke(_textureFactory.CreateBuilder(textureDesc).WithName(name)).Build();
            _textures.Add(name, newTexture);
            return newTexture;
        }
    }

    public Texture GetOrCreateEmptyTexture(string name, int width, int height)
    {
        if (TryGet(name, out Texture? texture))
        {
            return texture;
        }
        else
        {
            Texture newTexture = _textureFactory.CreateEmpty(name, width, height);
            _textures.Add(name, newTexture);
            return newTexture;
        }
    }

    public Texture GetOrCreateEmptyTexture(string name, int width, int height, in Texture2DDesc textureDesc)
    {
        if (TryGet(name, out Texture? texture))
        {
            return texture;
        }
        else
        {
            Texture newTexture = _textureFactory.CreateEmpty(name, width, height, in textureDesc);
            _textures.Add(name, newTexture);
            return newTexture;
        }
    }
    
    public Texture GetOrCreateEmptyTexture(string name, int width, int height, in Texture2DDesc textureDesc, in ShaderResourceViewDesc shaderResourceViewDesc)
    {
        if (TryGet(name, out Texture? texture))
        {
            return texture;
        }
        else
        {
            Texture newTexture = _textureFactory.CreateEmpty(name, width, height, in textureDesc, in shaderResourceViewDesc);
            _textures.Add(name, newTexture);
            return newTexture;
        }
    }

    public Texture? Get(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        if (TryGet(name, out Texture? texture))
        {
            return texture;
        }
        else
        {
            return null;
        }
    }

    public bool TryGet(string name, [NotNullWhen(true)][MaybeNullWhen(false)] out Texture? texture)
    {
        return _textures.TryGetValue(name, out texture);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                foreach (Texture texture in _textures.Values)
                {
                    texture.Dispose();
                }
                _textures.Clear();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~TextureManager()
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