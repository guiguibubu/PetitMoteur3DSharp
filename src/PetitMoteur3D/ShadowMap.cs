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
    public ref readonly ComPtr<ID3D11SamplerState> SampleState { get { return ref _sampleState; } }
    public Texture DepthTexture { get { return _depthTexture; } }
    #endregion

    private Texture _depthTexture; // texture de profondeur 

    private ComPtr<ID3D11SamplerState> _sampleState;

    private readonly TextureManager _textureManager;

    private readonly string _name;

    public ShadowMap(TextureManager textureManager, uint width, uint height, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            _name = this.GetType().Name + "_" + Guid.NewGuid().ToString();
        }
        else
        {
            _name = name;
        }

        _textureManager = textureManager;

        Initialisation(width, height);
    }

    [MemberNotNull(nameof(_depthTexture))]
    private void Initialisation(uint width, uint height)
    {
        InitTexture(_textureManager);
        InitDepthBuffer(width, height);
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
        _sampleState = textureManager.Factory.CreateSampler(samplerDesc);
    }

    [MemberNotNull(nameof(_depthTexture))]
    private unsafe void InitDepthBuffer(uint width, uint height)
    {
        Texture2DDesc depthTextureDesc = new()
        {
            Width = width,
            Height = height,
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
        InitDepthBuffer((uint)newSize.Width, (uint)newSize.Height);
    }
}
