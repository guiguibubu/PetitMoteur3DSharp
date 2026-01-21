namespace PetitMoteur3D.Graphics.Shaders;

internal class RenderPassFactory
{
    private readonly D3D11GraphicPipeline _graphicPipeline;

    public RenderPassFactory(D3D11GraphicPipeline graphicPipeline)
    {
        _graphicPipeline = graphicPipeline;
    }

    public DepthTestRenderPass CreateDepthTestRenderPass(string name = "")
    {
        return new DepthTestRenderPass(_graphicPipeline, name);
    }
    public MiniPhongNormalMapRenderPass CreateMiniPhongNormalMapRenderPass(string name = "")
    {
        return new MiniPhongNormalMapRenderPass(_graphicPipeline, name);
    }
    public MiniPhongRenderPass CreateMiniPhongRenderPass(string name = "")
    {
        return new MiniPhongRenderPass(_graphicPipeline, name);
    }
}
