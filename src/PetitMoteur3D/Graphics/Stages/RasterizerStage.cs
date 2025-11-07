using System;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Stages;

internal class RasterizerStage
{
    private readonly ComPtr<ID3D11DeviceContext> _deviceContext;
    public RasterizerStage(ComPtr<ID3D11DeviceContext> deviceContext) { _deviceContext = deviceContext; }

    public void SetScissorRects(uint NumRects, in Silk.NET.Maths.Box2D<int> pRects)
    {
        _deviceContext.RSSetScissorRects(NumRects, in pRects);
    }

    public void SetScissorRects(uint NumRects, ReadOnlySpan<Silk.NET.Maths.Box2D<int>> pRects)
    {
        _deviceContext.RSSetScissorRects(NumRects, in pRects.GetPinnableReference());
    }

    public void SetState(ComPtr<ID3D11RasterizerState> pRasterizerState)
    {
        _deviceContext.RSSetState(pRasterizerState);
    }

    public void SetViewports(uint NumViewports, in Viewport pViewports)
    {
        _deviceContext.RSSetViewports(NumViewports, in pViewports);
    }

    public void SetViewports(uint NumViewports, ReadOnlySpan<Viewport> pViewports)
    {
        _deviceContext.RSSetViewports(NumViewports, in pViewports.GetPinnableReference());
    }

    public void GetScissorRects(ref uint pNumRects, ref Silk.NET.Maths.Box2D<int> pRects)
    {
        _deviceContext.RSGetScissorRects(ref pNumRects, ref pRects);
    }

    public void GetState(ref ComPtr<ID3D11RasterizerState> ppRasterizerState)
    {
        _deviceContext.RSGetState(ref ppRasterizerState);
    }

    public void GetViewports(ref uint pNumViewports, ref Viewport pViewports)
    {
        _deviceContext.RSGetViewports(ref pNumViewports, ref pViewports);
    }
}
