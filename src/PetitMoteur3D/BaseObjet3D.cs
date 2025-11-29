using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PetitMoteur3D.Core.Math;
using PetitMoteur3D.Core.Memory;
using PetitMoteur3D.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace PetitMoteur3D;

internal abstract class BaseObjet3D : IObjet3D, IDisposable
{
    #region Public Properties
    /// <inheritdoc/>
    public ref readonly System.Numerics.Vector3 Position { get { return ref _position; } }
    public Orientation3D Orientation { get { return _orientation; } }
    /// <inheritdoc/>
    public string Name { get { return _name; } }
    #endregion

    #region Protected Properties
    protected ref readonly System.Numerics.Matrix4x4 MatWorld { get { return ref _matWorld; } }
    protected ref readonly SubObjet3D[] SubObjects { get { return ref _subObjects; } }
    protected ComPtr<ID3D11Buffer> IndexBuffer { get { return _indexBuffer; } }
    protected int NbIndices { get { return _indices.Length; } }
    protected GraphicBufferFactory BufferFactory { get { return _bufferFactory; } }
    protected Action<D3D11GraphicPipeline, SubObjet3D> AdditionalDrawConfig { get; set; }
    protected Action<D3D11GraphicPipeline, SubObjet3D> PostDrawConfig { get; set; }
    #endregion

    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private ComPtr<ID3D11Buffer> _constantBuffer;

    private ComPtr<ID3D11VertexShader> _vertexShader;
    private ComPtr<ID3D11InputLayout> _vertexLayout;
    private ComPtr<ID3D11PixelShader> _pixelShader;

    private ComPtr<ID3D11ShaderResourceView> _textureD3D;
    private ComPtr<ID3D11ShaderResourceView> _normalMap;
    private ComPtr<ID3D11SamplerState> _sampleState;

    private System.Numerics.Matrix4x4 _matWorld;

    private System.Numerics.Vector3 _position;
    private readonly Orientation3D _orientation;

    private static readonly System.Numerics.Vector3 ZeroRotation = System.Numerics.Vector3.Zero;

    private Sommet[] _sommets;
    private ushort[] _indices;
    private SubObjet3D[] _subObjects;

    private unsafe readonly uint _vertexStride = (uint)sizeof(Sommet);
    private static readonly uint _vertexOffset = 0;

    private readonly string _name;

    private static readonly IObjectPool<ObjectShadersParams> _objectShadersParamsPool = ObjectPoolFactory.Create<ObjectShadersParams>();
    private static readonly IObjectPool<SamplerDesc> _shaderDescPool = ObjectPoolFactory.Create<SamplerDesc>();
    private bool _disposed;
    private readonly GraphicBufferFactory _bufferFactory;
    private readonly ShaderManager _shaderManager;
    private readonly TextureManager _textureManager;

    protected BaseObjet3D(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, string name = "")
    {
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
        _shaderManager = graphicDeviceRessourceFactory.ShaderManager;
        _textureManager = graphicDeviceRessourceFactory.TextureManager;

        _vertexBuffer = default;
        _indexBuffer = default;
        _constantBuffer = default;

        _vertexShader = default;
        _vertexLayout = default;
        _pixelShader = default;

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
    public virtual void Update(float elapsedTime)
    {

    }

    /// <inheritdoc/>
    public virtual unsafe void Draw(D3D11GraphicPipeline graphicPipeline, ref readonly System.Numerics.Matrix4x4 matViewProj)
    {
        // Choisir la topologie des primitives
        graphicPipeline.InputAssemblerStage.SetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
        // Source des sommets
        graphicPipeline.InputAssemblerStage.SetVertexBuffers(0, 1, ref _vertexBuffer, in _vertexStride, in _vertexOffset);
        // Source des index
        graphicPipeline.InputAssemblerStage.SetIndexBuffer(_indexBuffer, Silk.NET.DXGI.Format.FormatR16Uint, 0);
        // input layout des sommets
        graphicPipeline.InputAssemblerStage.SetInputLayout(_vertexLayout);
        foreach (SubObjet3D subObjet3D in _subObjects)
        {
            // Initialiser et sélectionner les « constantes » des shaders
            _objectShadersParamsPool.Get(out ObjectPoolWrapper<ObjectShadersParams> shadersParamsWrapper);
            ref ObjectShadersParams shadersParams = ref shadersParamsWrapper.Data;
            shadersParams.matWorldViewProj = System.Numerics.Matrix4x4.Transpose(subObjet3D.Transformation * _matWorld * matViewProj).ToGeneric();
            shadersParams.matWorld = System.Numerics.Matrix4x4.Transpose(subObjet3D.Transformation * _matWorld).ToGeneric();
            shadersParams.ambiantMaterialValue = subObjet3D.Material.Ambient;
            shadersParams.diffuseMaterialValue = subObjet3D.Material.Diffuse;
            shadersParams.hasTexture = Convert.ToInt32(_textureD3D.Handle is not null);
            shadersParams.hasNormalMap = Convert.ToInt32(_normalMap.Handle is not null);

            graphicPipeline.RessourceFactory.UpdateSubresource(_constantBuffer, 0, in Unsafe.NullRef<Box>(), in shadersParams, 0, 0);

            // Activer le VS
            graphicPipeline.VertexShaderStage.SetShader(_vertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            graphicPipeline.VertexShaderStage.SetConstantBuffers(1, 1, ref _constantBuffer);
            // Activer le GS
            graphicPipeline.GeometryShaderStage.SetShader((ComPtr<ID3D11GeometryShader>)null, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            // Activer le PS
            graphicPipeline.PixelShaderStage.SetShader(_pixelShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            graphicPipeline.PixelShaderStage.SetConstantBuffers(1, 1, ref _constantBuffer);
            // Activation de la texture
            if (_textureD3D.Handle is not null)
            {
                graphicPipeline.PixelShaderStage.SetShaderResources(0, 1, ref _textureD3D);
            }
            if (_normalMap.Handle is not null)
            {
                graphicPipeline.PixelShaderStage.SetShaderResources(1, 1, ref _normalMap);
            }
            // Le sampler state
            graphicPipeline.PixelShaderStage.SetSamplers(0, 1, in _sampleState);

            // Additional draw config actions
            AdditionalDrawConfig?.Invoke(graphicPipeline, subObjet3D);

            // **** Rendu de l’objet
            graphicPipeline.DrawIndexed((uint)_indices.Length, 0, 0);

            _objectShadersParamsPool.Return(shadersParamsWrapper);

            if (_textureD3D.Handle is not null)
            {
                graphicPipeline.PixelShaderStage.ClearShaderResources(0);
            }
            if (_normalMap.Handle is not null)
            {
                graphicPipeline.PixelShaderStage.ClearShaderResources(1);
            }
        }
    }

    public void SetTexture(Texture texture)
    {
        _textureD3D = texture.TextureView;
    }

    public void SetNormalMapTexture(Texture texture)
    {
        _normalMap = texture.TextureView;
    }

    protected virtual void Initialisation()
    {
        _sommets = InitVertex();
        _indices = InitIndex();
        _subObjects = InitSubObjets();

        InitBuffers(_bufferFactory, _sommets, _indices);
        InitShaders(_shaderManager);
        InitTexture(_textureManager);
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

    private unsafe void InitShaders(ShaderManager shaderManager)
    {
        InitVertexShader(shaderManager);
        InitPixelShader(shaderManager);
    }

    private unsafe void InitTexture(TextureManager textureManager)
    {
        // Initialisation des paramètres de sampling de la texture
        _shaderDescPool.Get(out ObjectPoolWrapper<SamplerDesc> samplerDescWrapper);
        ref SamplerDesc samplerDesc = ref samplerDescWrapper.Data;
        samplerDesc.Filter = Filter.Anisotropic;
        samplerDesc.AddressU = TextureAddressMode.Wrap;
        samplerDesc.AddressV = TextureAddressMode.Wrap;
        samplerDesc.AddressW = TextureAddressMode.Wrap;
        samplerDesc.MipLODBias = 0f;
        samplerDesc.MaxAnisotropy = 4;
        samplerDesc.ComparisonFunc = ComparisonFunc.Always;
        samplerDesc.MinLOD = 0;
        samplerDesc.MaxLOD = float.MaxValue;
        samplerDesc.BorderColor[0] = 0f;
        samplerDesc.BorderColor[1] = 0f;
        samplerDesc.BorderColor[2] = 0f;
        samplerDesc.BorderColor[3] = 0f;

        // Création de l’état de sampling
        _sampleState = textureManager.Factory.CreateSampler(samplerDesc, $"{_name}_SamplerState");

        _shaderDescPool.Return(samplerDescWrapper);
    }

    private unsafe void InitBuffers<TVertex, TIndice>(GraphicBufferFactory bufferFactory, TVertex[] sommets, TIndice[] indices)
        where TVertex : unmanaged
        where TIndice : unmanaged
    {
        // Create our vertex buffer.
        _vertexBuffer = bufferFactory.CreateVertexBuffer<TVertex>(sommets, Usage.Immutable, CpuAccessFlag.None, $"{_name}_VertexBuffer");

        // Create our index buffer.
        _indexBuffer = bufferFactory.CreateIndexBuffer<TIndice>(indices, Usage.Immutable, CpuAccessFlag.None, $"{_name}_IndexBuffer");

        // Create our constant buffer.
        _constantBuffer = bufferFactory.CreateConstantBuffer<ObjectShadersParams>(Usage.Default, CpuAccessFlag.None, $"{_name}_ConstantBuffer");
    }

    protected void UpdateMatWorld()
    {
        _matWorld = System.Numerics.Matrix4x4.CreateFromQuaternion(_orientation.Quaternion) * System.Numerics.Matrix4x4.CreateTranslation(_position);
    }

    /// <summary>
    /// Compilation et chargement du vertex shader
    /// </summary>
    /// <param name="device"></param>
    /// <param name="compiler"></param>
    private unsafe void InitVertexShader(ShaderManager shaderManager)
    {
        ShaderCodeFile shaderFile = InitVertexShaderCodeFile();
        shaderManager.GetOrLoadVertexShaderAndLayout(shaderFile, Sommet.InputLayoutDesc, ref _vertexShader, ref _vertexLayout);
    }

    /// <summary>
    /// Compilation et chargement du pixel shader
    /// </summary>
    /// <param name="device"></param>
    /// <param name="compiler"></param>
    private unsafe void InitPixelShader(ShaderManager shaderManager)
    {
        ShaderCodeFile shaderFile = InitPixelShaderCodeFile();
        _pixelShader = shaderManager.GetOrLoadPixelShader(shaderFile);
    }

    /// <summary>
    /// VertexShader file
    /// </summary>
    [return: NotNull]
    protected virtual ShaderCodeFile InitVertexShaderCodeFile()
    {
        // Compilation et chargement du vertex shader
        string filePath = "shaders\\MiniPhongNormalMap.hlsl";
        string entryPoint = "MiniPhongNormalMapVS";
        string target = "vs_5_0";
        // #define D3DCOMPILE_ENABLE_STRICTNESS                    (1 << 11)
        uint flagStrictness = ((uint)1 << 11);
        // #define D3DCOMPILE_DEBUG (1 << 0)
        // #define D3DCOMPILE_SKIP_OPTIMIZATION                    (1 << 2)
#if DEBUG
        uint flagDebug = ((uint)1 << 0);
        uint flagSkipOptimization = ((uint)(1 << 2));
#else
        uint flagDebug = 0;
        uint flagSkipOptimization = 0;
#endif
        uint compilationFlags = flagStrictness | flagDebug | flagSkipOptimization;
        return new ShaderCodeFile
        (
            filePath,
            entryPoint,
            target,
            compilationFlags,
            name: "MiniPhongNormalMap_VertexShader"
        );
    }

    /// <summary>
    /// PixelShader file
    /// </summary>
    [return: NotNull]
    protected virtual ShaderCodeFile InitPixelShaderCodeFile()
    {
        string filePath = "shaders\\MiniPhongNormalMap.hlsl";
        string entryPoint = "MiniPhongNormalMapPS";
        string target = "ps_5_0";
        // #define D3DCOMPILE_ENABLE_STRICTNESS                    (1 << 11)
        uint flagStrictness = ((uint)1 << 11);
        // #define D3DCOMPILE_DEBUG (1 << 0)
        // #define D3DCOMPILE_SKIP_OPTIMIZATION                    (1 << 2)
#if DEBUG
        uint flagDebug = ((uint)1 << 0);
        uint flagSkipOptimization = ((uint)(1 << 2));
#else
        uint flagDebug = 0;
        uint flagSkipOptimization = 0;
#endif
        uint compilationFlags = flagStrictness | flagDebug | flagSkipOptimization;
        return new ShaderCodeFile
        (
           filePath,
           entryPoint,
           target,
           compilationFlags,
           name: "MiniPhongNormalMap_PixelShader"
        );
    }

    public struct SubObjet3D
    {
        public IReadOnlyList<ushort> Indices;
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
            _constantBuffer.Dispose();

            _vertexShader.Dispose();
            _vertexLayout.Dispose();
            _pixelShader.Dispose();

            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~BaseObjet3D()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
