using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace PetitMoteur3D;

internal interface IObjet3D : ISceneObjet, IMovableObjet, IRotationObjet
{
    /// <summary>
    /// Anime l'objet
    /// </summary>
    /// <param name="elapsedTime"></param>
    void Anime(float elapsedTime);
    /// <summary>
    /// Dessine l'objet
    /// </summary>
    /// <param name="deviceContext"></param>
    /// <param name="matViewProj"></param>
    void Draw(ref readonly ComPtr<ID3D11DeviceContext> deviceContext, ref readonly Matrix4X4<float> matViewProj);
}
