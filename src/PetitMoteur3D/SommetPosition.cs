using System;
using System.Numerics;
using System.Runtime.InteropServices;
using PetitMoteur3D.Graphics.Shaders;

namespace PetitMoteur3D;

internal struct SommetPosition
{
    private Vector3 _position;
    public readonly Vector3 Position => _position;

    /// <summary>
    /// Defini l’organisation de notre sommet
    /// </summary>
    public static InputLayoutDesc InputLayoutDesc => _inputLayoutDesc;

    private static InputLayoutDesc _inputLayoutDesc;

    static unsafe SommetPosition()
    {
        InputLayoutDescBuilder builder = new();

        builder.Add(D3D11Semantics.Position, Silk.NET.DXGI.Format.FormatR32G32B32Float, inputSlot: 0, alignedByteOffset: Convert.ToUInt32(Marshal.OffsetOf<SommetPosition>(nameof(_position))));

        _inputLayoutDesc = builder.Build();
    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="position"></param>
    public SommetPosition(Vector3 position)
    {
        _position = position;
    }

    /// <summary>
    /// Constructeur par défaut
    /// </summary>
    public unsafe SommetPosition()
    : this(Vector3.Zero)
    { }

    /// <summary>
    /// Constructeur de pie
    /// </summary>
    public unsafe SommetPosition(SommetPosition other)
    : this(other._position)
    { }

    public SommetPosition Clone()
    {
        return new SommetPosition(this);
    }
}
