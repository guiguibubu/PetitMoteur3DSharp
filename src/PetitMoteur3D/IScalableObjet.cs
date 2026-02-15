namespace PetitMoteur3D;

/// <summary>
/// Interface for a scalable object (scale)
/// </summary>
internal interface IScalableObjet
{
    /// <summary>
    /// Set scale of the object
    /// </summary>
    /// <param name="scale"></param>
    /// <returns>The new scale</returns>
    ref readonly System.Numerics.Vector3 SetScale(float scale);
    
    /// <summary>
    /// Set scale of the object
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns>The new scale</returns>
    ref readonly System.Numerics.Vector3 SetScale(float x, float y, float z);

    /// <summary>
    /// Set scale of the object
    /// </summary>
    /// <param name="scale"></param>
    /// <returns>The new scale</returns>
    ref readonly System.Numerics.Vector3 SetScale(System.Numerics.Vector3 scale);

    /// <summary>
    /// Set scale of the object
    /// </summary>
    /// <param name="scale"></param>
    /// <returns>The new scale</returns>
    ref readonly System.Numerics.Vector3 SetScale(scoped ref readonly System.Numerics.Vector3 scale);
}
