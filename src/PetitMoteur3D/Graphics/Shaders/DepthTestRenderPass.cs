using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PetitMoteur3D.Core.Memory;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Shaders;

internal sealed class DepthTestRenderPass : BaseRenderPass, IDisposable
{
    public ComPtr<ID3D11Buffer> VertexShaderConstantBuffer => _vertexShaderConstantBuffer;
    public ComPtr<ID3D11Buffer> PixelShaderConstantBuffer => _pixelShaderConstantBuffer;
    public ComPtr<ID3D11SamplerState> SampleState => _sampleState;

    private ComPtr<ID3D11Buffer> _vertexShaderConstantBuffer;
    private ComPtr<ID3D11Buffer> _pixelShaderConstantBuffer;
    private ComPtr<ID3D11SamplerState> _sampleState;

    private bool _disposedValue;

    public DepthTestRenderPass(D3D11GraphicPipeline graphicPipeline, string name = "")
        : base(graphicPipeline, name)
    {
        _disposedValue = false;
    }

    #region Public methods

    #region Update Values
    public void UpdateVertexShaderConstantBuffer(VertexConstantBufferParams value)
    {
        GraphicPipeline.RessourceFactory.UpdateSubresource(_vertexShaderConstantBuffer, 0, in Unsafe.NullRef<Box>(), in value, 0, 0);
    }

    public void UpdatePixelShaderConstantBuffer(PixelConstantBufferParams value)
    {
        GraphicPipeline.RessourceFactory.UpdateSubresource(_pixelShaderConstantBuffer, 0, in Unsafe.NullRef<Box>(), in value, 0, 0);
    }
    #endregion

    #region Vertex Shader
    public override void SetVertexShaderConstantBuffers()
    {
        GraphicPipeline.VertexShaderStage.SetConstantBuffers(0, 1, ref _vertexShaderConstantBuffer);
    }
    #endregion

    #region Pixel Shader
    public override void SetPixelShaderConstantBuffers()
    {
        GraphicPipeline.PixelShaderStage.SetConstantBuffers(0, 1, ref _pixelShaderConstantBuffer);
    }

    public override void SetPixelShaderRessources() { }

    public override void SetSamplers()
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

    /// <inheritdoc/>
    protected override InputElementDesc[] GetInputLayoutDesc()
    {
        return SommetPosition.InputLayoutDesc;
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

    /// <inheritdoc/>
    [return : NotNull]
    protected override ShaderCodeFile? InitPixelShaderCodeFile()
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
        _sampleState = textureManager.Factory.CreateSampler(samplerDesc, $"{Name}_SamplerState");
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
