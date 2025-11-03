using System;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace PetitMoteur3D;

internal struct Sommet
{
    private Vector3D<float> _position;
    private Vector3D<float> _normale;
    private Vector3D<float> _tangente;
    private Vector2D<float> _coordTex;
    public readonly Vector3D<float> Position => _position;
    public readonly Vector3D<float> Normale => _normale;
    public readonly Vector3D<float> Tangente => _tangente;
    public readonly Vector2D<float> CoordTex => _coordTex;

    /// <summary>
    /// Defini l’organisation de notre sommet
    /// </summary>
    public static InputElementDesc[] InputLayoutDesc => s_inputElements;

    private static readonly GlobalMemory s_semanticNamePosition;
    private static readonly GlobalMemory s_semanticNameNormal;
    private static readonly GlobalMemory s_semanticNameTexCoord;
    private static readonly GlobalMemory s_semanticNameTangent;
    private static readonly InputElementDesc[] s_inputElements;
    static unsafe Sommet()
    {
        s_semanticNamePosition = SilkMarshal.StringToMemory("POSITION", NativeStringEncoding.Ansi);
        s_semanticNameNormal = SilkMarshal.StringToMemory("NORMAL", NativeStringEncoding.Ansi);
        s_semanticNameTangent = SilkMarshal.StringToMemory("TANGENT", NativeStringEncoding.Ansi);
        s_semanticNameTexCoord = SilkMarshal.StringToMemory("TEXCOORD", NativeStringEncoding.Ansi);

        s_inputElements = new[]
        {
            new InputElementDesc(
                s_semanticNamePosition.AsPtr<byte>(), 0, Silk.NET.DXGI.Format.FormatR32G32B32Float, 0, Convert.ToUInt32(Marshal.OffsetOf<Sommet>(nameof(_position))), InputClassification.PerVertexData, 0
            ),
            new InputElementDesc(
                s_semanticNameNormal.AsPtr<byte>(), 0, Silk.NET.DXGI.Format.FormatR32G32B32Float, 0, Convert.ToUInt32(Marshal.OffsetOf<Sommet>(nameof(_normale))), InputClassification.PerVertexData, 0
            ),
            new InputElementDesc(
                s_semanticNameTangent.AsPtr<byte>(), 0, Silk.NET.DXGI.Format.FormatR32G32Float, 0, Convert.ToUInt32(Marshal.OffsetOf<Sommet>(nameof(_tangente))), InputClassification.PerVertexData, 0
            ),
            new InputElementDesc(
                s_semanticNameTexCoord.AsPtr<byte>(), 0, Silk.NET.DXGI.Format.FormatR32G32Float, 0, Convert.ToUInt32(Marshal.OffsetOf<Sommet>(nameof(_coordTex))), InputClassification.PerVertexData, 0
            ),
        };
    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="position"></param>
    /// <param name="normale"></param>
    /// <param name="coordTex"></param>
    public unsafe Sommet(Vector3D<float> position, Vector3D<float> normale, Vector3D<float> tangente, Vector2D<float> coordTex)
    {
        _position = position;
        _normale = normale;
        _tangente = tangente;
        _coordTex = coordTex;
    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="position"></param>
    /// <param name="normale"></param>
    /// <param name="coordTex"></param>
    public unsafe Sommet(Vector3D<float> position, Vector3D<float> normale, Vector3D<float> tangente)
    : this(position, normale, tangente, Vector2D<float>.Zero)
    { }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="position"></param>
    /// <param name="normale"></param>
    /// <param name="coordTex"></param>
    public unsafe Sommet(Vector3D<float> position, Vector3D<float> normale)
    : this(position, normale, Vector3D<float>.Zero, Vector2D<float>.Zero)
    { }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="position"></param>
    /// <param name="normale"></param>
    /// <param name="coordTex"></param>
    public unsafe Sommet()
    : this(Vector3D<float>.Zero, Vector3D<float>.Zero, Vector3D<float>.Zero, Vector2D<float>.Zero)
    { }

    public Sommet Clone()
    {
        Sommet copy = (Sommet)this.MemberwiseClone();
        copy._position = new Vector3D<float>(Position.X, Position.Y, Position.Z);
        copy._normale = new Vector3D<float>(Normale.X, Normale.Y, Normale.Z);
        copy._coordTex = new Vector2D<float>(CoordTex.X, CoordTex.Y);
        return copy;
    }
}
