using System;
using System.Numerics;
using PetitMoteur3D.Graphics;
using PetitMoteur3D.Graphics.Shaders;

namespace PetitMoteur3D;

/// <summary>
/// Create a screen-space quad that can be used to render full-screen post-process effects to the screen.
/// By default, the quad will have clip-space coordinates and can be used with a pass-through vertex shader
/// to render full-screen post-process effects. If you want more control over the area of the screen the quad covers, 
/// you can specify your own screen coordinates and supply an appropriate orthographic projection matrix to align the 
/// screen quad appropriately.
/// </summary>
internal sealed class ScreenQuad : BaseObjet3D
{
    public Material Material => _subObjects[0].Material;

    private readonly Vector3[] _vertices;
    private readonly Vector3[] _normales;
    private readonly Vector3[] _tangentes;
    private readonly Sommet[] _sommets;
    private readonly ushort[] _indices;
    private readonly SubObjet3D[] _subObjects;

    protected override bool SupportShadow => false;

    public ScreenQuad(float left, float right, float bottom, float top, float z,  GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, RenderPassFactory shaderFactory)
        : base(graphicDeviceRessourceFactory, shaderFactory)
    {
        _vertices = new Vector3[]
        {
            new(left, top, z),
            new(right, top, z),
            new(right, bottom, z),
            new(left, bottom, z),
        };

        _normales = new Vector3[]
        {
            new(0f, 0f, -1f), // devant
        };

        _tangentes = new Vector3[]
        {
            new(1f, 0f, 0f), // devant
        };

        _sommets = new Sommet[]
        {
            // Le devant du bloc
            new(_vertices[0], _normales[0], _tangentes[0], new Vector2(0f, 0f)),
            new(_vertices[1], _normales[0], _tangentes[0], new Vector2(1, 0f)),
            new(_vertices[2], _normales[0], _tangentes[0], new Vector2(1, 1)),
            new(_vertices[3], _normales[0], _tangentes[0], new Vector2(0f, 1)),
        };

        _indices = new ushort[]
        {
            0,1,2, // devant
            0,2,3, // devant
        };

        _subObjects = new SubObjet3D[] {new ()
            {
                Indices = _indices,
                Material = new Material(),
                Transformation = System.Numerics.Matrix4x4.Identity
            }
        };

        Initialisation();
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
}
