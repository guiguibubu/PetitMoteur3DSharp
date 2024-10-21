using System.Collections.Generic;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D
{
    internal class TextureManager
    {
        private ComPtr<ID3D11Device> _device;
        private readonly Dictionary<string, Texture> _textures = new();

        public TextureManager(ComPtr<ID3D11Device> device)
        {
            _device = device;
        }

        public Texture GetOrLoadTexture(string fileName)
        {
            if (_textures.ContainsKey(fileName))
            {
                return _textures[fileName];
            }
            else
            {
                Texture texture = new(fileName, _device);
                _textures.Add(fileName, texture);
                return texture;
            }
        }
    }
}