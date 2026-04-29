using System;
using PetitMoteur3D.Graphics;

namespace PetitMoteur3D;

internal sealed class ObjetMesh : BaseObjet3D
{
    private readonly Mesh _mesh;

    private static readonly System.Numerics.Vector3 AxisRotation = System.Numerics.Vector3.UnitY;

    public ObjetMesh(Mesh mesh, GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, string name = "")
        : base(mesh, graphicDeviceRessourceFactory, name)
    {
        _mesh = mesh;

        Initialisation();
    }

    /// <inheritdoc/>
    public override void Update(float elapsedTime)
    {
        base.Update(elapsedTime);
        Orientation.Rotate(in AxisRotation, (float)((Math.PI * 2.0f) / 24.0f * elapsedTime / 1000f));
        // modifier la matrice de l’objet bloc
        UpdateMatWorld();
    }
}