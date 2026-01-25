using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PetitMoteur3D.Core.Memory;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Shaders;

internal sealed class ShadowMapRenderPass : BaseRenderPass, IDisposable
{
    private ComPtr<ID3D11Buffer> _vertexShaderConstantBuffer;

    private bool _disposedValue;

    public ShadowMapRenderPass(D3D11GraphicPipeline graphicPipeline, string name = "")
        : base(graphicPipeline, name)
    {
        _disposedValue = false;
    }

    #region Public methods
    #region Update Values
    public void UpdateVertexShaderConstantBuffer(VertexShaderConstantBufferParams value)
    {
        GraphicPipeline.RessourceFactory.UpdateSubresource(_vertexShaderConstantBuffer, 0, in Unsafe.NullRef<Box>(), in value, 0, 0);
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
        GraphicPipeline.PixelShaderStage.SetConstantBuffers(0, 1, ref _vertexShaderConstantBuffer);
    }
    public override void SetPixelShaderRessources() { }

    public override void SetSamplers() { }

    public override void ClearPixelShaderResources() { }
    #endregion
    #endregion

    #region Protected methods
    /// <inheritdoc/>
    protected override void InitBuffers(GraphicBufferFactory bufferFactory)
    {
        // Create our constant buffer.
        _vertexShaderConstantBuffer = bufferFactory.CreateConstantBuffer<VertexShaderConstantBufferParams>(Usage.Default, CpuAccessFlag.None, $"{Name}_VextexShaderConstantBuffer");
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
        string filePath = "shaders\\ShadowMap.hlsl";
        string entryPoint = "ShadowMapVS";
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
            name: "ShadowMap_VertexShader"
        );
    }

    /// <inheritdoc/>
    protected override ShaderCodeFile? InitPixelShaderCodeFile()
    {
        return null;
    }

    /// <inheritdoc/>
    protected override void InitialisationImpl(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory)
    {

    }
    #endregion

    #region Private methods
    #endregion

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct VertexShaderConstantBufferParams : IResetable
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

            base.Dispose(disposing);

            _disposedValue = true;
        }
    }

    ~ShadowMapRenderPass()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }
}
