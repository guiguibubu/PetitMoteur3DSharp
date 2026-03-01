using System;
using System.Numerics;
using PetitMoteur3D.Graphics;

namespace PetitMoteur3D;

internal class SubObjet3D
{
    public ushort[] Indices { get; init; }
    public Matrix4x4 Transformation { get; init; }
    public Material Material { get; init; }

    public SubObjet3D()
    {
        this.Material = new Material();
        this.Indices = Array.Empty<ushort>();
        this.Transformation = Matrix4x4.Identity;
    }
    public SubObjet3D(Material material)
    {
        ArgumentNullException.ThrowIfNull(material);
        this.Material = material;
    }
}
