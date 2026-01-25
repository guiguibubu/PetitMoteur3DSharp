using PetitMoteur3D.Graphics.Shaders;

namespace PetitMoteur3D;

internal interface IDrawableObjet
{
    RenderPassType[] SupportedRenderPasses { get; }
    
    /// <summary>
    /// Dessine l'objet
    /// </summary>
    /// <param name="graphicPipeline"></param>
    /// <param name="matViewProj"></param>
    void Draw(RenderPassType renderPass, SceneViewContext scene);
}
