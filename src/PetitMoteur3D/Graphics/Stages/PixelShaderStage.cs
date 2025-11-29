using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Stages;

internal sealed class PixelShaderStage
{
    private readonly ComPtr<ID3D11DeviceContext> _deviceContext;
    public PixelShaderStage(ComPtr<ID3D11DeviceContext> deviceContext) { _deviceContext = deviceContext; }

    public void SetShader(ComPtr<ID3D11PixelShader> pVertexShader, ref ComPtr<ID3D11ClassInstance> ppClassInstances, uint NumClassInstances)
    {
        _deviceContext.PSSetShader(pVertexShader, ref ppClassInstances, 0);
    }

    public void SetConstantBuffers(uint StartSlot, uint NumBuffers, ref ComPtr<ID3D11Buffer> ppConstantBuffers)
    {
        _deviceContext.PSSetConstantBuffers(StartSlot, NumBuffers, ref ppConstantBuffers);
    }

    public void GetShader(ref ComPtr<ID3D11PixelShader> ppVertexShader, ref ComPtr<ID3D11ClassInstance> ppClassInstances, ref uint pNumClassInstances)
    {
        _deviceContext.PSGetShader(ref ppVertexShader, ref ppClassInstances, ref pNumClassInstances);
    }

    public void SetShaderResources(uint StartSlot, uint NumViews, ref ComPtr<ID3D11ShaderResourceView> ppShaderResourceViews)
    {
        _deviceContext.PSSetShaderResources(StartSlot, NumViews, ref ppShaderResourceViews);
    }

    public unsafe void ClearShaderResources(uint startSlot)
    {
        ID3D11ShaderResourceView* nullSRV = (ID3D11ShaderResourceView*)null;
        _deviceContext.PSSetShaderResources(startSlot, 1, in nullSRV);
    }

    public unsafe void SetSamplers(uint StartSlot, uint NumSamplers, ref readonly ComPtr<ID3D11SamplerState> ppSamplers)
    {
        _deviceContext.PSSetSamplers(StartSlot, NumSamplers, (ID3D11SamplerState**)ppSamplers.GetAddressOf()); ;
    }

    public void GetConstantBuffers(uint StartSlot, uint NumBuffers, ref ComPtr<ID3D11Buffer> ppConstantBuffers)
    {
        _deviceContext.PSGetConstantBuffers(StartSlot, NumBuffers, ref ppConstantBuffers);
    }

    public void GetSamplers(uint StartSlot, uint NumSamplers, ref ComPtr<ID3D11SamplerState> ppSamplers)
    {
        _deviceContext.PSGetSamplers(StartSlot, NumSamplers, ref ppSamplers);
    }

    public void GetShaderResources(uint StartSlot, uint NumViews, ref ComPtr<ID3D11ShaderResourceView> ppShaderResourceViews)
    {
        _deviceContext.PSGetShaderResources(StartSlot, NumViews, ref ppShaderResourceViews);
    }
}
