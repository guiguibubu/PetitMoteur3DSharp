using System.Numerics;

namespace PetitMoteur3D.Camera;

internal interface ICamera : ISceneObjet, IMovableObjet, IInputListener
{
    /// <summary>
    /// Champ vision
    /// </summary>
    float ChampVision { get; }

    /// <summary>
    /// FrustrumView
    /// </summary>
    FrustrumView FrustrumView { get; }

    /// <summary>
    /// Get the current view matrix
    /// </summary>
    /// <returns></returns>
    void GetViewMatrix(out Matrix4x4 viewMatrix);
}