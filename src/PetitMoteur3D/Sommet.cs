using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D;

internal struct Sommet
{
    private Vector3 _position;
    private Vector3 _normale;
    private Vector3 _tangente;
    private Vector2 _coordTex;
    public readonly Vector3 Position => _position;
    public readonly Vector3 Normale => _normale;
    public readonly Vector3 Tangente => _tangente;
    public readonly Vector2 CoordTex => _coordTex;

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
    public unsafe Sommet(Vector3 position, Vector3 normale, Vector3 tangente, Vector2 coordTex)
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
    public unsafe Sommet(Vector3 position, Vector3 normale, Vector3 tangente)
    : this(position, normale, tangente, Vector2.Zero)
    { }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="position"></param>
    /// <param name="normale"></param>
    /// <param name="coordTex"></param>
    public unsafe Sommet(Vector3 position, Vector3 normale)
    : this(position, normale, Vector3.Zero, Vector2.Zero)
    { }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="position"></param>
    /// <param name="normale"></param>
    /// <param name="coordTex"></param>
    public unsafe Sommet()
    : this(Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector2.Zero)
    { }

    public Sommet Clone()
    {
        Sommet copy = (Sommet)this.MemberwiseClone();
        copy._position = new Vector3(Position.X, Position.Y, Position.Z);
        copy._normale = new Vector3(Normale.X, Normale.Y, Normale.Z);
        copy._coordTex = new Vector2(CoordTex.X, CoordTex.Y);
        return copy;
    }
}
