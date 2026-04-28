using System.Runtime.CompilerServices;
using PetitMoteur3D.Graphics.Shaders;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics;

internal sealed class D3D11GraphicPipelineState
{
    public VertexShader VertexShader { get; set; }
    public ComPtr<ID3D11GeometryShader> GeometryShader { get; set; }
    public PixelShader? PixelShader { get; set; }

    public ComPtr<ID3D11RasterizerState> RasterizerState { get; set; }
    public ComPtr<ID3D11DepthStencilState> DepthStencilState { get; set; }

    public RenderTarget RenderTargets => _renderTargets;

    private readonly D3D11GraphicPipeline _graphicPipeline;
    private readonly RenderTarget _renderTargets;

    /// <summary>
    /// 
    /// </summary>
    internal D3D11GraphicPipelineState(D3D11GraphicPipeline graphicPipelie, RenderTarget renderTarget)
    {
        _graphicPipeline = graphicPipelie;
        _renderTargets = renderTarget;
    }

    public void Bind()
    {
        _graphicPipeline.RasterizerStage.SetState(RasterizerState);
        _graphicPipeline.OutputMergerStage.SetDepthStencilState(DepthStencilState);

        _renderTargets.Bind(_graphicPipeline);
        VertexShader.Bind(_graphicPipeline);
        VertexShader.BindLayout(_graphicPipeline);
        _graphicPipeline.GeometryShaderStage.SetShader(GeometryShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
        if (PixelShader is not null)
        {
            PixelShader.Bind(_graphicPipeline);
        }
        else
        {
            _graphicPipeline.PixelShaderStage.SetNoShader();
        }
    }
}
