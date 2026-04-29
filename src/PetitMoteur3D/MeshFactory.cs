using System;
using System.Numerics;
using PetitMoteur3D.Graphics;

namespace PetitMoteur3D;

internal sealed class MeshFactory
{
    public MeshFactory()
    {
    }

    public Mesh CreateBloc(float dx, float dy, float dz)
    {
        Vector3[] vertices = new Vector3[]
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

        Vector3[] normales = new Vector3[]
        {
            new(0f, 0f, -1f), // devant
            new(0f, 0f, 1f), // arrière
            new(0f, -1f, 0f), // dessous
            new(0f, 1f, 0f), // dessus
            new(-1f, 0f, 0f), // face gauche
            new(1f, 0f, 0f) // face droite
        };

        Vector3[] tangentes = new Vector3[]
        {
            new(1f, 0f, 0f), // devant
            new(-1f, 0f, 0f), // arrière
            new(1f, 0f, 0f), // dessous
            new(1f, 0f, 0f), // dessus
            new(0f, -1f, 0f), // face gauche
            new(0f, 1f, 0f) // face droite
        };

        Sommet[] sommets = new Sommet[]
        {
            // Le devant du bloc
            new(vertices[0], normales[0], tangentes[0], new Vector2(0f, 0f)),
            new(vertices[1], normales[0], tangentes[0], new Vector2(1f, 0f)),
            new(vertices[2], normales[0], tangentes[0], new Vector2(1f, 1f)),
            new(vertices[3], normales[0], tangentes[0], new Vector2(0f, 1f)),
            // L’arrière du bloc
            new(vertices[4], normales[1], tangentes[1], new Vector2(1f, 0f)),
            new(vertices[5], normales[1], tangentes[1], new Vector2(1f, 1f)),
            new(vertices[6], normales[1], tangentes[1], new Vector2(0f, 1f)),
            new(vertices[7], normales[1], tangentes[1], new Vector2(0f, 0f)),
            // Le dessous du bloc
            new(vertices[3], normales[2], tangentes[2], new Vector2(0f, 0f)),
            new(vertices[2], normales[2], tangentes[2], new Vector2(1f, 0f)),
            new(vertices[6], normales[2], tangentes[2], new Vector2(1f, 1f)),
            new(vertices[5], normales[2], tangentes[2], new Vector2(0f, 1f)),
            // Le dessus du bloc
            new(vertices[0], normales[3], tangentes[3], new Vector2(0f, 1f)),
            new(vertices[4], normales[3], tangentes[3], new Vector2(0f, 0f)),
            new(vertices[7], normales[3], tangentes[3], new Vector2(1f, 0f)),
            new(vertices[1], normales[3], tangentes[3], new Vector2(1f, 1f)),
            // La face gauche
            new(vertices[0], normales[4], tangentes[4], new Vector2(1f, 0f)),
            new(vertices[3], normales[4], tangentes[4], new Vector2(1f, 1f)),
            new(vertices[5], normales[4], tangentes[4], new Vector2(0f, 1f)),
            new(vertices[4], normales[4], tangentes[4], new Vector2(0f, 0f)),
            // La face droite
            new(vertices[1], normales[5], tangentes[5], new Vector2(0f, 0f)),
            new(vertices[7], normales[5], tangentes[5], new Vector2(1f, 0f)),
            new(vertices[6], normales[5], tangentes[5], new Vector2(1f, 1f)),
            new(vertices[2], normales[5], tangentes[5], new Vector2(0f, 1f))
        };

        ushort[] indices = new ushort[]
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

        return new Mesh(sommets, indices, new Material());
    }

    public Mesh CreatePlane(float dx, float dy)
    {
        Vector3[] vertices = new Vector3[]
        {
            new(-dx / 2, dy / 2, 0),
            new(dx / 2, dy / 2, 0),
            new(dx / 2, -dy / 2, 0),
            new(-dx / 2, -dy / 2, 0),
        };

        Vector3[] normales = new Vector3[]
        {
            new(0f, 0f, -1f), // devant
        };

        Vector3[] tangentes = new Vector3[]
        {
            new(1f, 0f, 0f), // devant
        };

        float minDim = Math.Min(dx, dy);
        Sommet[] sommets = new Sommet[]
        {
            // Le devant du bloc
            new(vertices[0], normales[0], tangentes[0], new Vector2(0f, 0f)),
            new(vertices[1], normales[0], tangentes[0], new Vector2(minDim, 0f)),
            new(vertices[2], normales[0], tangentes[0], new Vector2(minDim, minDim)),
            new(vertices[3], normales[0], tangentes[0], new Vector2(0f, minDim)),
        };

        ushort[] indices = new ushort[]
        {
            0,1,2, // devant
            0,2,3, // devant
        };

        return new Mesh(sommets, indices, new Material());
    }

    public Mesh CreateQuad(float left, float right, float bottom, float top, float z)
    {
        Vector3[] vertices = new Vector3[]
        {
            new(left, top, z),
            new(right, top, z),
            new(right, bottom, z),
            new(left, bottom, z),
        };

        Vector3[] normales = new Vector3[]
        {
            new(0f, 0f, -1f), // devant
        };

        Vector3[] tangentes = new Vector3[]
        {
            new(1f, 0f, 0f), // devant
        };

        Sommet[] sommets = new Sommet[]
        {
            // Le devant du bloc
            new(vertices[0], normales[0], tangentes[0], new Vector2(0f, 0f)),
            new(vertices[1], normales[0], tangentes[0], new Vector2(1, 0f)),
            new(vertices[2], normales[0], tangentes[0], new Vector2(1, 1)),
            new(vertices[3], normales[0], tangentes[0], new Vector2(0f, 1)),
        };

        ushort[] indices = new ushort[]
        {
            0,1,2, // devant
            0,2,3, // devant
        };

        return new Mesh(sommets, indices, new Material());
    }
}
