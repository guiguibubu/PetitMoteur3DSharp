using Silk.NET.Maths;

namespace PetitMoteur3D;

/// <summary>
/// Interface for a object which rotate
/// </summary>
internal interface IRotationObjet
{
    /// <summary>
    /// Rotate the object
    /// </summary>
    /// <param name="rotation"></param>
    /// <returns>The new rotation</returns>
    ref readonly Vector3D<float> Rotate(ref readonly Vector3D<float> rotation);
}
