using PetitMoteur3D.Graphics;

namespace PetitMoteur3D;

/// <summary>
/// Interface for scene object
/// </summary>
internal interface ISceneObjet<T> : IPositionObjet, IUpdatableObjet, IVisitable, IVisitable<T> where T : class
{
}
