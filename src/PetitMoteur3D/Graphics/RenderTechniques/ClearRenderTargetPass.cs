using System;
using System.Numerics;
using static PetitMoteur3D.Graphics.RenderTechniques.ClearRenderTargetPass;

namespace PetitMoteur3D.Graphics.RenderTechniques;

internal sealed class ClearRenderTargetPass : IRenderPass
{
    public bool IsEnabled { get; set; } = true;
    public string Name { get; }

    RenderTarget _renderTarget;
    D3D11GraphicPipeline _graphicPipeline;
    ClearOption _clearOption;

    [Flags]
    public enum ClearOption
    {
        None = 0,
        RenderTarget = 1 << 0,
        DepthStencil = 1 << 1,
        RenderTargetAndDepthStencil = RenderTarget | DepthStencil
    }

    public ClearRenderTargetPass(D3D11GraphicPipeline graphicPipeline, RenderTarget renderTarget, ClearOption clearOption = ClearOption.RenderTargetAndDepthStencil, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            Name = GetType().Name + "_" + Guid.NewGuid().ToString();
        }
        else
        {
            Name = name;
        }

        _graphicPipeline = graphicPipeline;
        _renderTarget = renderTarget;
        _clearOption = clearOption;
    }

    #region Public methods
    public void Render(Scene scene)
    {
        ClearRenderTarget();
    }

    public void Render(BaseObjet3D objet)
    {
    }

    public void Render(SubObjet3D subObjet3D)
    {
    }
    #endregion

    #region Protected methods
    #endregion

    #region Private methods
    private void ClearRenderTarget()
    {
        _graphicPipeline.GetBackgroundColour(out Vector4 backgroundColor);
        if (_clearOption.HasFlag(ClearOption.RenderTarget))
        {
            _renderTarget.ClearRenderTarget(_graphicPipeline.DeviceContext, [backgroundColor.X, backgroundColor.Y, backgroundColor.Z, backgroundColor.W]);
        }
        if (_clearOption.HasFlag(ClearOption.DepthStencil))
        {
            _renderTarget.ClearDepthStencil(_graphicPipeline.DeviceContext);
        }
    }
    #endregion
}
