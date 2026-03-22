using Silk.NET.Direct3D11;
using System;

namespace PetitMoteur3D.Graphics.RenderTechniques;

internal sealed class CopyTextureRenderPass : IRenderPass
{
    public bool IsEnabled { get; set; } = true;
    public string Name { get; }

    Texture _textureSource;
    Texture _textureTarget;
    D3D11GraphicPipeline _graphicPipeline;

    public CopyTextureRenderPass(D3D11GraphicPipeline graphicPipeline, Texture textureSource, Texture textureTarget, string name = "")
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
        _textureSource = textureSource;
        _textureTarget = textureTarget;
    }

    #region Public methods
    public void Render(Scene scene)
    {
        CopyTexture();
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
    private void CopyTexture()
    {
        _graphicPipeline.DeviceContext.CopyResource(_textureTarget.TextureRessource, _textureSource.TextureRessource);
    }
    #endregion
}
