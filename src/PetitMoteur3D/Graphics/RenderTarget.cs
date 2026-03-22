using System;
using System.Linq;
using PetitMoteur3D.Graphics.Stages;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics;

internal sealed class RenderTarget
{
    public string Name { get; init; }

    private Texture?[] _renderTargets;
    private Texture? _depthTexture;

    private const uint NbRenderTargets = OutputMergerStage.NbRenderTargets;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public unsafe RenderTarget(string name = "")
    : this(Array.Empty<Texture?>(), null, name)
    { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public unsafe RenderTarget(Texture?[] textures, Texture? depthTexture, string name = "")
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
        for (int i = 0; i < textures.Length; i++)
        {
            _renderTargets[i] = textures[i];
        }
        for (int i = textures.Length; i < NbRenderTargets; i++)
        {
            _renderTargets[i] = null;
        }
        _depthTexture = depthTexture;
    }

    public void SetRenderTarget(uint slot, Texture? texture)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, NbRenderTargets, nameof(slot));
        _renderTargets[slot] = texture;
    }

    public void SetRenderTarget(Texture?[] textures)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)textures.Length, NbRenderTargets, nameof(textures));
        for (int i = 0; i < textures.Length; i++)
        {
            _renderTargets[i] = textures[i];
        }
        for (int i = textures.Length; i < NbRenderTargets; i++)
        {
            _renderTargets[i] = null;
        }
    }

    public void SetDepth(Texture? texture)
    {
        _depthTexture = texture;
    }

    public unsafe void ClearRenderTarget(ComPtr<ID3D11DeviceContext> deviceContext, float[] colorRGBA)
    {
        foreach (Texture? renderTarget in _renderTargets.Where(t => t is not null))
        {
            deviceContext.ClearRenderTargetView(renderTarget!.RenderTargetView, colorRGBA.AsSpan());
        }
    }

    public unsafe void ClearDepthStencil(ComPtr<ID3D11DeviceContext> deviceContext)
    {
        if (_depthTexture is not null)
        {
            deviceContext.ClearDepthStencilView(_depthTexture!.DepthStencilView, (uint)(ClearFlag.Depth | ClearFlag.Stencil), 1.0f, 0);
        }
    }

    public void Bind(D3D11GraphicPipeline graphicPipeline)
    {
        ComPtr<ID3D11RenderTargetView>[] rtvs = _renderTargets.Select(t => t?.RenderTargetView ?? (ComPtr<ID3D11RenderTargetView>)null).ToArray();
        ComPtr<ID3D11DepthStencilView> dsv = _depthTexture?.DepthStencilView ?? (ComPtr<ID3D11DepthStencilView>)null;
        graphicPipeline.OutputMergerStage.SetRenderTarget(NbRenderTargets, in rtvs[0], dsv);
    }
}
