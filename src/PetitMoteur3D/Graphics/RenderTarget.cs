using System;
using PetitMoteur3D.Graphics.Stages;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics;

internal sealed class RenderTarget
{
    public string Name { get; init; }

    private Texture?[] _renderTargets;
    private Texture? _depthTexture;

    ComPtr<ID3D11RenderTargetView>[] _rtvs;
    ComPtr<ID3D11DepthStencilView> _dsv;

    private const uint NbRenderTargets = OutputMergerStage.NbRenderTargets;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public RenderTarget(string name = "")
    : this(Array.Empty<Texture?>(), null, name)
    { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public RenderTarget(Texture?[] textures, Texture? depthTexture, string name = "")
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)textures.Length, NbRenderTargets, nameof(textures));
        if (string.IsNullOrEmpty(name))
        {
            Name = GetType().Name + "_" + Guid.NewGuid().ToString();
        }
        else
        {
            Name = name;
        }

        _renderTargets = new Texture?[NbRenderTargets];
        _rtvs = new ComPtr<ID3D11RenderTargetView>[NbRenderTargets];
        SetRenderTarget(textures);
        _depthTexture = depthTexture;
        _dsv = _depthTexture?.DepthStencilView?.NativeHandle ?? (ComPtr<ID3D11DepthStencilView>)null;
    }

    public void SetRenderTarget(uint slot, Texture? texture)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, NbRenderTargets, nameof(slot));
        _renderTargets[slot] = texture;
        _rtvs[slot] = texture?.RenderTargetView?.NativeHandle ?? (ComPtr<ID3D11RenderTargetView>)null;
    }

    public void SetRenderTarget(Texture?[] textures)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)textures.Length, NbRenderTargets, nameof(textures));
        for (int i = 0; i < textures.Length; i++)
        {
            Texture? texture = textures[i];
            _renderTargets[i] = texture;
            _rtvs[i] = texture?.RenderTargetView?.NativeHandle ?? (ComPtr<ID3D11RenderTargetView>)null;
        }
        for (int i = textures.Length; i < NbRenderTargets; i++)
        {
            _renderTargets[i] = null;
            _rtvs[i] = (ComPtr<ID3D11RenderTargetView>)null;
        }
    }

    public void SetDepth(Texture? texture)
    {
        _depthTexture = texture;
        _dsv = _depthTexture?.DepthStencilView?.NativeHandle ?? (ComPtr<ID3D11DepthStencilView>)null;
    }

    public unsafe void ClearRenderTarget(ComPtr<ID3D11DeviceContext> deviceContext, float[] colorRGBA)
    {
        foreach (ComPtr<ID3D11RenderTargetView> renderTarget in _rtvs)
        {
            if (renderTarget.Handle is not null)
            {
                deviceContext.ClearRenderTargetView(renderTarget.Handle, colorRGBA.AsSpan());
            }
        }
    }

    public void ClearDepthStencil(ComPtr<ID3D11DeviceContext> deviceContext)
    {
        deviceContext.ClearDepthStencilView(_dsv, (uint)(ClearFlag.Depth | ClearFlag.Stencil), 1.0f, 0);
    }

    public void ResetNativeCache()
    {
        for (int i = 0; i < _renderTargets.Length; i++)
        {
            Texture? texture = _renderTargets[i];
            _rtvs[i] = texture?.RenderTargetView?.NativeHandle ?? (ComPtr<ID3D11RenderTargetView>)null;
        }
        for (int i = _renderTargets.Length; i < NbRenderTargets; i++)
        {
            _rtvs[i] = (ComPtr<ID3D11RenderTargetView>)null;
        }
        _dsv = _depthTexture?.DepthStencilView?.NativeHandle ?? (ComPtr<ID3D11DepthStencilView>)null;
    }

    public void Bind(D3D11GraphicPipeline graphicPipeline)
    {
        graphicPipeline.OutputMergerStage.SetRenderTarget(NbRenderTargets, in _rtvs[0], _dsv);
    }
}
