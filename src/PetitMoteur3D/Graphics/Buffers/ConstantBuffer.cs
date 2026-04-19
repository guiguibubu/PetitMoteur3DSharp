using System;
using System.Collections.Generic;
using PetitMoteur3D.Graphics.Shaders;
using PetitMoteur3D.Logging;

namespace PetitMoteur3D.Graphics.Buffers;

internal sealed class ConstantBuffer : IDisposable
{
    public GraphicBuffer Buffer  => _buffer;

    private readonly GraphicBuffer _buffer;
    private Dictionary<ShaderType, uint?> _bindingSlotByShader;

    private bool _disposedValue;

    public ConstantBuffer(GraphicBuffer buffer)
    {
        _buffer = buffer;
        _bindingSlotByShader = new Dictionary<ShaderType, uint?>();

        _disposedValue = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>No-op if bound already bound</remarks>
    /// <param name="graphicPipeline"></param>
    /// <param name="shaderType"></param>
    /// <param name="idSlot"></param>
    /// <returns></returns>
    public bool Bind(D3D11GraphicPipeline graphicPipeline, ShaderType shaderType, uint idSlot)
    {
        switch (shaderType)
        {
            case ShaderType.VertexShader:
                graphicPipeline.VertexShaderStage.SetConstantBuffers(idSlot, 1, ref _buffer.DataRef);
                break;
            case ShaderType.PixelShader:
                graphicPipeline.PixelShaderStage.SetConstantBuffers(idSlot, 1, ref _buffer.DataRef);
                break;
            default:
                Log.Warning("[ConstantBuffer] Bind  -  Only support Vertex and Pixel Shader");
                return false;
        }
        if (_bindingSlotByShader.ContainsKey(shaderType))
        {
            _bindingSlotByShader[shaderType] = idSlot;
        }
        else
        {
            _bindingSlotByShader.Add(shaderType, idSlot);
        }
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>No-op if not bound</remarks>
    /// <param name="graphicPipeline"></param>
    /// <param name="idSlot"></param>
    public unsafe void UnBind(D3D11GraphicPipeline graphicPipeline, ShaderType shaderType)
    {
        bool shaderTypeInCache = _bindingSlotByShader.TryGetValue(shaderType, out uint? idSlotCache);
        if (!shaderTypeInCache || (idSlotCache is null))
        {
            return;
        }
        switch (shaderType)
        {
            case ShaderType.VertexShader:
                graphicPipeline.VertexShaderStage.UnbindConstantBuffers(idSlotCache.Value, 1);
                break;
            case ShaderType.PixelShader:
                graphicPipeline.PixelShaderStage.UnbindConstantBuffers(idSlotCache.Value, 1);
                break;
            default:
                Log.Warning("[ConstantBuffer] UnBind  -  Only support Vertex and Pixel Shader");
                return;
        }
        _bindingSlotByShader[shaderType] = null;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _buffer.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    ~ConstantBuffer()
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
