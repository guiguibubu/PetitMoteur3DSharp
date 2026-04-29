using PetitMoteur3D.Graphics;

namespace PetitMoteur3D;

internal interface IObjet3D : INamedObjet, ISceneObjet<IObjet3D>, IMovableObjet, IRotationObjet, IScalableObjet, IMesh
{
    Mesh Mesh { get; }
}
