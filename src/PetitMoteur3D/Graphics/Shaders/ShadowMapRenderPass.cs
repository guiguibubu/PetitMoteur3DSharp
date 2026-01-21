using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PetitMoteur3D.Core.Memory;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Shaders;

internal sealed class ShadowMapRenderPass : IDisposable
{
    public string Name { get; init; }

    private ComPtr<ID3D11Buffer> _vertexShaderConstantBuffer;
    private ComPtr<ID3D11VertexShader> _vertexShader;
    private ComPtr<ID3D11InputLayout> _vertexLayout;

    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private uint _vertexStride;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private Silk.NET.DXGI.Format _format;
    private D3DPrimitiveTopology _topology;

    private readonly D3D11GraphicPipeline _graphicPipeline;

    private bool _disposedValue;

    public ShadowMapRenderPass(D3D11GraphicPipeline graphicPipeline, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            Name = this.GetType().Name + "_" + Guid.NewGuid().ToString();
        }
        else
        {
            Name = name;
        }

        _graphicPipeline = graphicPipeline;
        Initialisation(graphicPipeline.GraphicDevice.RessourceFactory);

        _disposedValue = false;
    }

    #region Public methods

    public void UpdatePrimitiveTopology(D3DPrimitiveTopology topology)
    {
        _topology = topology;
    }

    public void SetPrimitiveTopology()
    {
        _graphicPipeline.InputAssemblerStage.SetPrimitiveTopology(_topology);
    }

    public void UpdateVertexBuffer(ComPtr<ID3D11Buffer> vertexBuffer, uint vertexStride)
    {
        _vertexBuffer = vertexBuffer;
        _vertexStride = vertexStride;
    }

    public void UpdateIndexBuffer(ComPtr<ID3D11Buffer> indexBuffer, Silk.NET.DXGI.Format format)
    {
        _indexBuffer = indexBuffer;
        _format = format;
    }

    public void SetVertexBuffer(uint offset = 0)
    {
        _graphicPipeline.InputAssemblerStage.SetVertexBuffers(0, 1, ref _vertexBuffer, in _vertexStride, in offset);
    }

    public void SetIndexBuffer(uint offset = 0)
    {
        _graphicPipeline.InputAssemblerStage.SetIndexBuffer(_indexBuffer, _format, offset);
    }

    public void SetInputLayout()
    {
        _graphicPipeline.InputAssemblerStage.SetInputLayout(_vertexLayout);
    }

    public void UpdateVertexShaderConstantBuffer(VertexShaderConstantBufferParams value)
    {
        _graphicPipeline.RessourceFactory.UpdateSubresource(_vertexShaderConstantBuffer, 0, in Unsafe.NullRef<Box>(), in value, 0, 0);
    }

    public void SetVertexShader()
    {
        _graphicPipeline.VertexShaderStage.SetShader(_vertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
    }

    public void SetVertexShaderConstantBuffers()
    {
        _graphicPipeline.VertexShaderStage.SetConstantBuffers(0, 1, ref _vertexShaderConstantBuffer);
    }

    public void SetGeometryShader()
    {
        _graphicPipeline.GeometryShaderStage.SetShader((ComPtr<ID3D11GeometryShader>)null, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
    }

    public void SetPixelShader()
    {
        _graphicPipeline.PixelShaderStage.SetShader((ComPtr<ID3D11PixelShader>)null, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
    }

    public void SetPixelShaderConstantBuffers()
    {
        _graphicPipeline.PixelShaderStage.SetConstantBuffers(0, 1, ref _vertexShaderConstantBuffer);
    }

    public void DrawIndexed(uint indexCount, uint startIndexLocation, int baseVertexLocation)
    {
        _graphicPipeline.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
    }
    #endregion

    #region Private methods

    private void Initialisation(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory)
    {
        InitBuffers(graphicDeviceRessourceFactory.BufferFactory);
        InitShaders(graphicDeviceRessourceFactory.ShaderManager);
    }

    private unsafe void InitBuffers(GraphicBufferFactory bufferFactory)
    {
        // Create our constant buffer.
        _vertexShaderConstantBuffer = bufferFactory.CreateConstantBuffer<VertexShaderConstantBufferParams>(Usage.Default, CpuAccessFlag.None, $"{Name}_VextexShaderConstantBuffer");
    }

    private unsafe void InitShaders(ShaderManager shaderManager)
    {
        InitVertexShader(shaderManager);
    }

    /// <summary>
    /// Compilation et chargement du vertex shader
    /// </summary>
    /// <param name="device"></param>
    /// <param name="compiler"></param>
    private unsafe void InitVertexShader(ShaderManager shaderManager)
    {
        ShaderCodeFile shaderFile = InitVertexShaderCodeFile();
        shaderManager.GetOrLoadVertexShaderAndLayout(shaderFile, SommetPosition.InputLayoutDesc, ref _vertexShader, ref _vertexLayout);
    }

    /// <summary>
    /// VertexShader file
    /// </summary>
    [return: NotNull]
    private static ShaderCodeFile InitVertexShaderCodeFile()
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
            name: "MiniPhong_VertexShader"
        );
    }

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

    private void Dispose(bool disposing)
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
            _vertexShader.Dispose();
            _vertexLayout.Dispose();

            _disposedValue = true;
        }
    }

    ~ShadowMapRenderPass()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
