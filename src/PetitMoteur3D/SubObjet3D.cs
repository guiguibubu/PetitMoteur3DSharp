using System;
using System.Numerics;
using PetitMoteur3D.Graphics;

namespace PetitMoteur3D;

internal sealed class SubObjet3D : IVisitable
{
    public ushort[] Indices { get; init; }
    public Matrix4x4 Transformation { get; init; }
    public Material Material { get; init; }

    public SubObjet3D()
        : this(new Material(), Array.Empty<ushort>(), Matrix4x4.Identity)
    {
    }

    public SubObjet3D(Material material, ushort[] indices, Matrix4x4 transformation)
    {
        ArgumentNullException.ThrowIfNull(material);
        this.Material = material;
        this.Indices = indices;
        this.Transformation = transformation;
    }

    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}
