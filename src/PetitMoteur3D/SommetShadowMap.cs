using System;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace PetitMoteur3D;

internal struct SommetShadowMap
{
    private Vector3D<float> _position;
    public readonly Vector3D<float> Position => _position;

    /// <summary>
    /// Defini l’organisation de notre sommet
    /// </summary>
    public static InputElementDesc[] InputLayoutDesc => s_inputElements;

    private static readonly GlobalMemory s_semanticNamePosition;
    private static readonly InputElementDesc[] s_inputElements;
    static unsafe SommetShadowMap()
    {
        s_semanticNamePosition = SilkMarshal.StringToMemory("POSITION", NativeStringEncoding.Ansi);

        s_inputElements = new[]
        {
            new InputElementDesc(
                s_semanticNamePosition.AsPtr<byte>(), 0, Silk.NET.DXGI.Format.FormatR32G32B32Float, 0, Convert.ToUInt32(Marshal.OffsetOf<Sommet>(nameof(_position))), InputClassification.PerVertexData, 0
            )
        };
    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="position"></param>
    public SommetShadowMap(Vector3D<float> position)
    {
        _position = position;
    }

    /// <summary>
    /// Constructeur par défaut
    /// </summary>
    public unsafe SommetShadowMap()
    : this(Vector3D<float>.Zero)
    { }

    /// <summary>
    /// Constructeur de pie
    /// </summary>
    public unsafe SommetShadowMap(SommetShadowMap other)
    : this(other._position)
    { }

    public SommetShadowMap Clone()
    {
        return new SommetShadowMap(this);
    }
}
