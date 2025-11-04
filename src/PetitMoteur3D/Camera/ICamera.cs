using Silk.NET.Maths;

namespace PetitMoteur3D.Camera;

internal interface ICamera : ISceneObjet, IMovableObjet, IInputListener
{
    /// <summary>
    /// Champ vision
    /// </summary>
    float ChampVision { get; }

    /// <summary>
    /// Get the current view matrix
    /// </summary>
    /// <returns></returns>
    void GetViewMatrix(out Matrix4X4<float> viewMatrix);
}