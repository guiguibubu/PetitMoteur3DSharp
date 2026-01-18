using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PetitMoteur3D.Core.Memory;
using PetitMoteur3D.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D;

internal abstract class BaseObjet3DWithShadow : BaseObjet3D, IShadowDrawableObjet, IDisposable
{
    private ComPtr<ID3D11Buffer> _vertexBufferShadowMap;
    private ComPtr<ID3D11Buffer> _constantBufferShadowMap;
    private SommetShadowMap[] _sommetsShadowMap;
    private unsafe readonly uint _vertexStrideSadowMap = (uint)sizeof(SommetShadowMap);
    private static readonly uint _vertexOffsetShadowMap = 0;

    private static readonly IObjectPool<ObjectShadowMapShadersParams> _objectShadersParamsPool = ObjectPoolFactory.Create<ObjectShadowMapShadersParams>();
    private bool _disposed;

    protected BaseObjet3DWithShadow(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, string name = "")
        : base(graphicDeviceRessourceFactory, name)
    {
        _sommetsShadowMap = Array.Empty<SommetShadowMap>();

        _vertexBufferShadowMap = default;
        _constantBufferShadowMap = default;

        _disposed = false;
    }

    /// <inheritdoc/>
    public unsafe void Draw(D3D11GraphicPipeline graphicPipeline, ref readonly System.Numerics.Matrix4x4 matViewProj, ref readonly System.Numerics.Matrix4x4 matViewProjLight)
    {
        System.Numerics.Matrix4x4 matViewProjLightCopy = matViewProjLight;
        Action<D3D11GraphicPipeline, SubObjet3D> additionalConf = (graphicPipeline, subObject) =>
        {
            SetUpShadowConstants(graphicPipeline, in matViewProjLightCopy, subObject);
        };
        base.AdditionalDrawConfig += additionalConf;
        base.Draw(graphicPipeline, in matViewProj);
        base.AdditionalDrawConfig -= additionalConf;
    }

    /// <inheritdoc/>
    public unsafe void DrawShadow(D3D11GraphicPipeline graphicPipeline, ref readonly System.Numerics.Matrix4x4 matViewProjLight)
    {
        // Choisir la topologie des primitives
        graphicPipeline.InputAssemblerStage.SetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
        // Source des sommets
        graphicPipeline.InputAssemblerStage.SetVertexBuffers(0, 1, ref _vertexBufferShadowMap, in _vertexStrideSadowMap, in _vertexOffsetShadowMap);
        // Source des index
        graphicPipeline.InputAssemblerStage.SetIndexBuffer(IndexBuffer, Silk.NET.DXGI.Format.FormatR16Uint, 0);
        ref readonly SubObjet3D[] subObjects = ref base.SubObjects;
        foreach (SubObjet3D subObjet3D in subObjects)
        {
            // Initialiser et sélectionner les « constantes » des shaders
            _objectShadersParamsPool.Get(out ObjectPoolWrapper<ObjectShadowMapShadersParams> shadersParamsWrapper);
            ref ObjectShadowMapShadersParams shadersParams = ref shadersParamsWrapper.Data;
            ref readonly System.Numerics.Matrix4x4 matWorld = ref base.MatWorld;
            shadersParams.matWorldViewProjLight = System.Numerics.Matrix4x4.Transpose(subObjet3D.Transformation * matWorld * matViewProjLight);

            graphicPipeline.RessourceFactory.UpdateSubresource(_constantBufferShadowMap, 0, in Unsafe.NullRef<Box>(), in shadersParams, 0, 0);

            // Activer le VS
            graphicPipeline.VertexShaderStage.SetConstantBuffers(0, 1, ref _constantBufferShadowMap);
            // Activer le GS
            graphicPipeline.GeometryShaderStage.SetShader((ComPtr<ID3D11GeometryShader>)null, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            // Activer le PS
            graphicPipeline.PixelShaderStage.SetShader((ComPtr<ID3D11PixelShader>)null, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            // **** Rendu de l’objet
            graphicPipeline.DrawIndexed((uint)NbIndices, 0, 0);

            _objectShadersParamsPool.Return(shadersParamsWrapper);
        }
    }

    protected override void Initialisation()
    {
        base.Initialisation();
        _sommetsShadowMap = InitVertexShadowMap();

        InitBuffersShadowMap(BufferFactory, _sommetsShadowMap);
    }

    /// <summary>
    /// Initialise les vertex pour la shhadow map
    /// </summary>
    /// <returns></returns>
    protected abstract SommetShadowMap[] InitVertexShadowMap();

    private unsafe void InitBuffersShadowMap<TVertex>(GraphicBufferFactory bufferFactory, TVertex[] sommets)
        where TVertex : unmanaged
    {
        // Create our vertex buffer.
        _vertexBufferShadowMap = bufferFactory.CreateVertexBuffer<TVertex>(sommets, Usage.Immutable, CpuAccessFlag.None, $"{Name}_VertexBufferShadowMap");

        // Create our constant buffer.
        _constantBufferShadowMap = bufferFactory.CreateConstantBuffer<ObjectShadowMapShadersParams>(Usage.Default, CpuAccessFlag.None, $"{Name}_ConstantBufferShadowMap");
    }

    /// <summary>
    /// VertexShader file
    /// </summary>
    [return: NotNull]
    protected override ShaderCodeFile InitVertexShaderCodeFile()
    {
        string filePath = "shaders\\DepthTest_VS.hlsl";
        string entryPoint = "DepthTestVS";
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
           name: "DepthTest_VertexShader"
        );
    }

    /// <summary>
    /// PixelShader file
    /// </summary>
    [return: NotNull]
    protected override ShaderCodeFile InitPixelShaderCodeFile()
    {
        string filePath = "shaders\\DepthTest_PS.hlsl";
        string entryPoint = "DepthTestPS";
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
           name: "DepthTest_PixelShader"
        );
    }

    private void SetUpShadowConstants(D3D11GraphicPipeline graphicPipeline, ref readonly System.Numerics.Matrix4x4 matViewProjLight, SubObjet3D subObjet3D)
    {
        // Initialiser et sélectionner les « constantes » des shaders
        _objectShadersParamsPool.Get(out ObjectPoolWrapper<ObjectShadowMapShadersParams> shadersParamsWrapper);
        ref ObjectShadowMapShadersParams shadersParams = ref shadersParamsWrapper.Data;
        ref readonly System.Numerics.Matrix4x4 matWorld = ref base.MatWorld;
        shadersParams.matWorldViewProjLight = System.Numerics.Matrix4x4.Transpose(subObjet3D.Transformation * matWorld * matViewProjLight);

        graphicPipeline.RessourceFactory.UpdateSubresource(_constantBufferShadowMap, 0, in Unsafe.NullRef<Box>(), in shadersParams, 0, 0);

        // Activer le VS
        graphicPipeline.VertexShaderStage.SetConstantBuffers(3, 1, ref _constantBufferShadowMap);
        graphicPipeline.PixelShaderStage.SetConstantBuffers(3, 1, ref _constantBufferShadowMap);
    }

    ~BaseObjet3DWithShadow()
    {
        Dispose(disposing: false);
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null

            _vertexBufferShadowMap.Dispose();
            _constantBufferShadowMap.Dispose();

            _disposed = true;
        }

        // Call the base class implementation.
        base.Dispose(disposing);
    }
}
