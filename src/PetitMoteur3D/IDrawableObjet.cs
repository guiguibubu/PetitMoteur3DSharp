using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D;

internal interface IDrawableObjet
{
    /// <summary>
    /// Dessine l'objet
    /// </summary>
    /// <param name="deviceContext"></param>
    /// <param name="matViewProj"></param>
    void Draw(ref readonly ComPtr<ID3D11DeviceContext> deviceContext, ref readonly System.Numerics.Matrix4x4 matViewProj);
}
