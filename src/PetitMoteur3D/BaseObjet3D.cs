using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using PetitMoteur3D.Core.Math;
using PetitMoteur3D.Graphics;
using PetitMoteur3D.Graphics.Buffers;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D;

internal abstract class BaseObjet3D : IObjet3D, IDisposable
{
    #region Public Properties
    /// <inheritdoc/>
    public ref readonly Vector3 Scale { get { return ref _scale; } }
    public ref readonly Vector3 Position { get { return ref _position; } }
    public Orientation3D Orientation { get { return _orientation; } }
    /// <inheritdoc/>
    public string Name { get { return _name; } }
    public Mesh Mesh { get { return _mesh; } }
    public Sommet[] Sommets => _sommets;
    public ushort[] Indices => _indices;

    public VertexBuffer VertexBuffer => _vertexBuffer;
    public VertexBuffer VertexBufferPosition => _vertexBufferPosition;
    public IndexBuffer IndexBuffer => _indexBuffer;
    public readonly D3DPrimitiveTopology Topology = D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist;
    #endregion

    #region Protected Properties
    protected ref readonly Matrix4x4 MatWorld { get { return ref _matWorld; } }
    protected GraphicBufferFactory BufferFactory { get { return _bufferFactory; } }
    #endregion

    private VertexBuffer _vertexBuffer;
    private VertexBuffer _vertexBufferPosition;
    private IndexBuffer _indexBuffer;

    private Matrix4x4 _matWorld;

    private Vector3 _scale;
    private Vector3 _position;
    private readonly Orientation3D _orientation;

    private static readonly Vector3 ZeroRotation = Vector3.Zero;

    private Sommet[] _sommets;
    private ushort[] _indices;
    private Mesh _mesh;

    private readonly string _name;

    private bool _disposed;
    private readonly GraphicBufferFactory _bufferFactory;

    protected BaseObjet3D(Mesh mesh, GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, string name = "")
    {
        _scale = Vector3.One;
        _position = Vector3.Zero;
        _orientation = new Orientation3D();
        _matWorld = Matrix4x4.Identity;

        _mesh = mesh;
        _sommets = mesh.Sommets;
        _indices = mesh.Indices;

        if (string.IsNullOrEmpty(name))
        {
            _name = this.GetType().Name + "_" + Guid.NewGuid().ToString();
        }
        else
        {
            _name = name;
        }

        _bufferFactory = graphicDeviceRessourceFactory.BufferFactory;

        _vertexBuffer = default!;
        _vertexBufferPosition = default!;
        _indexBuffer = default!;

        _disposed = false;
    }

    /// <inheritdoc/>
    public ref readonly Vector3 Move(float dx, float dy, float dz)
    {
        _position.X += dx;
        _position.Y += dy;
        _position.Z += dz;
        UpdateMatWorld();
        return ref _position;
    }

    /// <inheritdoc/>
    public ref readonly Vector3 Move(Vector3 move)
    {
        return ref Move(in move);
    }

    /// <inheritdoc/>
    public ref readonly Vector3 Move(scoped ref readonly Vector3 move)
    {
        return ref Move(move.X, move.Y, move.Z);
    }

    /// <inheritdoc/>
    public ref readonly Vector3 SetPosition(float x, float y, float z)
    {
        _position.X = x;
        _position.Y = y;
        _position.Z = z;
        UpdateMatWorld();
        return ref _position;
    }

    /// <inheritdoc/>
    public ref readonly Vector3 SetPosition(Vector3 position)
    {
        return ref SetPosition(in position);
    }

    /// <inheritdoc/>
    public ref readonly Vector3 SetPosition(scoped ref readonly Vector3 position)
    {
        return ref SetPosition(position.X, position.Y, position.Z);
    }

    /// <inheritdoc/>
    /// <remarks>Currently only return zero vector</remarks>
    public ref readonly Vector3 RotateEuler(ref readonly Vector3 rotation)
    {
        Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        _orientation.Rotate(in quaternion);
        UpdateMatWorld();
        return ref ZeroRotation;
    }


    /// <inheritdoc/>
    /// <remarks>Currently only return zero vector</remarks>
    public ref readonly Vector3 Rotate(ref readonly Vector3 axis, float angle)
    {
        _orientation.Rotate(in axis, angle);
        UpdateMatWorld();
        return ref ZeroRotation;
    }

    /// <inheritdoc/>
    public ref readonly Vector3 SetScale(float x, float y, float z)
    {
        _scale.X = x;
        _scale.Y = y;
        _scale.Z = z;
        UpdateMatWorld();
        return ref _scale;
    }

    /// <inheritdoc/>
    public ref readonly Vector3 SetScale(float scale)
    {
        return ref SetScale(scale, scale, scale);
    }

    /// <inheritdoc/>
    public ref readonly Vector3 SetScale(Vector3 scale)
    {
        return ref SetScale(in scale);
    }

    /// <inheritdoc/>
    public ref readonly Vector3 SetScale(scoped ref readonly Vector3 scale)
    {
        return ref SetScale(scale.X, scale.Y, scale.Z);
    }

    /// <inheritdoc/>
    public virtual void Update(float elapsedTime)
    {

    }

    public ObjectViewContext GetViewContext()
    {
        return new ObjectViewContext()
        {
            MatWorld = _matWorld
        };
    }

    /// <inheritdoc/>
    public void Accept(IVisitor<IObjet3D> visitor)
    {
        visitor.Visit(this);
    }

    /// <inheritdoc/>
    public void Accept(IVisitor<BaseObjet3D> visitor)
    {
        visitor.Visit(this);
    }

    /// <inheritdoc/>
    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }

    protected virtual void Initialisation()
    {
        InitBuffers(_bufferFactory, _sommets, _indices);
    }

    [MemberNotNull(nameof(_vertexBuffer))]
    [MemberNotNull(nameof(_vertexBufferPosition))]
    [MemberNotNull(nameof(_indexBuffer))]
    private unsafe void InitBuffers<TIndice>(GraphicBufferFactory bufferFactory, Sommet[] sommets, TIndice[] indices)
        where TIndice : unmanaged
    {
        // Create our vertex buffer.
        _vertexBuffer = bufferFactory.CreateVertexBuffer<Sommet>(sommets, Usage.Immutable, CpuAccessFlag.None, $"{_name}_VertexBuffer");
        _vertexBufferPosition = bufferFactory.CreateVertexBuffer<SommetPosition>(sommets.Select(s => new SommetPosition(s.Position)).ToArray(), Usage.Immutable, CpuAccessFlag.None, $"{_name}_VertexBuffer");

        // Create our index buffer.
        _indexBuffer = bufferFactory.CreateIndexBuffer<TIndice>(indices, Usage.Immutable, CpuAccessFlag.None, $"{_name}_IndexBuffer");
    }

    protected void UpdateMatWorld()
    {
        _matWorld = Matrix4x4.CreateScale(_scale) * Matrix4x4.CreateFromQuaternion(_orientation.Quaternion) * Matrix4x4.CreateTranslation(_position);
    }

    ~BaseObjet3D()
    {
        Dispose(disposing: false);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null

            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();

            _disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
