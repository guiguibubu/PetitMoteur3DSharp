using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Stages;

internal class GeometryShaderStage
{
    private readonly ComPtr<ID3D11DeviceContext> _deviceContext;
    public GeometryShaderStage(ComPtr<ID3D11DeviceContext> deviceContext) { _deviceContext = deviceContext; }

    public void SetShader(ComPtr<ID3D11GeometryShader> pVertexShader, ref ComPtr<ID3D11ClassInstance> ppClassInstances, uint NumClassInstances)
    {
        _deviceContext.GSSetShader(pVertexShader, ref ppClassInstances, 0);
    }

    public void GetShader(ref ComPtr<ID3D11GeometryShader> ppVertexShader, ref ComPtr<ID3D11ClassInstance> ppClassInstances, ref uint pNumClassInstances)
    {
        _deviceContext.GSGetShader(ref ppVertexShader, ref ppClassInstances, ref pNumClassInstances);
    }
}
