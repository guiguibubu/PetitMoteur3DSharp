using PetitMoteur3D.Graphics;

namespace PetitMoteur3D;

internal interface IDrawableObjet
{
    /// <summary>
    /// Dessine l'objet
    /// </summary>
    /// <param name="graphicPipeline"></param>
    /// <param name="matViewProj"></param>
    void Draw(D3D11GraphicPipeline graphicPipeline, ref readonly System.Numerics.Matrix4x4 matViewProj);
}
