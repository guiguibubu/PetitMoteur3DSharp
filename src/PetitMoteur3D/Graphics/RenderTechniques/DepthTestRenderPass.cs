using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PetitMoteur3D.Core.Memory;
using PetitMoteur3D.Graphics.Buffers;
using PetitMoteur3D.Graphics.Shaders;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using ShaderType = PetitMoteur3D.Graphics.Shaders.ShaderType;

namespace PetitMoteur3D.Graphics.RenderTechniques;

internal sealed class DepthTestRenderPass : BaseRenderPass, IDisposable
{
    private ConstantBuffer _vertexShaderConstantBuffer;
    private ConstantBuffer _pixelShaderConstantBuffer;
    private ComPtr<ID3D11SamplerState> _sampleState;

    private bool _disposedValue;

    public DepthTestRenderPass(D3D11GraphicPipeline graphicPipeline, RenderTarget renderTarget, string name = "")
        : base(graphicPipeline, renderTarget, name)
    {
        _disposedValue = false;
    }

    #region Public methods

    #region Update Values
    public void UpdateVertexShaderConstantBuffer(VertexConstantBufferParams value)
    {
        _vertexShaderConstantBuffer.Buffer.UpdateSubresource(GraphicPipeline.DeviceContext, 0, in Unsafe.NullRef<Box>(), in value, 0, 0);
    }

    public void UpdatePixelShaderConstantBuffer(PixelConstantBufferParams value)
    {
        _pixelShaderConstantBuffer.Buffer.UpdateSubresource(GraphicPipeline.DeviceContext, 0, in Unsafe.NullRef<Box>(), in value, 0, 0);
    }
    #endregion

    #region Vertex Shader
    public override void BindVertexShaderConstantBuffers()
    {
        _vertexShaderConstantBuffer.Bind(GraphicPipeline, ShaderType.VertexShader, idSlot: 0);
    }
    #endregion

    #region Pixel Shader
    public override void BindPixelShaderConstantBuffers()
    {
        _pixelShaderConstantBuffer.Bind(GraphicPipeline, ShaderType.PixelShader, idSlot: 0);
    }

    public override void BindPixelShaderRessources() { }

    public override void BindSamplers()
    {
        GraphicPipeline.PixelShaderStage.SetSamplers(0, 1, in _sampleState);
    }

    public override void ClearPixelShaderResources() { }
    #endregion
    #endregion

    #region Protected methods
    /// <inheritdoc/>
    protected override unsafe void InitBuffers(GraphicBufferFactory bufferFactory)
    {
        // Create our constant buffer.
        _vertexShaderConstantBuffer = bufferFactory.CreateConstantBuffer<VertexConstantBufferParams>(Usage.Default, CpuAccessFlag.None, $"{Name}_VertexConstantBuffer");

        // Create our constant buffer.
        _pixelShaderConstantBuffer = bufferFactory.CreateConstantBuffer<PixelConstantBufferParams>(Usage.Default, CpuAccessFlag.None, $"{Name}_PixelConstantBuffer");
    }

    protected override void UpdateVertexBuffer(BaseObjet3D baseObjet3D)
    {
        UpdateVertexBuffer(baseObjet3D.VertexBuffer);
    }

    protected override void UpdatePerMeshRessourcesBuffers(Mesh mesh)
    {
        SceneViewContext sceneContext = RenderArgs.SceneContext;
        Matrix4x4 matViewProj = sceneContext.MatViewProj;
        Matrix4x4 matWorld = RenderArgs.ObjectContext.MatWorld;
        UpdateVertexShaderConstantBuffer(new DepthTestRenderPass.VertexConstantBufferParams()
        {
            matWorldViewProj = Matrix4x4.Transpose(RenderArgs.ObjectContext.AdditionalTransformation * matWorld * matViewProj)
        });

        UpdatePixelShaderConstantBuffer(new DepthTestRenderPass.PixelConstantBufferParams()
        {
            successColor = new Vector4(0, 255, 0, 1),
            failColor = new Vector4(255, 0, 0, 1)
        });
    }

    /// <inheritdoc/>
    protected override InputLayoutDesc GetInputLayoutDesc()
    {
        return SommetPosition.InputLayoutDesc;
    }

    /// <inheritdoc/>
    protected override ComPtr<ID3D11RasterizerState> GetRasterizerState()
    {
        return GraphicPipeline.SolidCullBackRS;
    }

    /// <inheritdoc/>
    protected override ComPtr<ID3D11DepthStencilState> GetDepthStencilState()
    {
        return GraphicPipeline.DefaultDSS;
    }

    /// <inheritdoc/>
    [return: NotNull]
    protected override ShaderCodeFile InitVertexShaderCodeFile()
    {
        // Compilation et chargement du vertex shader
        string filePath = "shaders\\DepthTest_VS.hlsl";
        string entryPoint = "DepthTestVS";
        string target = "vs_5_0";
        // #define D3DCOMPILE_ENABLE_STRICTNESS                    (1 << 11)
        uint flagStrictness = (uint)1 << 11;
        // #define D3DCOMPILE_DEBUG (1 << 0)
        // #define D3DCOMPILE_SKIP_OPTIMIZATION                    (1 << 2)
#if DEBUG
        uint flagDebug = (uint)1 << 0;
        uint flagSkipOptimization = 1 << 2;
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

    /// <inheritdoc/>
    [return : NotNull]
    protected override ShaderCodeFile? InitPixelShaderCodeFile()
    {
        string filePath = "shaders\\DepthTest_PS.hlsl";
        string entryPoint = "DepthTestPS";
        string target = "ps_5_0";
        // #define D3DCOMPILE_ENABLE_STRICTNESS                    (1 << 11)
        uint flagStrictness = (uint)1 << 11;
        // #define D3DCOMPILE_DEBUG (1 << 0)
        // #define D3DCOMPILE_SKIP_OPTIMIZATION                    (1 << 2)
#if DEBUG
        uint flagDebug = (uint)1 << 0;
        uint flagSkipOptimization = 1 << 2;
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

    /// <inheritdoc/>
    protected override void InitialisationImpl(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory)
    {
        InitTextureSampler(graphicDeviceRessourceFactory.TextureManager);
    }
    #endregion

    #region Private methods

    private unsafe void InitTextureSampler(TextureManager textureManager)
    {
        // Initialisation des paramètres de sampling de la texture
        SamplerDesc samplerDesc = new()
        {
            Filter = Filter.Anisotropic,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap,
            MipLODBias = 0f,
            MaxAnisotropy = 4,
            ComparisonFunc = ComparisonFunc.Always,
            MinLOD = 0,
            MaxLOD = float.MaxValue,
        };
        samplerDesc.BorderColor[0] = 0f;
        samplerDesc.BorderColor[1] = 0f;
        samplerDesc.BorderColor[2] = 0f;
        samplerDesc.BorderColor[3] = 0f;

        // Création de l’état de sampling
        _sampleState = textureManager.Factory.CreateSampler(samplerDesc);
    }
    #endregion

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct VertexConstantBufferParams : IResetable
    {
        /// <summary>
        /// la matrice totale
        /// </summary>
        public Matrix4x4 matWorldViewProj;

        public void Reset()
        {
            MemoryHelper.ResetMemory(this);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct PixelConstantBufferParams : IResetable
    {
        /// <summary>
        /// la valeur ambiante du matériau en cas de succes
        /// </summary>
        public Vector4 successColor;
        /// <summary>
        /// la valeur ambiante du matériau en cas d'echec
        /// </summary>
        public Vector4 failColor;

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
            _vertexShaderConstantBuffer.Dispose();
            _pixelShaderConstantBuffer.Dispose();
            _sampleState.Dispose();

            base.Dispose(disposing);

            _disposedValue = true;
        }
    }

    ~DepthTestRenderPass()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }
}
