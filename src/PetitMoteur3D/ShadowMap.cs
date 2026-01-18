using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using PetitMoteur3D.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D;

internal sealed class ShadowMap
{
    #region Public properties
    public ref readonly ComPtr<ID3D11InputLayout> VertexLayout { get { return ref _vertexLayout; } }
    public ref readonly ComPtr<ID3D11VertexShader> VertexShader { get { return ref _vertexShader; } }
    public ref readonly ComPtr<ID3D11SamplerState> SampleState { get { return ref _sampleState; } }
    public ref readonly Size Dimension { get { return ref _dimension; } }
    public Texture DepthTexture { get { return _depthTexture; } }
    #endregion

    private Texture _depthTexture; // texture de profondeur 

    private ComPtr<ID3D11VertexShader> _vertexShader;
    private ComPtr<ID3D11InputLayout> _vertexLayout;
    private ComPtr<ID3D11SamplerState> _sampleState;

    private readonly ShaderManager _shaderManager;
    private readonly TextureManager _textureManager;
    private readonly GraphicDeviceRessourceFactory _graphicRessourceFactory;

    private readonly string _name;
    private Size _dimension;

    public ShadowMap(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, Size size, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            _name = this.GetType().Name + "_" + Guid.NewGuid().ToString();
        }
        else
        {
            _name = name;
        }

        _graphicRessourceFactory = graphicDeviceRessourceFactory;
        _shaderManager = graphicDeviceRessourceFactory.ShaderManager;
        _textureManager = graphicDeviceRessourceFactory.TextureManager;

        _dimension = size;
        Initialisation();
    }

    [MemberNotNull(nameof(_depthTexture))]
    private void Initialisation()
    {
        InitShaders(_shaderManager);
        InitTexture(_textureManager);
        InitDepthBuffer(_dimension.Width, _dimension.Height);
    }

    private unsafe void InitShaders(ShaderManager shaderManager)
    {
        InitVertexShader(shaderManager);
    }

    private unsafe void InitTexture(TextureManager textureManager)
    {
        // Initialisation des paramètres de sampling de la texture
        SamplerDesc samplerDesc = new();
        samplerDesc.Filter = Filter.ComparisonMinMagMipPoint;
        samplerDesc.AddressU = TextureAddressMode.Border;
        samplerDesc.AddressV = TextureAddressMode.Border;
        samplerDesc.AddressW = TextureAddressMode.Border;
        samplerDesc.MipLODBias = 0f;
        samplerDesc.MaxAnisotropy = 0;
        samplerDesc.ComparisonFunc = ComparisonFunc.LessEqual;
        samplerDesc.MinLOD = 0;
        samplerDesc.MaxLOD = float.MaxValue;
        samplerDesc.BorderColor[0] = 1f;
        samplerDesc.BorderColor[1] = 1f;
        samplerDesc.BorderColor[2] = 1f;
        samplerDesc.BorderColor[3] = 1f;

        // Création de l’état de sampling
        _sampleState = textureManager.Factory.CreateSampler(samplerDesc, $"{_name}_SamplerState");
    }

    [MemberNotNull(nameof(_depthTexture))]
    private unsafe void InitDepthBuffer(int width, int height)
    {
        Texture2DDesc depthTextureDesc = new()
        {
            Width = (uint)width,
            Height = (uint)height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Silk.NET.DXGI.Format.FormatR24G8Typeless,
            SampleDesc = new Silk.NET.DXGI.SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)(BindFlag.DepthStencil | BindFlag.ShaderResource),
            CPUAccessFlags = 0,
            MiscFlags = 0
        };

        ShaderResourceViewDesc shaderResourceViewDesc = new()
        {
            Format = Silk.NET.DXGI.Format.FormatR24UnormX8Typeless,
            ViewDimension = D3DSrvDimension.D3DSrvDimensionTexture2D,
            Anonymous = new ShaderResourceViewDescUnion
            {
                Texture2D =
                {
                    MostDetailedMip = 0,
                    MipLevels = 1
                }
            }
        };

        DepthStencilViewDesc descDSView = new()
        {
            Format = Silk.NET.DXGI.Format.FormatD24UnormS8Uint,
            ViewDimension = DsvDimension.Texture2D,
            Texture2D = new Tex2DDsv() { MipSlice = 0 }
        };

        _depthTexture = _textureManager.GetOrCreateTexture($"{_name}_DepthTexture", depthTextureDesc, 
            (TextureBuilder builder) => builder
            .WithShaderRessourceView(shaderResourceViewDesc)
            .WithDepthStencilView(descDSView));
    }

    public void Resize(Size newSize)
    {
        _dimension = newSize;
        InitDepthBuffer(_dimension.Width, _dimension.Height);
    }

    /// <summary>
    /// Compilation et chargement du vertex shader
    /// </summary>
    private unsafe void InitVertexShader(ShaderManager shaderManager)
    {
        // Compilation et chargement du vertex shader
        string filePath = "shaders\\ShadowMap.hlsl";
        string entryPoint = "ShadowMapVS";
        string target = "vs_5_0";
        // #define D3DCOMPILE_ENABLE_STRICTNESS                    (1 << 11)
        uint flagStrictness = ((uint)1 << 11);
        // #define D3DCOMPILE_DEBUG (1 << 0)
        // #define D3DCOMPILE_SKIP_OPTIMIZATION                    (1 << 2)
#if DEBUG
        uint flagDebug = ((uint)1 << 0);
        uint flagSkipOptimization = ((uint)(1 << 2));
#else
        uint flagDebug = 0;
        uint flagSkipOptimization = 0;
#endif
        uint compilationFlags = flagStrictness | flagDebug | flagSkipOptimization;
        ShaderCodeFile shaderFile = new
        (
            filePath,
            entryPoint,
            target,
            compilationFlags,
            name: "ShadowMap_VertexShader"
        );
        shaderManager.GetOrLoadVertexShaderAndLayout(shaderFile, SommetShadowMap.InputLayoutDesc, ref _vertexShader, ref _vertexLayout);
    }
}
