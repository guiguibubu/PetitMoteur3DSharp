using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.RenderTechniques;

internal sealed class WireframeOpaqueRenderPass : ForwardOpaqueRenderPass
{
    public WireframeOpaqueRenderPass(D3D11GraphicPipeline graphicPipeline, RenderTarget renderTarget, string name = "")
        : base(graphicPipeline, renderTarget, name)
    {
    }

    #region Protected methods
    /// <inheritdoc/>
    protected override ComPtr<ID3D11RasterizerState> GetRasterizerState()
    {
        return GraphicPipeline.WireFrameCullBackRS;
    }
    #endregion
}
