using System;
using System.Linq;
using System.Numerics;
using PetitMoteur3D.Core.Math;
using PetitMoteur3D.Graphics;
using PetitMoteur3D.Graphics.Shaders;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D;

internal abstract class BaseObjet3D : IObjet3D, IDisposable
{
    #region Public Properties
    /// <inheritdoc/>
    public ref readonly System.Numerics.Vector3 Scale { get { return ref _scale; } }
    public ref readonly System.Numerics.Vector3 Position { get { return ref _position; } }
    public Orientation3D Orientation { get { return _orientation; } }
    /// <inheritdoc/>
    public string Name { get { return _name; } }

    /// <inheritdoc/>
    public RenderPassType[] SupportedRenderPasses { get; set; } = [RenderPassType.ForwardOpac, RenderPassType.DeferredShadingGeometry, RenderPassType.DepthTest, RenderPassType.ShadowMap];
    #endregion

    #region Protected Properties
    protected abstract bool SupportShadow { get; }
    protected ref readonly System.Numerics.Matrix4x4 MatWorld { get { return ref _matWorld; } }
    protected ref readonly SubObjet3D[] SubObjects { get { return ref _subObjects; } }
    protected ComPtr<ID3D11Buffer> IndexBuffer { get { return _indexBuffer; } }
    protected int NbIndices { get { return _indices.Length; } }
    protected GraphicBufferFactory BufferFactory { get { return _bufferFactory; } }
    protected Action<D3D11GraphicPipeline, SubObjet3D> AdditionalDrawConfig { get; set; }
    protected Action<D3D11GraphicPipeline, SubObjet3D> PostDrawConfig { get; set; }
    #endregion

    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private ComPtr<ID3D11Buffer> _vertexBufferPosition;
    private ComPtr<ID3D11Buffer> _indexBuffer;

    private System.Numerics.Matrix4x4 _matWorld;

    private System.Numerics.Vector3 _scale;
    private System.Numerics.Vector3 _position;
    private readonly Orientation3D _orientation;

    private static readonly System.Numerics.Vector3 ZeroRotation = System.Numerics.Vector3.Zero;

    private Sommet[] _sommets;
    private ushort[] _indices;
    private SubObjet3D[] _subObjects;

    private unsafe readonly uint _vertexStride = (uint)sizeof(Sommet);
    private unsafe readonly uint _vertexPositionStride = (uint)sizeof(SommetPosition);

    private readonly string _name;

    private bool _disposed;
    private readonly GraphicBufferFactory _bufferFactory;

    private readonly DepthTestRenderPass _depthTestRenderPass;
    private readonly ForwardOpaqueRenderPass _forwardOpaqueRenderPass;
    private readonly DeferredGeometryRenderPass _deferredGeometryRenderPass;
    private readonly DeferredLightningRenderPass _deferredLightningRenderPass;
    private readonly ShadowMapRenderPass _shadowMapRenderPass;

    protected BaseObjet3D(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, RenderPassFactory renderPassFactory, string name = "")
    {
        _scale = System.Numerics.Vector3.One;
        _position = System.Numerics.Vector3.Zero;
        _orientation = new Orientation3D();
        _matWorld = System.Numerics.Matrix4x4.Identity;

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

        _depthTestRenderPass = renderPassFactory.Create<DepthTestRenderPass>($"{_name}_DepthTestRenderPass");
        _forwardOpaqueRenderPass = renderPassFactory.Create<ForwardOpaqueRenderPass>($"{_name}_ForwardRenderPass");
        _deferredGeometryRenderPass = renderPassFactory.Create<DeferredGeometryRenderPass>($"{_name}_DeferredGeometryRenderPass");
        _deferredLightningRenderPass = renderPassFactory.Create<DeferredLightningRenderPass>($"{_name}_DeferredLightningRenderPass");
        _shadowMapRenderPass = renderPassFactory.Create<ShadowMapRenderPass>($"{_name}_ShadowMapRenderPass");

        _disposed = false;
    }

    /// <inheritdoc/>
    public ref readonly System.Numerics.Vector3 Move(float dx, float dy, float dz)
    {
        _position.X += dx;
        _position.Y += dy;
        _position.Z += dz;
        UpdateMatWorld();
        return ref _position;
    }

    /// <inheritdoc/>
    public ref readonly System.Numerics.Vector3 Move(System.Numerics.Vector3 move)
    {
        return ref Move(in move);
    }

    /// <inheritdoc/>
    public ref readonly System.Numerics.Vector3 Move(scoped ref readonly System.Numerics.Vector3 move)
    {
        return ref Move(move.X, move.Y, move.Z);
    }

    /// <inheritdoc/>
    public ref readonly System.Numerics.Vector3 SetPosition(float x, float y, float z)
    {
        _position.X = x;
        _position.Y = y;
        _position.Z = z;
        UpdateMatWorld();
        return ref _position;
    }

    /// <inheritdoc/>
    public ref readonly System.Numerics.Vector3 SetPosition(System.Numerics.Vector3 position)
    {
        return ref SetPosition(in position);
    }

    /// <inheritdoc/>
    public ref readonly System.Numerics.Vector3 SetPosition(scoped ref readonly System.Numerics.Vector3 position)
    {
        return ref SetPosition(position.X, position.Y, position.Z);
    }

    /// <inheritdoc/>
    /// <remarks>Currently only return zero vector</remarks>
    public ref readonly System.Numerics.Vector3 RotateEuler(ref readonly System.Numerics.Vector3 rotation)
    {
        System.Numerics.Quaternion quaternion = System.Numerics.Quaternion.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        _orientation.Rotate(in quaternion);
        UpdateMatWorld();
        return ref ZeroRotation;
    }


    /// <inheritdoc/>
    /// <remarks>Currently only return zero vector</remarks>
    public ref readonly System.Numerics.Vector3 Rotate(ref readonly System.Numerics.Vector3 axis, float angle)
    {
        _orientation.Rotate(in axis, angle);
        UpdateMatWorld();
        return ref ZeroRotation;
    }

    /// <inheritdoc/>
    public ref readonly System.Numerics.Vector3 SetScale(float x, float y, float z)
    {
        _scale.X = x;
        _scale.Y = y;
        _scale.Z = z;
        UpdateMatWorld();
        return ref _scale;
    }

    /// <inheritdoc/>
    public ref readonly System.Numerics.Vector3 SetScale(float scale)
    {
        return ref SetScale(scale, scale, scale);
    }

    /// <inheritdoc/>
    public ref readonly System.Numerics.Vector3 SetScale(System.Numerics.Vector3 scale)
    {
        return ref SetScale(in scale);
    }

    /// <inheritdoc/>
    public ref readonly System.Numerics.Vector3 SetScale(scoped ref readonly System.Numerics.Vector3 scale)
    {
        return ref SetScale(scale.X, scale.Y, scale.Z);
    }

    /// <inheritdoc/>
    public virtual void Update(float elapsedTime)
    {

    }

    /// <inheritdoc/>
    public virtual unsafe void Draw(RenderPassType renderPass, SceneViewContext scene)
    {
        Matrix4x4 matViewProj = scene.MatViewProj;
        Matrix4x4 matViewProjLight = scene.MatViewProjLight;
        if (renderPass == RenderPassType.DepthTest)
        {
            // Choisir la topologie des primitives
            _depthTestRenderPass.UpdatePrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
            _depthTestRenderPass.SetPrimitiveTopology();
            // Source des sommets
            _depthTestRenderPass.UpdateVertexBuffer(_vertexBufferPosition, _vertexPositionStride);
            _depthTestRenderPass.SetVertexBuffer();
            // Source des index
            _depthTestRenderPass.UpdateIndexBuffer(_indexBuffer, Silk.NET.DXGI.Format.FormatR16Uint);
            _depthTestRenderPass.SetIndexBuffer();
            // input layout des sommets
            _depthTestRenderPass.SetInputLayout();
            foreach (SubObjet3D subObjet3D in _subObjects)
            {
                _depthTestRenderPass.UpdateVertexShaderConstantBuffer(new DepthTestRenderPass.VertexConstantBufferParams()
                {
                    matWorldViewProj = System.Numerics.Matrix4x4.Transpose(subObjet3D.Transformation * _matWorld * matViewProj)
                });

                _depthTestRenderPass.UpdatePixelShaderConstantBuffer(new DepthTestRenderPass.PixelConstantBufferParams()
                {
                    successColor = new System.Numerics.Vector4(0, 255, 0, 1),
                    failColor = new System.Numerics.Vector4(255, 0, 0, 1)
                });

                // Activer le VS
                _depthTestRenderPass.SetVertexShader();
                _depthTestRenderPass.SetVertexShaderConstantBuffers();
                // Activer le GS
                _depthTestRenderPass.SetGeometryShader();
                // Activer le PS
                _depthTestRenderPass.SetPixelShader();
                _depthTestRenderPass.SetPixelShaderConstantBuffers();

                // Le sampler state
                _depthTestRenderPass.SetSamplers();

                // **** Rendu de l’objet
                _depthTestRenderPass.DrawIndexed((uint)_indices.Length, 0, 0);
            }
        }
        else if (renderPass == RenderPassType.ForwardOpac)
        {
            // Choisir la topologie des primitives
            _forwardOpaqueRenderPass.UpdatePrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
            _forwardOpaqueRenderPass.SetPrimitiveTopology();
            // Source des sommets
            _forwardOpaqueRenderPass.UpdateVertexBuffer(_vertexBuffer, _vertexStride);
            _forwardOpaqueRenderPass.SetVertexBuffer();
            // Source des index
            _forwardOpaqueRenderPass.UpdateIndexBuffer(_indexBuffer, Silk.NET.DXGI.Format.FormatR16Uint);
            _forwardOpaqueRenderPass.SetIndexBuffer();
            // input layout des sommets
            _forwardOpaqueRenderPass.SetInputLayout();

            foreach (SubObjet3D subObjet3D in _subObjects)
            {
                // Initialiser et sélectionner les « constantes » des shaders
                _forwardOpaqueRenderPass.UpdateSceneConstantBuffer(new ForwardOpaqueRenderPass.SceneConstantBufferParams()
                {
                    LightParams = new ForwardOpaqueRenderPass.LightParams()
                    {
                        Position = scene.Light.Position,
                        Direction = scene.Light.Direction,
                        AmbiantColor = scene.Light.AmbiantColor,
                        DiffuseColor = scene.Light.DiffuseColor,
                        Enable = Convert.ToInt32(scene.ShowShadow),
                        EnableShadow = Convert.ToInt32(scene.ShowShadow),
                    },
                    CameraPos = scene.GameCameraPos
                });

                Matrix4x4 matrixWorld = subObjet3D.Transformation * _matWorld;
                _forwardOpaqueRenderPass.UpdateVertexObjectConstantBuffer(new ForwardOpaqueRenderPass.VertexObjectConstantBufferParams()
                {
                    matWorldViewProj = System.Numerics.Matrix4x4.Transpose(matrixWorld * matViewProj),
                    matWorld = System.Numerics.Matrix4x4.Transpose(matrixWorld),
                    matWorldViewProjLight = System.Numerics.Matrix4x4.Transpose(matrixWorld * matViewProjLight),
                });

                _forwardOpaqueRenderPass.UpdatePixelObjectConstantBuffer(new ForwardOpaqueRenderPass.PixelObjectConstantBufferParams()
                {
                    ambiantMaterialValue = subObjet3D.Material.Ambient,
                    diffuseMaterialValue = subObjet3D.Material.Diffuse,
                    HasDiffuseTexture = Convert.ToInt32(subObjet3D.Material.DiffuseTexture is not null),
                    HasNormalTexture = Convert.ToInt32(subObjet3D.Material.NormalTexture is not null),
                });

                // Activer le VS
                _forwardOpaqueRenderPass.SetVertexShader();
                _forwardOpaqueRenderPass.SetVertexShaderConstantBuffers();
                // Activer le GS
                _forwardOpaqueRenderPass.SetGeometryShader();
                // Activer le PS
                _forwardOpaqueRenderPass.SetPixelShader();
                _forwardOpaqueRenderPass.SetPixelShaderConstantBuffers();
                // Activation de la texture
                _forwardOpaqueRenderPass.UpdateTexture(subObjet3D.Material.DiffuseTexture?.ShaderRessourceView ?? new ComPtr<ID3D11ShaderResourceView>());
                _forwardOpaqueRenderPass.UpdateNormalMap(subObjet3D.Material.NormalTexture?.ShaderRessourceView ?? new ComPtr<ID3D11ShaderResourceView>());
                _forwardOpaqueRenderPass.UpdateShadowMap(scene.ShadowMap.DepthTexture.ShaderRessourceView);
                _forwardOpaqueRenderPass.SetPixelShaderRessources();

                // Le sampler state
                _forwardOpaqueRenderPass.SetSamplers();

                // **** Rendu de l’objet
                _forwardOpaqueRenderPass.DrawIndexed((uint)_indices.Length, 0, 0);

                _forwardOpaqueRenderPass.ClearPixelShaderResources();
            }
        }
        else if (renderPass == RenderPassType.DeferredShadingGeometry)
        {
            // Choisir la topologie des primitives
            _deferredGeometryRenderPass.UpdatePrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
            _deferredGeometryRenderPass.SetPrimitiveTopology();
            // Source des sommets
            _deferredGeometryRenderPass.UpdateVertexBuffer(_vertexBuffer, _vertexStride);
            _deferredGeometryRenderPass.SetVertexBuffer();
            // Source des index
            _deferredGeometryRenderPass.UpdateIndexBuffer(_indexBuffer, Silk.NET.DXGI.Format.FormatR16Uint);
            _deferredGeometryRenderPass.SetIndexBuffer();
            // input layout des sommets
            _deferredGeometryRenderPass.SetInputLayout();

            foreach (SubObjet3D subObjet3D in _subObjects)
            {
                // Initialiser et sélectionner les « constantes » des shaders
                _deferredGeometryRenderPass.UpdateSceneConstantBuffer(new DeferredGeometryRenderPass.SceneConstantBufferParams()
                {
                    LightParams = new DeferredGeometryRenderPass.LightParams()
                    {
                        Position = scene.Light.Position,
                        Direction = scene.Light.Direction,
                        AmbiantColor = scene.Light.AmbiantColor,
                        DiffuseColor = scene.Light.DiffuseColor,
                        Enable = Convert.ToInt32(scene.ShowShadow),
                    },
                    CameraPos = scene.GameCameraPos
                });

                Matrix4x4 matrixWorld = subObjet3D.Transformation * _matWorld;
                _deferredGeometryRenderPass.UpdateVertexObjectConstantBuffer(new DeferredGeometryRenderPass.VertexObjectConstantBufferParams()
                {
                    matWorldViewProj = System.Numerics.Matrix4x4.Transpose(matrixWorld * matViewProj),
                    matWorld = System.Numerics.Matrix4x4.Transpose(matrixWorld),
                });

                _deferredGeometryRenderPass.UpdatePixelObjectConstantBuffer(new DeferredGeometryRenderPass.PixelObjectConstantBufferParams()
                {
                    ambiantMaterialValue = subObjet3D.Material.Ambient,
                    diffuseMaterialValue = subObjet3D.Material.Diffuse,
                    HasDiffuseTexture = Convert.ToInt32(subObjet3D.Material.DiffuseTexture is not null),
                    HasNormalTexture = Convert.ToInt32(subObjet3D.Material.NormalTexture is not null),
                });

                // Activer le VS
                _deferredGeometryRenderPass.SetVertexShader();
                _deferredGeometryRenderPass.SetVertexShaderConstantBuffers();
                // Activer le GS
                _deferredGeometryRenderPass.SetGeometryShader();
                // Activer le PS
                _deferredGeometryRenderPass.SetPixelShader();
                _deferredGeometryRenderPass.SetPixelShaderConstantBuffers();
                // Activation de la texture
                _deferredGeometryRenderPass.UpdateTexture(subObjet3D.Material.DiffuseTexture?.ShaderRessourceView ?? new ComPtr<ID3D11ShaderResourceView>());
                _deferredGeometryRenderPass.UpdateNormalMap(subObjet3D.Material.NormalTexture?.ShaderRessourceView ?? new ComPtr<ID3D11ShaderResourceView>());
                _deferredGeometryRenderPass.SetPixelShaderRessources();

                // Le sampler state
                _deferredGeometryRenderPass.SetSamplers();

                // **** Rendu de l’objet
                _deferredGeometryRenderPass.DrawIndexed((uint)_indices.Length, 0, 0);

                _deferredGeometryRenderPass.ClearPixelShaderResources();
            }
        }
        else if (renderPass == RenderPassType.DeferredShadingLightning)
        {
            // Choisir la topologie des primitives
            _deferredLightningRenderPass.UpdatePrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
            _deferredLightningRenderPass.SetPrimitiveTopology();
            // Source des sommets
            _deferredLightningRenderPass.UpdateVertexBuffer(_vertexBuffer, _vertexStride);
            _deferredLightningRenderPass.SetVertexBuffer();
            // Source des index
            _deferredLightningRenderPass.UpdateIndexBuffer(_indexBuffer, Silk.NET.DXGI.Format.FormatR16Uint);
            _deferredLightningRenderPass.SetIndexBuffer();
            // input layout des sommets
            _deferredLightningRenderPass.SetInputLayout();

            foreach (SubObjet3D subObjet3D in _subObjects)
            {
                // Initialiser et sélectionner les « constantes » des shaders
                _deferredLightningRenderPass.UpdateSceneConstantBuffer(new DeferredLightningRenderPass.SceneConstantBufferParams()
                {
                    LightParams = new DeferredLightningRenderPass.LightParams()
                    {
                        Position = scene.Light.Position,
                        Direction = scene.Light.Direction,
                        AmbiantColor = scene.Light.AmbiantColor,
                        DiffuseColor = scene.Light.DiffuseColor,
                        Enable = Convert.ToInt32(scene.ShowShadow),
                    },
                    CameraPos = scene.GameCameraPos
                });

                Matrix4x4 matrixWorld = subObjet3D.Transformation * _matWorld;
                _deferredLightningRenderPass.UpdateVertexObjectConstantBuffer(new DeferredLightningRenderPass.VertexObjectConstantBufferParams()
                {
                    matWorldViewProj = System.Numerics.Matrix4x4.Transpose(matrixWorld * matViewProj),
                    matWorld = System.Numerics.Matrix4x4.Transpose(matrixWorld),
                });

                // Activer le VS
                _deferredLightningRenderPass.SetVertexShader();
                _deferredLightningRenderPass.SetVertexShaderConstantBuffers();
                // Activer le GS
                _deferredLightningRenderPass.SetGeometryShader();
                // Activer le PS
                _deferredLightningRenderPass.SetPixelShader();
                _deferredLightningRenderPass.SetPixelShaderConstantBuffers();
                // Activation de la texture
                _deferredLightningRenderPass.SetPixelShaderRessources();

                // Le sampler state
                _deferredLightningRenderPass.SetSamplers();

                // **** Rendu de l’objet
                _deferredLightningRenderPass.DrawIndexed((uint)_indices.Length, 0, 0);

                _deferredLightningRenderPass.ClearPixelShaderResources();
            }
        }
        else if (renderPass == RenderPassType.ShadowMap)
        {
            // Choisir la topologie des primitives
            _shadowMapRenderPass.UpdatePrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
            _shadowMapRenderPass.SetPrimitiveTopology();
            // Source des sommets
            _shadowMapRenderPass.UpdateVertexBuffer(_vertexBufferPosition, _vertexPositionStride);
            _shadowMapRenderPass.SetVertexBuffer();
            // Source des index
            _shadowMapRenderPass.UpdateIndexBuffer(_indexBuffer, Silk.NET.DXGI.Format.FormatR16Uint);
            _shadowMapRenderPass.SetIndexBuffer();
            // input layout des sommets
            _shadowMapRenderPass.SetInputLayout();
            foreach (SubObjet3D subObjet3D in _subObjects)
            {
                _shadowMapRenderPass.UpdateVertexShaderConstantBuffer(new ShadowMapRenderPass.VertexShaderConstantBufferParams()
                {
                    matWorldViewProj = System.Numerics.Matrix4x4.Transpose(subObjet3D.Transformation * _matWorld * matViewProjLight)
                });

                // Activer le VS
                _shadowMapRenderPass.SetVertexShader();
                _shadowMapRenderPass.SetVertexShaderConstantBuffers();
                // Activer le GS
                _shadowMapRenderPass.SetGeometryShader();
                // Activer le PS
                _shadowMapRenderPass.SetPixelShader();
                _shadowMapRenderPass.SetPixelShaderConstantBuffers();

                // **** Rendu de l’objet
                _shadowMapRenderPass.DrawIndexed((uint)_indices.Length, 0, 0);
            }
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
        _matWorld = System.Numerics.Matrix4x4.CreateScale(_scale) * System.Numerics.Matrix4x4.CreateFromQuaternion(_orientation.Quaternion) * System.Numerics.Matrix4x4.CreateTranslation(_position);
    }

    public class SubObjet3D
{
    public ushort[] Indices;
    public System.Numerics.Matrix4x4 Transformation;
    public Material Material;
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

            _forwardOpaqueRenderPass.Dispose();
            _depthTestRenderPass.Dispose();
            _deferredGeometryRenderPass.Dispose();
            _deferredLightningRenderPass.Dispose();
            _shadowMapRenderPass.Dispose();

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
