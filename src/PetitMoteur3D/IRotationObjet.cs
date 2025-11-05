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
    ref readonly System.Numerics.Vector3 RotateEuler(ref readonly System.Numerics.Vector3 rotation);

    /// <summary>
    /// Rotate the object
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="angle">In radians</param>
    /// <returns>The new rotation</returns>
    ref readonly System.Numerics.Vector3 Rotate(ref readonly System.Numerics.Vector3 axis, float angle);
}
