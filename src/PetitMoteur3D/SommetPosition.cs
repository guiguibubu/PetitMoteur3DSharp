using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D;

internal struct SommetPosition
{
    private Vector3 _position;
    public readonly Vector3 Position => _position;

    /// <summary>
    /// Defini l’organisation de notre sommet
    /// </summary>
    public static InputElementDesc[] InputLayoutDesc => s_inputElements;

    private static readonly GlobalMemory s_semanticNamePosition;
    private static readonly InputElementDesc[] s_inputElements;
    static unsafe SommetPosition()
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
