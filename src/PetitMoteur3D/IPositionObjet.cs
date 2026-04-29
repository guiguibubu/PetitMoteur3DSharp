namespace PetitMoteur3D;

/// <summary>
/// Interface for object with position
/// </summary>
internal interface IPositionObjet
{
    /// <summary>
    /// Position
    /// </summary>
    ref readonly System.Numerics.Vector3 Position { get; }
}
