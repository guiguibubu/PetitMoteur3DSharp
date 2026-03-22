namespace PetitMoteur3D;

internal interface IObjet3D : INamedObjet, ISceneObjet, IMovableObjet, IRotationObjet, IScalableObjet
{
    SubObjet3D[] SubObjects { get; }
}
