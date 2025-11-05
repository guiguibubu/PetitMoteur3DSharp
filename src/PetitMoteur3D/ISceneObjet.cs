namespace PetitMoteur3D;

/// <summary>
/// Interface for scene object
/// </summary>
internal interface ISceneObjet : IUpdatableObjet
{
    /// <summary>
    /// Position
    /// </summary>
    ref readonly System.Numerics.Vector3 Position { get; }
}
