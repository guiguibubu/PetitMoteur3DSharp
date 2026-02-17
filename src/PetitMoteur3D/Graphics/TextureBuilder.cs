using System;
using System.Data;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics;

internal interface ITextureBuilder<TBuilder>
{
    TBuilder InitializeWith(SubresourceData initialData);
    TBuilder WithName(string name);
    TBuilder WithShaderRessourceView(ShaderResourceViewDesc desc);
    TBuilder WithDepthStencilView(DepthStencilViewDesc desc);
    Texture Build();
}

internal sealed class TextureBuilder : ITextureBuilder<TextureBuilder>
{
    private readonly Texture2DDesc _textureDesc;
    private ComPtr<ID3D11Texture2D>? _texture;
    private SubresourceData? _textureInitialData;
    private ShaderResourceViewDesc? _shaderRessourceViewDesc;
    private DepthStencilViewDesc? _depthStencilViewDesc;
    private bool _createRenderTargetView;
    private string _name;

    private readonly TextureFactory _textureFactory;

    public TextureBuilder(TextureFactory textureFactory, Texture2DDesc textureDesc)
    {
        _textureFactory = textureFactory;
        _textureDesc = textureDesc;
        _name = "";
        _texture = null;
        _textureInitialData = null;
        _shaderRessourceViewDesc = null;
        _depthStencilViewDesc = null;
        _createRenderTargetView = false;
    }

    public TextureBuilder(TextureFactory textureFactory, Texture2DDesc textureDesc, ComPtr<ID3D11Texture2D> texture)
    {
        _textureFactory = textureFactory;
        _textureDesc = textureDesc;
        _name = "";
        _texture = texture;
        _textureInitialData = null;
        _shaderRessourceViewDesc = null;
        _depthStencilViewDesc = null;
        _createRenderTargetView = false;
    }

    public Texture Build()
    {
        ComPtr<ID3D11Texture2D> texture;
        bool textureOwner = true;
        if(_texture is not null)
        {
            texture = _texture.Value;
            textureOwner = false;   
        }
        else if (_textureInitialData is not null)
        {
            texture = _textureFactory.CreateTexture2D(in _textureDesc, in (Nullable.GetValueRefOrDefaultRef(ref _textureInitialData)));
        }
        else
        {
            texture = _textureFactory.CreateEmptyTexture2D(in _textureDesc);
        }

        ComPtr<ID3D11ShaderResourceView> textureView;
        if (_shaderRessourceViewDesc is null)
        {
            textureView = new ComPtr<ID3D11ShaderResourceView>();
        }
        else
        {
            textureView = _textureFactory.CreateShaderResourceView(texture, in (Nullable.GetValueRefOrDefaultRef(ref _shaderRessourceViewDesc)));
        }

        ComPtr<ID3D11DepthStencilView> depthStencilView;
        if (_depthStencilViewDesc is null)
        {
            depthStencilView = new ComPtr<ID3D11DepthStencilView>();
        }
        else
        {
            depthStencilView = _textureFactory.CreateDepthStencilView(texture, in (Nullable.GetValueRefOrDefaultRef(ref _depthStencilViewDesc)));
        }

        ComPtr<ID3D11RenderTargetView> renderTargetView;
        if (_createRenderTargetView)
        {
            renderTargetView = _textureFactory.CreateRenderTargetView(texture);
        }
        else
        {
            renderTargetView = new ComPtr<ID3D11RenderTargetView>();
        }

        Texture textureResult = new(_name, (int)_textureDesc.Width, (int)_textureDesc.Height, texture, textureOwner);
        textureResult.SetTextureView(textureView);
        textureResult.SetTextureDepthStencilView(depthStencilView);
        textureResult.SetTextureRenderTargetView(renderTargetView);
        return textureResult;
    }

    public TextureBuilder InitializeWith(SubresourceData initialData)
    {
        _textureInitialData = initialData;
        return this;
    }

    public TextureBuilder WithDepthStencilView(DepthStencilViewDesc desc)
    {
        _depthStencilViewDesc = desc;
        return this;
    }

    public TextureBuilder WithRenderTargetView()
    {
        _createRenderTargetView = true;
        return this;
    }

    public TextureBuilder WithShaderRessourceView(ShaderResourceViewDesc desc)
    {
        _shaderRessourceViewDesc = desc;
        return this;
    }

    public TextureBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
}
