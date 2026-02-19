using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PetitMoteur3D.Core.Memory;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Shaders;

internal sealed class DeferredLightningRenderPass : BaseRenderPass, IDisposable
{
    private ComPtr<ID3D11Buffer> _sceneConstantBuffer;
    private ComPtr<ID3D11Buffer> _pixelObjectConstantBuffer;
    private ComPtr<ID3D11Buffer> _vertexObjectConstantBuffer;

    private bool _disposedValue;

    public DeferredLightningRenderPass(D3D11GraphicPipeline graphicPipeline, string name = "")
        : base(graphicPipeline, name)
    {
        _disposedValue = false;
    }

    #region Public methods
    #region Update Values
    public void UpdateSceneConstantBuffer(SceneConstantBufferParams value)
    {
        GraphicPipeline.RessourceFactory.UpdateSubresource(_sceneConstantBuffer, 0, in Unsafe.NullRef<Box>(), in value, 0, 0);
    }

    public void UpdateVertexObjectConstantBuffer(VertexObjectConstantBufferParams value)
    {
        GraphicPipeline.RessourceFactory.UpdateSubresource(_vertexObjectConstantBuffer, 0, in Unsafe.NullRef<Box>(), in value, 0, 0);
    }
    #endregion

    #region Vertex Shader
    public override void SetVertexShaderConstantBuffers()
    {
        GraphicPipeline.VertexShaderStage.SetConstantBuffers(0, 1, ref _vertexObjectConstantBuffer);
    }
    #endregion

    #region Pixel Shader
    public override void SetPixelShaderConstantBuffers()
    {
        GraphicPipeline.PixelShaderStage.SetConstantBuffers(0, 1, ref _sceneConstantBuffer);
    }

    public override unsafe void SetPixelShaderRessources()
    {
        ComPtr<ID3D11ShaderResourceView> textureDiffuse = GraphicPipeline.GeometryBufferDiffuseSRV;
        ComPtr<ID3D11ShaderResourceView> textureSpecular = GraphicPipeline.GeometryBufferSpecularSRV;
        ComPtr<ID3D11ShaderResourceView> textureNormal = GraphicPipeline.GeometryBufferNormalSRV;

        // Activation de la texture
        if (textureDiffuse.Handle is not null)
        {
            GraphicPipeline.PixelShaderStage.SetShaderResources(0, 1, ref textureDiffuse);
        }
        else
        {
            GraphicPipeline.PixelShaderStage.ClearShaderResources(0);
        }

        if (textureSpecular.Handle is not null)
        {
            GraphicPipeline.PixelShaderStage.SetShaderResources(1, 1, ref textureSpecular);
        }
        else
        {
            GraphicPipeline.PixelShaderStage.ClearShaderResources(1);
        }

        if (textureNormal.Handle is not null)
        {
            GraphicPipeline.PixelShaderStage.SetShaderResources(2, 1, ref textureNormal);
        }
        else
        {
            GraphicPipeline.PixelShaderStage.ClearShaderResources(2);
        }
    }

    public override void SetSamplers()
    {
    }

    public override void ClearPixelShaderResources()
    {
        GraphicPipeline.PixelShaderStage.ClearShaderResources(0);
        GraphicPipeline.PixelShaderStage.ClearShaderResources(1);
        GraphicPipeline.PixelShaderStage.ClearShaderResources(2);
    }
    #endregion
    #endregion

    #region Protected methods
    /// <inheritdoc/>
    protected sealed override void InitBuffers(GraphicBufferFactory bufferFactory)
    {
        // Create our constant buffer.
        _sceneConstantBuffer = bufferFactory.CreateConstantBuffer<SceneConstantBufferParams>(Usage.Default, CpuAccessFlag.None, $"{Name}_SceneConstantBuffer");

        // Create our constant buffer.
        _vertexObjectConstantBuffer = bufferFactory.CreateConstantBuffer<VertexObjectConstantBufferParams>(Usage.Default, CpuAccessFlag.None, $"{Name}_VertexObjectConstantBuffer");
    }

    /// <inheritdoc/>
    protected sealed override InputElementDesc[] GetInputLayoutDesc()
    {
        return Sommet.InputLayoutDesc;
    }

    /// <inheritdoc/>
    [return: NotNull]
    protected override ShaderCodeFile InitVertexShaderCodeFile()
    {
        // Compilation et chargement du vertex shader
        string filePath = "shaders\\DeferredShadingLightningPass_VS.hlsl";
        string entryPoint = "DeferredShadingLightningPassVS";
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
            name: "DeferredShadingLightningPass_VertexShader"
        );
    }

    /// <inheritdoc/>
    [return: NotNull]
    protected override ShaderCodeFile? InitPixelShaderCodeFile()
    {
        string filePath = "shaders\\DeferredShadingLightningPass_PS.hlsl";
        string entryPoint = "DeferredShadingLightningPassPS";
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
           name: "DeferredShadingLightningPass_PixelShader"
        );
    }

    /// <inheritdoc/>
    protected sealed override void InitialisationImpl(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory)
    {
    }
    #endregion

    #region Private methods
    #endregion

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct LightParams
    {
        /// <summary>
        /// la position de la source d’éclairage (Source Point)
        /// </summary>
        public Vector4 Position;
        /// <summary>
        /// la direction de la source d’éclairage (Source Directionnelle)
        /// </summary>
        public Vector4 Direction;
        /// <summary>
        /// la valeur ambiante de l’éclairage
        /// </summary>
        public Vector4 AmbiantColor;
        /// <summary>
        /// la valeur diffuse de l’éclairage
        /// </summary>
        public Vector4 DiffuseColor;
        /// <summary>
        /// Indique la lumiere est active
        /// </summary>
        public int Enable;
        private readonly uint alignement1_1;
        private readonly uint alignement1_2;
        private readonly uint alignement1_3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct SceneConstantBufferParams : IResetable
    {
        /// <summary>
        /// les infos de la lumiere
        /// </summary>
        public LightParams LightParams;
        /// <summary>
        /// la position de la caméra
        /// </summary>
        public Vector4 CameraPos;

        public void Reset()
        {
            MemoryHelper.ResetMemory(this);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct VertexObjectConstantBufferParams : IResetable
    {
        /// <summary>
        /// la matrice totale
        /// </summary>
        public Matrix4x4 matWorldViewProj;

        /// <summary>
        /// matrice de transformation dans le monde
        /// </summary>
        public Matrix4x4 matWorld;

        public void Reset()
        {
            MemoryHelper.ResetMemory(this);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _sceneConstantBuffer.Dispose();
            _pixelObjectConstantBuffer.Dispose();
            _vertexObjectConstantBuffer.Dispose();

            base.Dispose(disposing);

            _disposedValue = true;
        }
    }

    ~DeferredLightningRenderPass()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }
}
