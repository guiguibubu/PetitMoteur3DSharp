using Silk.NET.Maths;

namespace PetitMoteur3D
{
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
        ref readonly Vector3D<float> Move(ref readonly Vector3D<float> move);
    }
}
