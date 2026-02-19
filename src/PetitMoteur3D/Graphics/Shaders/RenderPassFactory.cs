using System;
using System.Collections.Generic;

namespace PetitMoteur3D.Graphics.Shaders;

internal class RenderPassFactory
{
    private readonly D3D11GraphicPipeline _graphicPipeline;
    private readonly Dictionary<RenderPassKey, BaseRenderPass> _renderPassCache;

    public RenderPassFactory(D3D11GraphicPipeline graphicPipeline)
    {
        _graphicPipeline = graphicPipeline;
        _renderPassCache = new Dictionary<RenderPassKey, BaseRenderPass>();
    }

    public T Create<T>(string name = "") where T : BaseRenderPass
    {
        Type type = typeof(T);
        RenderPassKey key = new(name, type.FullName!);
        if (!_renderPassCache.TryGetValue(key, out BaseRenderPass? renderPass))
        {
            if (type == typeof(DepthTestRenderPass))
            {
                renderPass = new DepthTestRenderPass(_graphicPipeline, name);
            }
            else if (type == typeof(ForwardOpaqueRenderPass))
            {
                renderPass = new ForwardOpaqueRenderPass(_graphicPipeline, name);
            }
            else if (type == typeof(DeferredGeometryRenderPass))
            {
                renderPass = new DeferredGeometryRenderPass(_graphicPipeline, name);
            }
            else if (type == typeof(DeferredLightningRenderPass))
            {
                renderPass = new DeferredLightningRenderPass(_graphicPipeline, name);
            }
            else if (type == typeof(ShadowMapRenderPass))
            {
                renderPass = new ShadowMapRenderPass(_graphicPipeline, name);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
            _renderPassCache.Add(key, renderPass);
        }
        return renderPass as T ?? throw new InvalidCastException();
    }

    private record struct RenderPassKey
    {
        public string Name { get; init; }
        public string TypeFullName { get; init; }
        public RenderPassKey(string name, string typeFullName)
        {
            Name = name;
            TypeFullName = typeFullName;
        }
    }
}
