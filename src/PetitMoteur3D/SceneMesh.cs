using System;
using System.Collections.Generic;
using System.Linq;
using Silk.NET.Maths;

namespace PetitMoteur3D;

internal class SceneMesh
{
    public Mesh Mesh { get { return _mesh; } }
    public Matrix4X4<float> Transformation { get { return _transformation; } }
    public IReadOnlyList<SceneMesh> Children { get { return _children; } }

    private readonly Mesh _mesh;
    private Matrix4X4<float> _transformation;
    private readonly List<SceneMesh> _children;

    public SceneMesh(Mesh mesh, Matrix4X4<float> transformation, IReadOnlyList<SceneMesh> children)
    {
        _mesh = mesh;
        _transformation = transformation;
        _children = new List<SceneMesh>(children);
    }

    public SceneMesh(Mesh mesh, Matrix4X4<float> transformation)
    : this(mesh, transformation, Array.Empty<SceneMesh>())
    {
    }

    public SceneMesh(Mesh mesh)
    : this(mesh, Matrix4X4<float>.Identity)
    {
    }

    public void AddChild(SceneMesh submesh)
    {
        _children.Add(submesh);
    }

    public void AddChildren(IEnumerable<SceneMesh> submeshes)
    {
        _children.AddRange(submeshes);
    }

    public IReadOnlyList<Sommet> GetAllVertices()
    {
        List<Sommet> vertices = new(Mesh.Sommets);
        foreach (SceneMesh child in _children)
        {
            vertices.AddRange(child.GetAllVertices());
        }
        return vertices;
    }

    public IReadOnlyList<ushort> GetAllIndices()
    {
        List<ushort> indices = new(Mesh.Indices);
        foreach (SceneMesh child in _children)
        {
            indices.AddRange(child.GetAllIndices());
        }
        return indices;
    }

    public BoundingBox GetBoundingBox()
    {
        return GetBoundingBox(Matrix4X4<float>.Identity);
    }

    public BoundingBox GetBoundingBox(Matrix4X4<float> transformation)
    {
        BoundingBox boundingBox = new();

        IReadOnlyList<Sommet> sommets = Mesh.Sommets;
        Matrix4X4<float> fullTransformation = transformation * _transformation;

        IReadOnlyList<Vector4D<float>> positions = sommets.Select(v => new Vector4D<float>(v.Position, 1f) * fullTransformation).ToList();
        IReadOnlyList<float> sommetsX = positions.Select(v => v.X).Order().ToList();
        IReadOnlyList<float> sommetsY = positions.Select(v => v.Y).Order().ToList();
        IReadOnlyList<float> sommetsZ = positions.Select(v => v.Z).Order().ToList();
        boundingBox.Min.X = sommetsX[0];
        boundingBox.Min.Y = sommetsY[0];
        boundingBox.Min.Z = sommetsZ[0];

        boundingBox.Max.X = sommetsX[sommetsX.Count - 1];
        boundingBox.Max.Y = sommetsY[sommetsY.Count - 1];
        boundingBox.Max.Z = sommetsZ[sommetsZ.Count - 1];

        foreach (SceneMesh child in Children)
        {
            BoundingBox childBoundingBox = child.GetBoundingBox(fullTransformation);
            boundingBox.Min.X = float.Min(boundingBox.Min.X, childBoundingBox.Min.X);
            boundingBox.Min.Y = float.Min(boundingBox.Min.Y, childBoundingBox.Min.Y);
            boundingBox.Min.Z = float.Min(boundingBox.Min.Z, childBoundingBox.Min.Z);

            boundingBox.Max.X = float.Min(boundingBox.Max.X, childBoundingBox.Max.X);
            boundingBox.Max.Y = float.Min(boundingBox.Max.Y, childBoundingBox.Max.Y);
            boundingBox.Max.Z = float.Min(boundingBox.Max.Z, childBoundingBox.Max.Z);
        }
        return boundingBox;
    }

    public void AddTransform(Matrix4X4<float> transformation)
    {
        _transformation = transformation * _transformation;
    }
}