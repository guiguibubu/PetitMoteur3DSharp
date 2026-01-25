using System;

namespace PetitMoteur3D.Graphics.Shaders;

internal class RenderPassFactory
{
    private readonly D3D11GraphicPipeline _graphicPipeline;

    public RenderPassFactory(D3D11GraphicPipeline graphicPipeline)
    {
        _graphicPipeline = graphicPipeline;
    }

    public T Create<T>(string name = "") where T : BaseRenderPass
    {
        if (typeof(T) == typeof(DepthTestRenderPass))
        {
            return new DepthTestRenderPass(_graphicPipeline, name) as T ?? throw new InvalidCastException();
        }
        else if (typeof(T) == typeof(MiniPhongNormalMapRenderPass))
        {
            return new MiniPhongNormalMapRenderPass(_graphicPipeline, name) as T ?? throw new InvalidCastException();
        }
        else if (typeof(T) == typeof(MiniPhongRenderPass))
        {
            return new MiniPhongRenderPass(_graphicPipeline, name) as T ?? throw new InvalidCastException();
        }
        else if (typeof(T) == typeof(ShadowMapRenderPass))
        {
            return new ShadowMapRenderPass(_graphicPipeline, name) as T ?? throw new InvalidCastException();
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }
}
