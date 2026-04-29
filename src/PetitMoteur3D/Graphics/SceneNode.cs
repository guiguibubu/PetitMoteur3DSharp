using System;
using System.Collections.Generic;
using System.Linq;

namespace PetitMoteur3D.Graphics;

internal sealed class SceneNode<TMesh> : IVisitable, IVisitable<SceneNode<TMesh>> where TMesh : class, IMesh, IVisitable, IVisitable<TMesh>
{
    public IReadOnlyList<TMesh> Meshes { get { return _meshes; } }
    public System.Numerics.Matrix4x4 Transformation { get { return _localTransformation; } }
    public IReadOnlyList<SceneNode<TMesh>> Children { get { return _children; } }

    private readonly List<TMesh> _meshes;
    private System.Numerics.Matrix4x4 _localTransformation;
    private readonly List<SceneNode<TMesh>> _children;

    public SceneNode(TMesh[] meshes, System.Numerics.Matrix4x4 localTransformation, SceneNode<TMesh>[] children)
    {
        _meshes = new List<TMesh>(meshes);
        _localTransformation = localTransformation;
        _children = new List<SceneNode<TMesh>>(children);
    }

    public SceneNode(TMesh[] meshes, System.Numerics.Matrix4x4 transformation)
    : this(meshes, transformation, Array.Empty<SceneNode<TMesh>>())
    {
    }

    public SceneNode(TMesh[] meshes)
    : this(meshes, System.Numerics.Matrix4x4.Identity)
    {
    }

    public SceneNode(System.Numerics.Matrix4x4 transformation)
    : this(Array.Empty<TMesh>(), transformation)
    {
    }

    public SceneNode()
    : this(Array.Empty<TMesh>(), System.Numerics.Matrix4x4.Identity)
    {
    }

    public void AddMesh(TMesh mesh)
    {
        _meshes.Add(mesh);
    }

    public void AddChildren(IEnumerable<TMesh> mesh)
    {
        _meshes.AddRange(mesh);
    }

    public void AddChild(SceneNode<TMesh> node)
    {
        _children.Add(node);
    }

    public void AddChildren(IEnumerable<SceneNode<TMesh>> nodes)
    {
        _children.AddRange(nodes);
    }

    public Sommet[] GetAllVertices()
    {
        List<Sommet> vertices = new();
        foreach (TMesh mesh in _meshes)
        {
            vertices.AddRange(mesh.Sommets);
        }
        foreach (SceneNode<TMesh> child in _children)
        {
            vertices.AddRange(child.GetAllVertices());
        }
        return vertices.ToArray();
    }

    public ushort[] GetAllIndices()
    {
        List<ushort> indices = new();
        foreach (TMesh mesh in _meshes)
        {
            indices.AddRange(mesh.Indices);
        }
        foreach (SceneNode<TMesh> child in _children)
        {
            indices.AddRange(child.GetAllIndices());
        }
        return indices.ToArray();
    }

    public void AddTransform(System.Numerics.Matrix4x4 transformation)
    {
        _localTransformation = _localTransformation * transformation;
    }

    public BoundingBox GetBoundingBox()
    {
        return GetBoundingBox(System.Numerics.Matrix4x4.Identity);
    }

    public BoundingBox GetBoundingBox(System.Numerics.Matrix4x4 transformation)
    {
        BoundingBox boundingBox = new();

        IReadOnlyList<Sommet> sommets = GetAllVertices();
        System.Numerics.Matrix4x4 fullTransformation = transformation * _localTransformation;

        System.Numerics.Vector4[] positions = sommets.Select(v => System.Numerics.Vector4.Transform(new System.Numerics.Vector4(v.Position, 1f), fullTransformation)).ToArray();
        float[] sommetsX = positions.Select(v => v.X).Order().ToArray();
        float[] sommetsY = positions.Select(v => v.Y).Order().ToArray();
        float[] sommetsZ = positions.Select(v => v.Z).Order().ToArray();
        boundingBox.Min.X = sommetsX[0];
        boundingBox.Min.Y = sommetsY[0];
        boundingBox.Min.Z = sommetsZ[0];

        boundingBox.Max.X = sommetsX[sommetsX.Length - 1];
        boundingBox.Max.Y = sommetsY[sommetsY.Length - 1];
        boundingBox.Max.Z = sommetsZ[sommetsZ.Length - 1];

        foreach (SceneNode<TMesh> child in Children)
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

    public T[] GetObject<T>() where T : class
    {
        List<T> objects = _meshes.Select(x => x as T).Where(x => x is not null).Select(x => x!).ToList();
        foreach (SceneNode<TMesh> node in _children)
        {
            objects.AddRange(GetObject<T>());
        }
        return objects.ToArray();
    }

    public SceneNode<T> Select<T>(Func<TMesh, T> selector) where T : class, IMesh, IVisitable, IVisitable<T>
    {
        SceneNode<T> rootNode = new(_meshes.Select(selector).ToArray(), _localTransformation, _children.Select(n => n.Select(selector)).ToArray());
        return rootNode;
    }

    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
        foreach (TMesh mesh in _meshes)
        {
            mesh.Accept(visitor);
        }

        foreach (SceneNode<TMesh> child in Children)
        {
            child.Accept(visitor);
        }
    }

    public void Accept(IVisitor<SceneNode<TMesh>> visitor)
    {
        visitor.Visit(this);
        if (visitor is IVisitor<TMesh> visitorMesh)
        {
            foreach (TMesh mesh in _meshes)
            {
                mesh.Accept(visitorMesh);
            }
        }

        foreach (SceneNode<TMesh> child in Children)
        {
            child.Accept(visitor);
        }
    }
}