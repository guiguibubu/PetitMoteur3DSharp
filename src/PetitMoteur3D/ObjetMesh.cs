using System.Collections.Generic;
using PetitMoteur3D.Graphics;

namespace PetitMoteur3D;

internal class ObjetMesh : BaseObjet3D
{
    public SceneMesh Mesh => _sceneMesh;
    private readonly SceneMesh _sceneMesh;

    private IReadOnlyList<SubObjet3D>? _subObjects = null;

    public ObjetMesh(SceneMesh sceneMesh, GraphicDeviceRessourceFactory graphicDeviceRessourceFactory)
        : base(graphicDeviceRessourceFactory)
    {
        _sceneMesh = sceneMesh;

        Initialisation();
    }

    /// <inheritdoc/>
    protected override IReadOnlyList<Sommet> InitVertex()
    {
        return _sceneMesh.GetAllVertices();
    }

    /// <inheritdoc/>
    protected override IReadOnlyList<ushort> InitIndex()
    {
        return _sceneMesh.GetAllIndices();
    }

    /// <inheritdoc/>
    protected override IReadOnlyList<SubObjet3D> GetSubObjets()
    {
        if (_subObjects is null)
        {
            _subObjects = GetSubObjets(_sceneMesh);
        }
        return _subObjects;
    }

    private static IReadOnlyList<SubObjet3D> GetSubObjets(SceneMesh sceneMesh)
    {
        List<SubObjet3D> subObjects = new();
        subObjects.Add(ToSubObjet3D(sceneMesh, System.Numerics.Matrix4x4.Identity));
        foreach (SceneMesh child in sceneMesh.Children)
        {
            subObjects.AddRange(GetSubObjets(child));
        }
        return subObjects;
    }

    private static SubObjet3D ToSubObjet3D(SceneMesh sceneMesh, System.Numerics.Matrix4x4 parentTransform)
    {
        return new SubObjet3D()
        {
            Indices = sceneMesh.Mesh.Indices,
            Material = sceneMesh.Mesh.Material,
            Transformation = parentTransform * sceneMesh.Transformation
        };
    }
}