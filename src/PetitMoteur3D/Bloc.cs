using System;
using System.Collections.Generic;
using PetitMoteur3D.Graphics;
using Silk.NET.Maths;

namespace PetitMoteur3D;

internal sealed class Bloc : BaseObjet3D
{
    private readonly Vector3D<float>[] _vertices;
    private readonly Vector3D<float>[] _normales;
    private readonly Vector3D<float>[] _tangentes;
    private readonly Sommet[] _sommets;
    private readonly ushort[] _indices;
    private readonly SubObjet3D[] _subObjects;

    private System.Numerics.Matrix4x4 _transformation;

    public unsafe Bloc(float dx, float dy, float dz, GraphicDeviceRessourceFactory graphicDeviceRessourceFactory)
        : base(graphicDeviceRessourceFactory)
    {
        _vertices = new Vector3D<float>[]
        {
            new(-dx / 2, dy / 2, -dz / 2),
            new(dx / 2, dy / 2, -dz / 2),
            new(dx / 2, -dy / 2, -dz / 2),
            new(-dx / 2, -dy / 2, -dz / 2),
            new(-dx / 2, dy / 2, dz / 2),
            new(-dx / 2, -dy / 2, dz / 2),
            new(dx / 2, -dy / 2, dz / 2),
            new(dx / 2, dy / 2, dz / 2)
        };

        _normales = new Vector3D<float>[]
        {
            new(0f, 0f, -1f), // devant
            new(0f, 0f, 1f), // arrière
            new(0f, -1f, 0f), // dessous
            new(0f, 1f, 0f), // dessus
            new(-1f, 0f, 0f), // face gauche
            new(1f, 0f, 0f) // face droite
        };

        _tangentes = new Vector3D<float>[]
        {
            new(1f, 0f, 0f), // devant
            new(-1f, 0f, 0f), // arrière
            new(1f, 0f, 0f), // dessous
            new(1f, 0f, 0f), // dessus
            new(0f, -1f, 0f), // face gauche
            new(0f, 1f, 0f) // face droite
        };

        _sommets = new Sommet[]
        {
            // Le devant du bloc
            new(_vertices[0], _normales[0], _tangentes[0], new Vector2D<float>(0f, 0f)),
            new(_vertices[1], _normales[0], _tangentes[0], new Vector2D<float>(1f, 0f)),
            new(_vertices[2], _normales[0], _tangentes[0], new Vector2D<float>(1f, 1f)),
            new(_vertices[3], _normales[0], _tangentes[0], new Vector2D<float>(0f, 1f)),
            // L’arrière du bloc
            new(_vertices[4], _normales[1], _tangentes[1], new Vector2D<float>(1f, 0f)),
            new(_vertices[5], _normales[1], _tangentes[1], new Vector2D<float>(1f, 1f)),
            new(_vertices[6], _normales[1], _tangentes[1], new Vector2D<float>(0f, 1f)),
            new(_vertices[7], _normales[1], _tangentes[1], new Vector2D<float>(0f, 0f)),
            // Le dessous du bloc
            new(_vertices[3], _normales[2], _tangentes[2], new Vector2D<float>(0f, 0f)),
            new(_vertices[2], _normales[2], _tangentes[2], new Vector2D<float>(1f, 0f)),
            new(_vertices[6], _normales[2], _tangentes[2], new Vector2D<float>(1f, 1f)),
            new(_vertices[5], _normales[2], _tangentes[2], new Vector2D<float>(0f, 1f)),
            // Le dessus du bloc
            new(_vertices[0], _normales[3], _tangentes[3], new Vector2D<float>(0f, 1f)),
            new(_vertices[4], _normales[3], _tangentes[3], new Vector2D<float>(0f, 0f)),
            new(_vertices[7], _normales[3], _tangentes[3], new Vector2D<float>(1f, 0f)),
            new(_vertices[1], _normales[3], _tangentes[3], new Vector2D<float>(1f, 1f)),
            // La face gauche
            new(_vertices[0], _normales[4], _tangentes[4], new Vector2D<float>(1f, 0f)),
            new(_vertices[3], _normales[4], _tangentes[4], new Vector2D<float>(1f, 1f)),
            new(_vertices[5], _normales[4], _tangentes[4], new Vector2D<float>(0f, 1f)),
            new(_vertices[4], _normales[4], _tangentes[4], new Vector2D<float>(0f, 0f)),
            // La face droite
            new(_vertices[1], _normales[5], _tangentes[5], new Vector2D<float>(0f, 0f)),
            new(_vertices[7], _normales[5], _tangentes[5], new Vector2D<float>(1f, 0f)),
            new(_vertices[6], _normales[5], _tangentes[5], new Vector2D<float>(1f, 1f)),
            new(_vertices[2], _normales[5], _tangentes[5], new Vector2D<float>(0f, 1f))
        };

        _indices = new ushort[]
        {
            0,1,2, // devant
            0,2,3, // devant
            5,6,7, // arrière
            5,7,4, // arrière
            8,9,10, // dessous
            8,10,11, // dessous
            13,14,15, // dessus
            13,15,12, // dessus
            19,16,17, // gauche
            19,17,18, // gauche
            20,21,22, // droite
            20,22,23 // droite
        };


        _transformation = System.Numerics.Matrix4x4.Identity;

        _subObjects = new SubObjet3D[] {new ()
            {
                Indices = _indices,
                Material = new Material(),
                Transformation = _transformation
            }
        };

        Initialisation();
    }

    public void AddTransform(System.Numerics.Matrix4x4 transformation)
    {
        _transformation = transformation * _transformation;
    }

    /// <inheritdoc/>
    protected override Sommet[] InitVertex()
    {
        return _sommets;
    }

    /// <inheritdoc/>
    protected override ushort[] InitIndex()
    {
        return _indices;
    }

    protected override SubObjet3D[] InitSubObjets()
    {
        return _subObjects;
    }

    private static readonly System.Numerics.Vector3 AxisRotation = System.Numerics.Vector3.UnitY;

    /// <inheritdoc/>
    public override void Update(float elapsedTime)
    {
        base.Update(elapsedTime);
        Orientation.Rotate(in AxisRotation, (float)((Math.PI * 2.0f) / 24.0f * elapsedTime / 1000f));
        // modifier la matrice de l’objet bloc
        UpdateMatWorld();
    }
}
