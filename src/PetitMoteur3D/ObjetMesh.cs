using System.Collections.Generic;
using System.Linq;
using PetitMoteur3D.Graphics;

namespace PetitMoteur3D;

internal sealed class ObjetMesh : BaseObjet3D
{
    private readonly SceneMesh _sceneMesh;

    private readonly SubObjet3D[] _subObjects;

    public ObjetMesh(SceneMesh sceneMesh, GraphicDeviceRessourceFactory graphicDeviceRessourceFactory)
        : base(graphicDeviceRessourceFactory)
    {
        _sceneMesh = sceneMesh;

        _subObjects = GetSubObjets(_sceneMesh);

        Initialisation();
    }

    /// <inheritdoc/>
    protected override Sommet[] InitVertex()
    {
        return _sceneMesh.GetAllVertices().ToArray();
    }

    /// <inheritdoc/>
    protected override ushort[] InitIndex()
    {
        return _sceneMesh.GetAllIndices().ToArray();
    }

    /// <inheritdoc/>
    protected override SubObjet3D[] InitSubObjets()
    {
        return _subObjects;
    }

    private static SubObjet3D[] GetSubObjets(SceneMesh sceneMesh)
    {
        List<SubObjet3D> subObjects = new();
        subObjects.Add(ToSubObjet3D(sceneMesh, System.Numerics.Matrix4x4.Identity));
        foreach (SceneMesh child in sceneMesh.Children)
        {
            subObjects.AddRange(GetSubObjets(child));
        }
        return subObjects.ToArray();
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