using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics;

internal class TextureManager
{
    public TextureFactory Factory => _textureFactory;

    private TextureFactory _textureFactory;
    private readonly Dictionary<string, Texture> _textures = new();

    public TextureManager(ComPtr<ID3D11Device> device)
    {
        _textureFactory = new(device);
    }

    public Texture GetOrLoadTexture(string fileName)
    {
        if (_textures.ContainsKey(fileName))
        {
            return _textures[fileName];
        }
        else
        {
            Texture texture = _textureFactory.FromFile(fileName);
            _textures.Add(fileName, texture);
            return texture;
        }
    }

    public Texture GetOrCreateTexture(string name, nint pixels, int width, int height, int bytesPerPixel)
    {
        if (_textures.ContainsKey(name))
        {
            return _textures[name];
        }
        else
        {
            Texture texture = _textureFactory.Create(name, pixels, width, height, bytesPerPixel);
            _textures.Add(name, texture);
            return texture;
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

    public bool TryGet(string name, [MaybeNullWhen(false)] out Texture? texture)
    {
        return _textures.TryGetValue(name, out texture);
    }
}