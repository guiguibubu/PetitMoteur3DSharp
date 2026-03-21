using System;
using System.Numerics;
using System.Runtime.InteropServices;
using PetitMoteur3D.Graphics.Shaders;

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
    public static InputLayoutDesc InputLayoutDesc => _inputLayoutDesc;

    private static InputLayoutDesc _inputLayoutDesc;

    static unsafe Sommet()
    {
        InputLayoutDescBuilder builder = new();

        builder.Add(D3D11Semantics.Position, Silk.NET.DXGI.Format.FormatR32G32B32Float, inputSlot: 0, alignedByteOffset: Convert.ToUInt32(Marshal.OffsetOf<Sommet>(nameof(_position))));
        builder.Add(D3D11Semantics.Normal, Silk.NET.DXGI.Format.FormatR32G32B32Float, inputSlot: 0, alignedByteOffset: Convert.ToUInt32(Marshal.OffsetOf<Sommet>(nameof(_normale))));
        builder.Add(D3D11Semantics.Tangent, Silk.NET.DXGI.Format.FormatR32G32Float, inputSlot: 0, alignedByteOffset: Convert.ToUInt32(Marshal.OffsetOf<Sommet>(nameof(_tangente))));
        builder.Add(D3D11Semantics.TexCoord, Silk.NET.DXGI.Format.FormatR32G32Float, inputSlot: 0, alignedByteOffset: Convert.ToUInt32(Marshal.OffsetOf<Sommet>(nameof(_coordTex))));

        _inputLayoutDesc = builder.Build();
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
