using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Stages;

internal class InputAssemblerStage
{
    private readonly ComPtr<ID3D11DeviceContext> _deviceContext;
    public InputAssemblerStage(ComPtr<ID3D11DeviceContext> deviceContext) { _deviceContext = deviceContext; }

    public void SetPrimitiveTopology(D3DPrimitiveTopology topology)
    {
        _deviceContext.IASetPrimitiveTopology(topology);
    }

    public void SetVertexBuffers(uint startSlot, uint numBuffers, ref ComPtr<ID3D11Buffer> ppVertexBuffers, in uint pStrides, in uint pOffsets)
    {
        _deviceContext.IASetVertexBuffers(startSlot, numBuffers, ref ppVertexBuffers, in pStrides, in pOffsets);
    }

    public void SetIndexBuffer(ComPtr<ID3D11Buffer> pIndexBuffer, Silk.NET.DXGI.Format Format, uint Offset)
    {
        _deviceContext.IASetIndexBuffer(pIndexBuffer, Format, Offset);
    }

    public void SetInputLayout(ComPtr<ID3D11InputLayout> pInputLayout)
    {
        _deviceContext.IASetInputLayout(pInputLayout);
    }

    public void GetPrimitiveTopology(ref D3DPrimitiveTopology topology)
    {
        _deviceContext.IAGetPrimitiveTopology(ref topology);
    }

    public void GetIndexBuffer(ref ComPtr<ID3D11Buffer> pIndexBuffer, ref Silk.NET.DXGI.Format Format, ref uint Offset)
    {
        _deviceContext.IAGetIndexBuffer(ref pIndexBuffer, ref Format, ref Offset);
    }

    public void GetVertexBuffers(uint startSlot, uint numBuffers, ref ComPtr<ID3D11Buffer> ppVertexBuffers, ref uint pStrides, ref uint pOffsets)
    {
        _deviceContext.IAGetVertexBuffers(startSlot, numBuffers, ref ppVertexBuffers, ref pStrides, ref pOffsets);
    }

    public void GetInputLayout(ref ComPtr<ID3D11InputLayout> ppInputLayout)
    {
        _deviceContext.IAGetInputLayout(ref ppInputLayout);
    }
}
