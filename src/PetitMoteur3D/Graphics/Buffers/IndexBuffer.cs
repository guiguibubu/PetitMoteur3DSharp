using System;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Buffers;

internal sealed class IndexBuffer : IDisposable
{
    public GraphicBuffer Buffer => _buffer;

    private readonly GraphicBuffer _buffer;
    private readonly Silk.NET.DXGI.Format _format;
    private bool _isBound;

    private bool _disposedValue;

    public IndexBuffer(GraphicBuffer buffer, Silk.NET.DXGI.Format format)
    {
        _format = format;
        _buffer = buffer;
        _isBound = false;

        _disposedValue = false;
    }

    public bool Bind(D3D11GraphicPipeline graphicPipeline)
    {
        graphicPipeline.InputAssemblerStage.SetIndexBuffer(_buffer.DataRef, _format, 0);
        _isBound = true;
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>No-op if not bound</remarks>
    /// <param name="graphicPipeline"></param>
    /// <param name="idSlot"></param>
    public unsafe void UnBind(D3D11GraphicPipeline graphicPipeline)
    {
        if (_isBound)
        {
            return;
        }
        ID3D11Buffer* buffer = (ID3D11Buffer*)null;
        graphicPipeline.InputAssemblerStage.UnbindIndexBuffer();
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

    ~IndexBuffer()
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
