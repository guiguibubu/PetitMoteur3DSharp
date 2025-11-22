using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Stages;

internal sealed class VertexShaderStage
{
    private readonly ComPtr<ID3D11DeviceContext> _deviceContext;
    public VertexShaderStage(ComPtr<ID3D11DeviceContext> deviceContext) { _deviceContext = deviceContext; }

    public void SetShader(ComPtr<ID3D11VertexShader> pVertexShader, ref ComPtr<ID3D11ClassInstance> ppClassInstances, uint numClassInstances)
    {
        _deviceContext.VSSetShader(pVertexShader, ref ppClassInstances, numClassInstances);
    }

    public unsafe void SetShader(ref readonly ComPtr<ID3D11VertexShader> pVertexShader, ref readonly ComPtr<ID3D11ClassInstance> ppClassInstances, uint numClassInstances)
    {
        _deviceContext.VSSetShader((ID3D11VertexShader*)pVertexShader, (ID3D11ClassInstance**)ppClassInstances.GetAddressOf(), numClassInstances);
    }

    public void SetConstantBuffers(uint StartSlot, uint NumBuffers, ref ComPtr<ID3D11Buffer> ppConstantBuffers)
    {
        _deviceContext.VSSetConstantBuffers(StartSlot, NumBuffers, ref ppConstantBuffers);
    }

    public void GetShader(ref ComPtr<ID3D11VertexShader> ppVertexShader, ref ComPtr<ID3D11ClassInstance> ppClassInstances, ref uint pNumClassInstances)
    {
        _deviceContext.VSGetShader(ref ppVertexShader, ref ppClassInstances, ref pNumClassInstances);
    }

    public void GetConstantBuffers(uint StartSlot, uint NumBuffers, ref ComPtr<ID3D11Buffer> ppConstantBuffers)
    {
        _deviceContext.VSGetConstantBuffers(StartSlot, NumBuffers, ref ppConstantBuffers);
    }
}
