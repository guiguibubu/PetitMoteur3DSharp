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
    ref readonly System.Numerics.Vector3 Move(System.Numerics.Vector3 move);

    /// <summary>
    /// Move the object
    /// </summary>
    /// <param name="move"></param>
    /// <returns>The new position</returns>
    ref readonly System.Numerics.Vector3 Move(scoped ref readonly System.Numerics.Vector3 move);

    /// <summary>
    /// Set position of the object
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns>The new position</returns>
    ref readonly System.Numerics.Vector3 SetPosition(float x, float y, float z);

    /// <summary>
    /// Set position of the object
    /// </summary>
    /// <param name="position"></param>
    /// <returns>The new position</returns>
    ref readonly System.Numerics.Vector3 SetPosition(System.Numerics.Vector3 position);

    /// <summary>
    /// Set position of the object
    /// </summary>
    /// <param name="position"></param>
    /// <returns>The new position</returns>
    ref readonly System.Numerics.Vector3 SetPosition(scoped ref readonly System.Numerics.Vector3 position);
}
