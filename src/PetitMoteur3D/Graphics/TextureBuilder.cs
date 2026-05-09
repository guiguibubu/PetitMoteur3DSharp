using System;
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
    private Texture2DDesc _textureDesc;
    private D3D11Texture2D? _textureRessource;
    private Texture? _texture;
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
        _textureRessource = null;
        _textureInitialData = null;
        _shaderRessourceViewDesc = null;
        _depthStencilViewDesc = null;
        _createRenderTargetView = false;
    }

    public TextureBuilder(TextureFactory textureFactory, Texture2DDesc textureDesc, Texture texture)
    {
        _textureFactory = textureFactory;
        _textureDesc = textureDesc;
        _name = texture.Name;
        _texture = texture;
        _textureRessource = null;
        _textureInitialData = null;
        _shaderRessourceViewDesc = null;
        _depthStencilViewDesc = null;
        _createRenderTargetView = false;
    }

    public TextureBuilder(TextureFactory textureFactory, D3D11Texture2D textureRessource)
    {
        _textureFactory = textureFactory;
        _textureDesc = new Texture2DDesc();
        _name = "";
        _texture = null;
        _textureRessource = textureRessource;
        _textureInitialData = null;
        _shaderRessourceViewDesc = null;
        _depthStencilViewDesc = null;
        _createRenderTargetView = false;
    }

    public TextureBuilder(TextureFactory textureFactory, D3D11Texture2D textureRessource, Texture texture)
    {
        _textureFactory = textureFactory;
        _textureDesc = new Texture2DDesc();
        _name = texture.Name;
        _texture = texture;
        _textureRessource = textureRessource;
        _textureInitialData = null;
        _shaderRessourceViewDesc = null;
        _depthStencilViewDesc = null;
        _createRenderTargetView = false;
    }

    public Texture Build()
    {
        D3D11Texture2D texture;
        bool textureOwner = true;
        if (_textureRessource is not null)
        {
            texture = _textureRessource;
            textureOwner = false;
        }
        else if (_textureInitialData is not null)
        {
            texture = _textureFactory.CreateTexture2D(in _textureDesc, in (Nullable.GetValueRefOrDefaultRef(ref _textureInitialData)), _name + "_Texture");
        }
        else
        {
            texture = _textureFactory.CreateEmptyTexture2D(in _textureDesc, _name + "_Texture");
        }

        D3D11ShaderResourceView? textureView = null;
        if (_shaderRessourceViewDesc is not null)
        {
            textureView = _textureFactory.CreateShaderResourceView(texture.NativeHandle, in (Nullable.GetValueRefOrDefaultRef(ref _shaderRessourceViewDesc)), _name + "_ShaderRessourceView");
        }

        D3D11DepthStencilView? depthStencilView = null;
        if (_depthStencilViewDesc is not null)
        {
            depthStencilView = _textureFactory.CreateDepthStencilView(texture.NativeHandle, in (Nullable.GetValueRefOrDefaultRef(ref _depthStencilViewDesc)), _name + "_DepthStencilView");
        }

        D3D11RenderTargetView? renderTargetView = null;
        if (_createRenderTargetView)
        {
            renderTargetView = _textureFactory.CreateRenderTargetView(texture.NativeHandle, _name + "_RenderTargetView");
        }

        Texture textureResult;
        if (_texture is null)
        {
            textureResult = new Texture(_name, texture, textureOwner);
        }
        else
        {
            textureResult = _texture;
            textureResult.SetTextureRessource(texture);
        }
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
