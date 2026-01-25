using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Shaders;

internal abstract class BaseRenderPass : IRenderPass, IDisposable
{
    public string Name { get; init; }

    public ComPtr<ID3D11InputLayout> VertexLayout => _vertexLayout;
    public ComPtr<ID3D11VertexShader> VertexShader => _vertexShader;
    public ComPtr<ID3D11GeometryShader> GeometryShader => _geometryShader;
    public ComPtr<ID3D11PixelShader> PixelShader => _pixelShader;

    protected D3D11GraphicPipeline GraphicPipeline => _graphicPipeline;

    private ComPtr<ID3D11InputLayout> _vertexLayout;
    private ComPtr<ID3D11VertexShader> _vertexShader;
    private ComPtr<ID3D11GeometryShader> _geometryShader;
    private ComPtr<ID3D11PixelShader> _pixelShader;

    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private uint _vertexStride;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private Silk.NET.DXGI.Format _format;
    private D3DPrimitiveTopology _topology;

    private readonly D3D11GraphicPipeline _graphicPipeline;

    private bool _disposedValue;

    public BaseRenderPass(D3D11GraphicPipeline graphicPipeline, string name = "")
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
    #region Update Values
    public void UpdatePrimitiveTopology(D3DPrimitiveTopology topology)
    {
        _topology = topology;
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
    #endregion

    #region Input Assembler
    public void SetPrimitiveTopology()
    {
        _graphicPipeline.InputAssemblerStage.SetPrimitiveTopology(_topology);
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
    #endregion

    #region Vertex Shader
    public void SetVertexShader()
    {
        _graphicPipeline.VertexShaderStage.SetShader(_vertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
    }

    public abstract void SetVertexShaderConstantBuffers();
    #endregion

    #region Geometry Shader
    public void SetGeometryShader()
    {
        _graphicPipeline.GeometryShaderStage.SetShader((ComPtr<ID3D11GeometryShader>)null, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
    }
    #endregion

    #region Pixel Shader
    public void SetPixelShader()
    {
        _graphicPipeline.PixelShaderStage.SetShader(_pixelShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
    }

    public abstract void SetPixelShaderConstantBuffers();

    public abstract void SetPixelShaderRessources();

    public abstract void SetSamplers();

    public abstract void ClearPixelShaderResources();
    #endregion

    public void DrawIndexed(uint indexCount, uint startIndexLocation, int baseVertexLocation)
    {
        _graphicPipeline.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
    }
    #endregion

    #region Protected methods
    /// <summary>
    /// Init constant buffers
    /// </summary>
    /// <param name="bufferFactory"></param>
    protected abstract void InitBuffers(GraphicBufferFactory bufferFactory);

    /// <summary>
    /// Get vertex input layout description
    /// </summary>
    /// <returns></returns>
    protected abstract InputElementDesc[] GetInputLayoutDesc();

    /// <summary>
    /// VertexShader file
    /// </summary>
    [return: NotNull]
    protected abstract ShaderCodeFile InitVertexShaderCodeFile();

    /// <summary>
    /// PixelShader file
    /// </summary>
    protected abstract ShaderCodeFile? InitPixelShaderCodeFile();

    /// <summary>
    /// Init Impl
    /// </summary>
    protected abstract void InitialisationImpl(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory);
    #endregion

    #region Private methods

    private void Initialisation(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory)
    {
        InitBuffers(graphicDeviceRessourceFactory.BufferFactory);
        InitShaders(graphicDeviceRessourceFactory.ShaderManager);
        InitialisationImpl(graphicDeviceRessourceFactory);
    }

    private unsafe void InitShaders(ShaderManager shaderManager)
    {
        InitVertexShader(shaderManager);
        InitGeometryShader(shaderManager);
        InitPixelShader(shaderManager);
    }

    /// <summary>
    /// Compilation et chargement du vertex shader
    /// </summary>
    /// <param name="device"></param>
    /// <param name="compiler"></param>
    private unsafe void InitVertexShader(ShaderManager shaderManager)
    {
        ShaderCodeFile shaderFile = InitVertexShaderCodeFile();
        shaderManager.GetOrLoadVertexShaderAndLayout(shaderFile, GetInputLayoutDesc(), ref _vertexShader, ref _vertexLayout);
    }

    /// <summary>
    /// Compilation et chargement du geometry shader
    /// </summary>
    /// <param name="device"></param>
    /// <param name="compiler"></param>
    private unsafe void InitGeometryShader(ShaderManager shaderManager)
    {
        _geometryShader = (ComPtr<ID3D11GeometryShader>)null;
    }

    /// <summary>
    /// Compilation et chargement du pixel shader
    /// </summary>
    /// <param name="device"></param>
    /// <param name="compiler"></param>
    private unsafe void InitPixelShader(ShaderManager shaderManager)
    {
        ShaderCodeFile? shaderFile = InitPixelShaderCodeFile();
        if (shaderFile is null)
        {
            _pixelShader = (ComPtr<ID3D11PixelShader>)null;
        }
        else
        {
            _pixelShader = shaderManager.GetOrLoadPixelShader(shaderFile);
        }
    }

    #endregion

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _vertexShader.Dispose();
            _vertexLayout.Dispose();
            _pixelShader.Dispose();

            _disposedValue = true;
        }
    }

    ~BaseRenderPass()
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
