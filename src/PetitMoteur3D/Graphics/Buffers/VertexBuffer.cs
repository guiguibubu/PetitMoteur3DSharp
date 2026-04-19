using System;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Buffers;

internal sealed class VertexBuffer : IDisposable
{
    public GraphicBuffer Buffer => _buffer;

    public readonly uint _stride;

    private readonly  GraphicBuffer _buffer;
    private bool _isBound;

    private bool _disposedValue;

    public VertexBuffer(GraphicBuffer buffer)
    {
        _stride = buffer.Stride;
        _buffer = buffer;
        _isBound = false;

        _disposedValue = false;
    }

    public bool Bind(D3D11GraphicPipeline graphicPipeline, uint idSlot)
    {
        graphicPipeline.InputAssemblerStage.SetVertexBuffers(idSlot, 1, ref _buffer.DataRef, _stride, 0);
        _isBound = true;
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>No-op if not bound</remarks>
    /// <param name="graphicPipeline"></param>
    /// <param name="idSlot"></param>
    public unsafe void UnBind(D3D11GraphicPipeline graphicPipeline, uint idSlot)
    {
        if (_isBound)
        {
            return;
        }
        ID3D11Buffer* buffer = (ID3D11Buffer*)null;
        graphicPipeline.InputAssemblerStage.UnbindVertexBuffers(idSlot, 1);
        _isBound = false;
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

    ~VertexBuffer()
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
