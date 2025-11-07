using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Stages;

internal class OutputMergerStage
{
    private readonly ComPtr<ID3D11DeviceContext> _deviceContext;
    public OutputMergerStage(ComPtr<ID3D11DeviceContext> deviceContext) { _deviceContext = deviceContext; }

    public void GetBlendState(ref ComPtr<ID3D11BlendState> ppBlendState, ref float BlendFactor, ref uint pSampleMask)
    {
        _deviceContext.OMGetBlendState(ref ppBlendState, ref BlendFactor, ref pSampleMask);
    }

    public void GetDepthStencilState(ref ComPtr<ID3D11DepthStencilState> ppDepthStencilState, ref uint pStencilRef)
    {
        _deviceContext.OMGetDepthStencilState(ref ppDepthStencilState, ref pStencilRef);
    }

    public void SetBlendState(ComPtr<ID3D11BlendState> pBlendState, ref float BlendFactor, uint SampleMask)
    {
        _deviceContext.OMSetBlendState(pBlendState, ref BlendFactor, SampleMask);
    }

    public void SetDepthStencilState(ComPtr<ID3D11DepthStencilState> pDepthStencilState, uint StencilRef)
    {
        _deviceContext.OMSetDepthStencilState(pDepthStencilState, StencilRef);
    }
}
