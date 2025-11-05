using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics;

internal sealed class TextureManager
{
    public TextureFactory Factory => _textureFactory;

    private readonly TextureFactory _textureFactory;
    private readonly Dictionary<string, Texture> _textures;

    public TextureManager(ComPtr<ID3D11Device> device)
    {
        _textureFactory = new TextureFactory(device);
        _textures = new Dictionary<string, Texture>();
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
}