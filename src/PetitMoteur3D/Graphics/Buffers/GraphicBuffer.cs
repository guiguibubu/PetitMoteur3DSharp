using System;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Buffers;

internal class GraphicBuffer : IDisposable
{
    public ref ComPtr<ID3D11Buffer> DataRef => ref _data;
    public string Name { get; init; }
    public uint NbElements { get; init; }
    public uint Stride { get; init; }
    public BindFlag BindFlag => _bindFlag;

    private ComPtr<ID3D11Buffer> _data;
    private BindFlag _bindFlag;

    private D3D11GraphicDevice _graphicDevice;

    private bool _disposedValue;

    public GraphicBuffer(D3D11GraphicDevice graphicDevice, ComPtr<ID3D11Buffer> bufferData, BindFlag bindFlag, uint nbElements, uint stride, string name = "")
    {
        _graphicDevice = graphicDevice;
        _data = bufferData;
        NbElements = nbElements;
        Stride = stride;
        _bindFlag = bindFlag;

        if (string.IsNullOrEmpty(name))
        {
            Name = GetType().Name + "_" + Guid.NewGuid().ToString();
        }
        else
        {
            Name = name;
        }

        _disposedValue = false;
    }

    public void UpdateSubresource<T>(ComPtr<ID3D11DeviceContext> deviceContext, uint dstSubresource, in Box pDstBox, in T pSrcData, uint srcRowPitch, uint srcDepthPitch) where T : unmanaged
    {
        deviceContext.UpdateSubresource(_data, dstSubresource, in pDstBox, in pSrcData, srcRowPitch, srcDepthPitch);
    }

    public void Map(ComPtr<ID3D11DeviceContext> deviceContext, uint subresource, Map mapType, uint mapFlags, ref MappedSubresource mappedResource)
    {
        SilkMarshal.ThrowHResult(deviceContext.Map(_data, subresource, mapType, mapFlags, ref mappedResource));
    }

    public void Unmap(ComPtr<ID3D11DeviceContext> deviceContext, uint subresource)
    {
        deviceContext.Unmap(_data, subresource);
    }

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
            _disposedValue = true;
        }
    }

    ~GraphicBuffer()
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
