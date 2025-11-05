namespace PetitMoteur3D;

/// <summary>
/// Interface for a moveable object (move)
/// </summary>
internal interface IMovableObjet
{
    /// <summary>
    /// Move the object
    /// </summary>
    /// <param name="move"></param>
    /// <returns>The new position</returns>
    ref readonly System.Numerics.Vector3 Move(float dx, float dy, float dz);

    /// <summary>
    /// Move the object
    /// </summary>
    /// <param name="move"></param>
    /// <returns>The new position</returns>
    ref readonly System.Numerics.Vector3 Move(scoped ref readonly System.Numerics.Vector3 move);
}
