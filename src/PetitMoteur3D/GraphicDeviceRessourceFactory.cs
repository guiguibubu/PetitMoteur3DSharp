using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D;

internal class GraphicDeviceRessourceFactory
{
    private readonly GraphicBufferFactory _bufferFactory;
    private readonly ShaderManager _shaderManager;
    private readonly TextureManager _textureManager;

    public GraphicBufferFactory BufferFactory => _bufferFactory;
    public ShaderManager ShaderManager => _shaderManager;
    public TextureManager TextureManager => _textureManager;

    public GraphicDeviceRessourceFactory(ComPtr<ID3D11Device> device, D3DCompiler compiler)
        : this(new GraphicBufferFactory(device), new ShaderManager(device, compiler), new TextureManager(device))
    { }

    public GraphicDeviceRessourceFactory(GraphicBufferFactory bufferFactory, ShaderManager shaderManager, TextureManager textureManager)
    {
        _bufferFactory = bufferFactory;
        _shaderManager = shaderManager;
        _textureManager = textureManager;
    }
}
