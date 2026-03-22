using System;
using System.Linq;
using System.Numerics;
using PetitMoteur3D.Core.Math;
using PetitMoteur3D.Graphics;
using PetitMoteur3D.Graphics.Buffers;
using PetitMoteur3D.Graphics.RenderTechniques;
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
    public SubObjet3D[] SubObjects { get { return _subObjects; } }


    public VertexBuffer VertexBuffer => _vertexBuffer;
    public VertexBuffer VertexBufferPosition => _vertexBufferPosition;
    public IndexBuffer IndexBuffer => _indexBuffer;
    public readonly D3DPrimitiveTopology Topology = D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist;

    public ushort[] Indices => _indices;

    /// <inheritdoc/>
    public RenderPassType[] SupportedRenderPasses { get; set; } = [RenderPassType.ForwardOpac, RenderPassType.DeferredShadingGeometry, RenderPassType.DepthTest, RenderPassType.ShadowMap];
    #endregion

    #region Protected Properties
    protected abstract bool SupportShadow { get; }
    protected ref readonly Matrix4x4 MatWorld { get { return ref _matWorld; } }
    protected GraphicBufferFactory BufferFactory { get { return _bufferFactory; } }
    protected Action<D3D11GraphicPipeline, SubObjet3D> AdditionalDrawConfig { get; set; }
    protected Action<D3D11GraphicPipeline, SubObjet3D> PostDrawConfig { get; set; }
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
    private SubObjet3D[] _subObjects;

    private readonly string _name;

    private bool _disposed;
    private readonly GraphicBufferFactory _bufferFactory;

    //private readonly DepthTestRenderPass _depthTestRenderPass;
    //private readonly ForwardOpaqueRenderPass _forwardOpaqueRenderPass;
    //private readonly DeferredGeometryRenderPass _deferredGeometryRenderPass;
    //private readonly DeferredLightningRenderPass _deferredLightningRenderPass;
    //private readonly ShadowMapRenderPass _shadowMapRenderPass;

    protected BaseObjet3D(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, string name = "")
    {
        _scale = Vector3.One;
        _position = Vector3.Zero;
        _orientation = new Orientation3D();
        _matWorld = Matrix4x4.Identity;

        _sommets = Array.Empty<Sommet>();
        _indices = Array.Empty<ushort>();

        if (string.IsNullOrEmpty(name))
        {
            _name = this.GetType().Name + "_" + Guid.NewGuid().ToString();
        }
        else
        {
            _name = name;
        }

        _bufferFactory = graphicDeviceRessourceFactory.BufferFactory;

        _vertexBuffer = default;
        _vertexBufferPosition = default;
        _indexBuffer = default;

        //_depthTestRenderPass = renderPassFactory.Create<DepthTestRenderPass>($"{_name}_DepthTestRenderPass");
        //_forwardOpaqueRenderPass = renderPassFactory.Create<ForwardOpaqueRenderPass>($"{_name}_ForwardRenderPass");
        //_deferredGeometryRenderPass = renderPassFactory.Create<DeferredGeometryRenderPass>($"{_name}_DeferredGeometryRenderPass");
        //_deferredLightningRenderPass = renderPassFactory.Create<DeferredLightningRenderPass>($"{_name}_DeferredLightningRenderPass");
        //_shadowMapRenderPass = renderPassFactory.Create<ShadowMapRenderPass>($"{_name}_ShadowMapRenderPass");

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
    //public virtual unsafe void Draw(RenderPassType renderPass, SceneViewContext scene)
    //{
    //    Matrix4x4 matViewProj = scene.MatViewProj;
    //    Matrix4x4 matViewProjLight = scene.MatViewProjLight;
    //    if (renderPass == RenderPassType.DepthTest)
    //    {
    //        // Choisir la topologie des primitives
    //        _depthTestRenderPass.UpdatePrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
    //        // Source des sommets
    //        _depthTestRenderPass.UpdateVertexBuffer(_vertexBufferPosition);
    //        // Source des index
    //        _depthTestRenderPass.UpdateIndexBuffer(_indexBuffer);
    //        // input layout des sommets
    //        foreach (SubObjet3D subObjet3D in _subObjects)
    //        {
    //            _depthTestRenderPass.UpdateVertexShaderConstantBuffer(new DepthTestRenderPass.VertexConstantBufferParams()
    //            {
    //                matWorldViewProj = Matrix4x4.Transpose(subObjet3D.Transformation * _matWorld * matViewProj)
    //            });

    //            _depthTestRenderPass.UpdatePixelShaderConstantBuffer(new DepthTestRenderPass.PixelConstantBufferParams()
    //            {
    //                successColor = new Vector4(0, 255, 0, 1),
    //                failColor = new Vector4(255, 0, 0, 1)
    //            });

    //            _depthTestRenderPass.Bind();

    //            // **** Rendu de l’objet
    //            _depthTestRenderPass.DrawIndexed((uint)_indices.Length, 0, 0);
    //        }
    //    }
    //    else if (renderPass == RenderPassType.ForwardOpac)
    //    {
    //        // Choisir la topologie des primitives
    //        _forwardOpaqueRenderPass.UpdatePrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
    //        // Source des sommets
    //        _forwardOpaqueRenderPass.UpdateVertexBuffer(_vertexBuffer);
    //        // Source des index
    //        _forwardOpaqueRenderPass.UpdateIndexBuffer(_indexBuffer);

    //        foreach (SubObjet3D subObjet3D in _subObjects)
    //        {
    //            // Initialiser et sélectionner les « constantes » des shaders
    //            _forwardOpaqueRenderPass.UpdateSceneConstantBuffer(new ForwardOpaqueRenderPass.SceneConstantBufferParams()
    //            {
    //                LightParams = new ForwardOpaqueRenderPass.LightParams()
    //                {
    //                    Position = scene.Light.Position,
    //                    Direction = scene.Light.Direction,
    //                    AmbiantColor = scene.Light.AmbiantColor,
    //                    DiffuseColor = scene.Light.DiffuseColor,
    //                    Enable = Convert.ToInt32(scene.ShowShadow),
    //                    EnableShadow = Convert.ToInt32(scene.ShowShadow),
    //                },
    //                CameraPos = scene.GameCameraPos
    //            });

    //            Matrix4x4 matrixWorld = subObjet3D.Transformation * _matWorld;
    //            _forwardOpaqueRenderPass.UpdateVertexObjectConstantBuffer(new ForwardOpaqueRenderPass.VertexObjectConstantBufferParams()
    //            {
    //                matWorldViewProj = Matrix4x4.Transpose(matrixWorld * matViewProj),
    //                matWorld = Matrix4x4.Transpose(matrixWorld),
    //                matWorldViewProjLight = Matrix4x4.Transpose(matrixWorld * matViewProjLight),
    //            });

    //            _forwardOpaqueRenderPass.UpdatePixelObjectConstantBuffer(new ForwardOpaqueRenderPass.PixelObjectConstantBufferParams()
    //            {
    //                Material = new ForwardOpaqueRenderPass.MaterialParams()
    //                {
    //                    AmbiantColor = subObjet3D.Material.Ambient,
    //                    DiffuseColor = subObjet3D.Material.Diffuse,
    //                    SpecularColor = subObjet3D.Material.Specular,
    //                    SpecularPower = subObjet3D.Material.SpecularPower,
    //                    HasDiffuseTexture = Convert.ToInt32(subObjet3D.Material.DiffuseTexture is not null),
    //                    HasNormalTexture = Convert.ToInt32(subObjet3D.Material.NormalTexture is not null),
    //                }
    //            });

    //            // Activation de la texture
    //            _forwardOpaqueRenderPass.UpdateTexture(subObjet3D.Material.DiffuseTexture?.ShaderRessourceView ?? new ComPtr<ID3D11ShaderResourceView>());
    //            _forwardOpaqueRenderPass.UpdateNormalMap(subObjet3D.Material.NormalTexture?.ShaderRessourceView ?? new ComPtr<ID3D11ShaderResourceView>());
    //            _forwardOpaqueRenderPass.UpdateShadowMap(scene.ShadowMap.DepthTexture.ShaderRessourceView);

    //            _forwardOpaqueRenderPass.Bind();

    //            // **** Rendu de l’objet
    //            _forwardOpaqueRenderPass.DrawIndexed((uint)_indices.Length, 0, 0);

    //            _forwardOpaqueRenderPass.ClearPixelShaderResources();
    //        }
    //    }
    //    else if (renderPass == RenderPassType.DeferredShadingGeometry)
    //    {
    //        // Choisir la topologie des primitives
    //        _deferredGeometryRenderPass.UpdatePrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
    //        // Source des sommets
    //        _deferredGeometryRenderPass.UpdateVertexBuffer(_vertexBuffer);
    //        // Source des index
    //        _deferredGeometryRenderPass.UpdateIndexBuffer(_indexBuffer);

    //        foreach (SubObjet3D subObjet3D in _subObjects)
    //        {
    //            // Initialiser et sélectionner les « constantes » des shaders
    //            _deferredGeometryRenderPass.UpdateSceneConstantBuffer(new DeferredGeometryRenderPass.SceneConstantBufferParams()
    //            {
    //                LightParams = new DeferredGeometryRenderPass.LightParams()
    //                {
    //                    Position = scene.Light.Position,
    //                    Direction = scene.Light.Direction,
    //                    AmbiantColor = scene.Light.AmbiantColor,
    //                    DiffuseColor = scene.Light.DiffuseColor,
    //                    Enable = Convert.ToInt32(scene.ShowShadow),
    //                },
    //                CameraPos = scene.GameCameraPos
    //            });

    //            Matrix4x4 matrixWorld = subObjet3D.Transformation * _matWorld;
    //            _deferredGeometryRenderPass.UpdateVertexObjectConstantBuffer(new DeferredGeometryRenderPass.VertexObjectConstantBufferParams()
    //            {
    //                matWorldViewProj = Matrix4x4.Transpose(matrixWorld * matViewProj),
    //                matWorld = Matrix4x4.Transpose(matrixWorld),
    //            });

    //            _deferredGeometryRenderPass.UpdatePixelObjectConstantBuffer(new DeferredGeometryRenderPass.PixelObjectConstantBufferParams()
    //            {
    //                Material = new DeferredGeometryRenderPass.MaterialParams()
    //                {
    //                    AmbiantColor = subObjet3D.Material.Ambient,
    //                    DiffuseColor = subObjet3D.Material.Diffuse,
    //                    SpecularColor = subObjet3D.Material.Specular,
    //                    SpecularPower = subObjet3D.Material.SpecularPower,
    //                    HasDiffuseTexture = Convert.ToInt32(subObjet3D.Material.DiffuseTexture is not null),
    //                    HasNormalTexture = Convert.ToInt32(subObjet3D.Material.NormalTexture is not null),
    //                }
    //            });

    //            // Activation de la texture
    //            _deferredGeometryRenderPass.UpdateTexture(subObjet3D.Material.DiffuseTexture?.ShaderRessourceView ?? new ComPtr<ID3D11ShaderResourceView>());
    //            _deferredGeometryRenderPass.UpdateNormalMap(subObjet3D.Material.NormalTexture?.ShaderRessourceView ?? new ComPtr<ID3D11ShaderResourceView>());

    //            _deferredGeometryRenderPass.Bind();

    //            // **** Rendu de l’objet
    //            _deferredGeometryRenderPass.DrawIndexed((uint)_indices.Length, 0, 0);

    //            _deferredGeometryRenderPass.ClearPixelShaderResources();
    //        }
    //    }
    //    else if (renderPass == RenderPassType.DeferredShadingLightning)
    //    {
    //        // Choisir la topologie des primitives
    //        _deferredLightningRenderPass.UpdatePrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
    //        // Source des sommets
    //        _deferredLightningRenderPass.UpdateVertexBuffer(_vertexBuffer);
    //        // Source des index
    //        _deferredLightningRenderPass.UpdateIndexBuffer(_indexBuffer);

    //        foreach (SubObjet3D subObjet3D in _subObjects)
    //        {
    //            // Initialiser et sélectionner les « constantes » des shaders
    //            _deferredLightningRenderPass.UpdateSceneConstantBuffer(new DeferredLightningRenderPass.SceneConstantBufferParams()
    //            {
    //                LightParams = new DeferredLightningRenderPass.LightParams()
    //                {
    //                    Position = scene.Light.Position,
    //                    Direction = scene.Light.Direction,
    //                    AmbiantColor = scene.Light.AmbiantColor,
    //                    DiffuseColor = scene.Light.DiffuseColor,
    //                    Enable = Convert.ToInt32(scene.ShowShadow),
    //                },
    //                CameraPos = scene.GameCameraPos
    //            });

    //            Matrix4x4 matrixWorld = subObjet3D.Transformation * _matWorld;
    //            _deferredLightningRenderPass.UpdateVertexObjectConstantBuffer(new DeferredLightningRenderPass.VertexObjectConstantBufferParams()
    //            {
    //                matWorldViewProj = Matrix4x4.Transpose(matrixWorld * matViewProj),
    //                matWorld = Matrix4x4.Transpose(matrixWorld),
    //            });

    //            _deferredLightningRenderPass.Bind();

    //            // **** Rendu de l’objet
    //            _deferredLightningRenderPass.DrawIndexed((uint)_indices.Length, 0, 0);

    //            _deferredLightningRenderPass.ClearPixelShaderResources();
    //        }
    //    }
    //    else if (renderPass == RenderPassType.ShadowMap)
    //    {
    //        // Choisir la topologie des primitives
    //        _shadowMapRenderPass.UpdatePrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
    //        // Source des sommets
    //        _shadowMapRenderPass.UpdateVertexBuffer(_vertexBufferPosition);
    //        // Source des index
    //        _shadowMapRenderPass.UpdateIndexBuffer(_indexBuffer);
    //        foreach (SubObjet3D subObjet3D in _subObjects)
    //        {
    //            _shadowMapRenderPass.UpdateVertexShaderConstantBuffer(new ShadowMapRenderPass.VertexShaderConstantBufferParams()
    //            {
    //                matWorldViewProj = Matrix4x4.Transpose(subObjet3D.Transformation * _matWorld * matViewProjLight)
    //            });

    //            _shadowMapRenderPass.Bind();

    //            // **** Rendu de l’objet
    //            _shadowMapRenderPass.DrawIndexed((uint)_indices.Length, 0, 0);
    //        }
    //    }
    //}

    /// <inheritdoc/>
    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);

        foreach (SubObjet3D subObjet3D in _subObjects)
        {
            subObjet3D.Accept(visitor);
        }
    }

    protected virtual void Initialisation()
    {
        _sommets = InitVertex();
        _indices = InitIndex();
        _subObjects = InitSubObjets();

        InitBuffers(_bufferFactory, _sommets, _indices);
    }

    /// <summary>
    /// Initialise les vertex
    /// </summary>
    /// <returns></returns>
    protected abstract Sommet[] InitVertex();

    /// <summary>
    /// Initialise l'index de rendu
    /// </summary>
    /// <returns></returns>
    protected abstract ushort[] InitIndex();

    /// <summary>
    /// Initialise les parties de l'objet pour le rendu
    /// </summary>
    /// <returns></returns>
    protected abstract SubObjet3D[] InitSubObjets();

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

            //_forwardOpaqueRenderPass.Dispose();
            //_depthTestRenderPass.Dispose();
            //_deferredGeometryRenderPass.Dispose();
            //_deferredLightningRenderPass.Dispose();
            //_shadowMapRenderPass.Dispose();

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
