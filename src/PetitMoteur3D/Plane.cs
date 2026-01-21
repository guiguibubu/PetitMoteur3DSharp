using System;
using System.Linq;
using System.Numerics;
using PetitMoteur3D.Graphics;
using PetitMoteur3D.Graphics.Shaders;

namespace PetitMoteur3D;

internal sealed class Plane : BaseObjet3D
{
    private readonly Vector3[] _vertices;
    private readonly Vector3[] _normales;
    private readonly Vector3[] _tangentes;
    private readonly Sommet[] _sommets;
    private readonly ushort[] _indices;
    private readonly SubObjet3D[] _subObjects;

    private System.Numerics.Matrix4x4 _transformation;

    public Plane(float dx, float dy, GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, RenderPassFactory shaderFactory)
        : base(graphicDeviceRessourceFactory, shaderFactory)
    {
        _vertices = new Vector3[]
        {
            new(-dx / 2, dy / 2, 0),
            new(dx / 2, dy / 2, 0),
            new(dx / 2, -dy / 2, 0),
            new(-dx / 2, -dy / 2, 0),
        };

        _normales = new Vector3[]
        {
            new(0f, 0f, -1f), // devant
        };

        _tangentes = new Vector3[]
        {
            new(1f, 0f, 0f), // devant
        };

        float minDim = Math.Min(dx, dy);
        _sommets = new Sommet[]
        {
            // Le devant du bloc
            new(_vertices[0], _normales[0], _tangentes[0], new Vector2(0f, 0f)),
            new(_vertices[1], _normales[0], _tangentes[0], new Vector2(minDim, 0f)),
            new(_vertices[2], _normales[0], _tangentes[0], new Vector2(minDim, minDim)),
            new(_vertices[3], _normales[0], _tangentes[0], new Vector2(0f, minDim)),
        };

        _indices = new ushort[]
        {
            0,1,2, // devant
            0,2,3, // devant
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

    ///// <inheritdoc/>
    //protected override SommetShadowMap[] InitVertexShadowMap()
    //{
    //    return _sommets.Select(s => new SommetShadowMap(s.Position)).ToArray();
    //}

    /// <inheritdoc/>
    protected override ushort[] InitIndex()
    {
        return _indices;
    }

    protected override SubObjet3D[] InitSubObjets()
    {
        return _subObjects;
    }
}
