using Silk.NET.Maths;

namespace PetitMoteur3D
{
    /// <summary>
    /// Interface for scene object
    /// </summary>
    internal interface ISceneObjet
    {
        /// <summary>
        /// Position
        /// </summary>
        Vector3D<float> Position { get; }
    }
}
