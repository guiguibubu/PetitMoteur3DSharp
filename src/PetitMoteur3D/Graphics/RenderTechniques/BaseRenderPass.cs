using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PetitMoteur3D.Graphics.Buffers;
using PetitMoteur3D.Graphics.Shaders;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.RenderTechniques;

internal abstract class BaseRenderPass : IRenderPass, IDisposable
{
    public string Name { get; init; }

    public VertexShader VertexShader => _vertexShader;
    public ComPtr<ID3D11GeometryShader> GeometryShader => _geometryShader;
    public PixelShader? PixelShader => _pixelShader;

    protected D3D11GraphicPipeline GraphicPipeline => _graphicPipeline;

    private VertexShader _vertexShader;
    private ComPtr<ID3D11GeometryShader> _geometryShader;
    private PixelShader? _pixelShader;

    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private D3DPrimitiveTopology _topology;

    private readonly D3D11GraphicPipeline _graphicPipeline;

    private bool _disposedValue;

    public BaseRenderPass(D3D11GraphicPipeline graphicPipeline, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            Name = GetType().Name + "_" + Guid.NewGuid().ToString();
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

    public void UpdateVertexBuffer(VertexBuffer vertexBuffer)
    {
        _vertexBuffer = vertexBuffer;
    }

    public void UpdateIndexBuffer(IndexBuffer indexBuffer)
    {
        _indexBuffer = indexBuffer;
    }
    #endregion

    #region Input Assembler
    public void SetPrimitiveTopology()
    {
        _graphicPipeline.InputAssemblerStage.SetPrimitiveTopology(_topology);
    }

    public void BindVertexBuffer()
    {
        _vertexBuffer.Bind(_graphicPipeline, idSlot: 0);
    }

    public void SetIndexBuffer()
    {
        _indexBuffer.Bind(_graphicPipeline);
    }

    public void SetInputLayout()
    {
        _vertexShader.BindLayout(_graphicPipeline);
    }
    #endregion

    #region Vertex Shader
    public void SetVertexShader()
    {
        _vertexShader.Bind(_graphicPipeline);
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
        _pixelShader?.Bind(_graphicPipeline);
    }

    public abstract void SetPixelShaderConstantBuffers();

    public abstract void SetPixelShaderRessources();

    public abstract void SetSamplers();

    public abstract void ClearPixelShaderResources();
    #endregion

    public void Bind()
    {
        SetPrimitiveTopology();
        BindVertexBuffer();
        SetIndexBuffer();
        SetInputLayout();
        SetVertexShader();
        SetVertexShaderConstantBuffers();
        SetGeometryShader();
        SetPixelShader();
        SetPixelShaderConstantBuffers();
        SetPixelShaderRessources();
        SetSamplers();
    }

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
    protected abstract InputLayoutDesc GetInputLayoutDesc();

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
        InitShaders(graphicDeviceRessourceFactory.ShaderFactory);
        InitialisationImpl(graphicDeviceRessourceFactory);
    }

    private unsafe void InitShaders(ShaderFactory shaderFactory)
    {
        InitVertexShader(shaderFactory);
        InitGeometryShader(shaderFactory);
        InitPixelShader(shaderFactory);
    }

    /// <summary>
    /// Compilation et chargement du vertex shader
    /// </summary>
    /// <param name="device"></param>
    /// <param name="compiler"></param>
    private unsafe void InitVertexShader(ShaderFactory shaderFactory)
    {
        ShaderCodeFile shaderFile = InitVertexShaderCodeFile();
        _vertexShader = shaderFactory.CreateVertexShader(shaderFile, GetInputLayoutDesc());
    }

    /// <summary>
    /// Compilation et chargement du geometry shader
    /// </summary>
    /// <param name="device"></param>
    /// <param name="compiler"></param>
    private unsafe void InitGeometryShader(ShaderFactory shaderFactory)
    {
        _geometryShader = (ComPtr<ID3D11GeometryShader>)null;
    }

    /// <summary>
    /// Compilation et chargement du pixel shader
    /// </summary>
    /// <param name="device"></param>
    /// <param name="compiler"></param>
    private unsafe void InitPixelShader(ShaderFactory shaderFactory)
    {
        ShaderCodeFile? shaderFile = InitPixelShaderCodeFile();
        if (shaderFile is null)
        {
            _pixelShader = null;
        }
        else
        {
            _pixelShader = shaderFactory.CreatePixelShader(shaderFile);
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
            _pixelShader?.Dispose();

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
