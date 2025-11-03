using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace PetitMoteur3D.DebugGui;

/// <summary>
/// ImGui necessary data for DirectX 11 Backend
/// </summary>
/// <remarks>
/// Adapted from ImGui_ImplDX11_Data struct in official ImGui code (https://github.com/ocornut/imgui/blob/master/backends/imgui_impl_dx11.cpp)
/// </remarks>
internal struct ImGuiImplDX11Data : IComVtbl<ImGuiImplDX11Data>
{
    public ComPtr<ID3D11Device> D3dDevice = null;
    public ComPtr<ID3D11DeviceContext> D3dDeviceContext = null;
    public ComPtr<IDXGIFactory> Factory = null;
    public ComPtr<ID3D11Buffer> VertexBuffer = null;
    public ComPtr<ID3D11Buffer> IndexBuffer = null;
    public ComPtr<ID3D11VertexShader> VertexShader = null;
    public ComPtr<ID3D11InputLayout> InputLayout = null;
    public ComPtr<ID3D11Buffer> VertexConstantBuffer = null;
    public ComPtr<ID3D11PixelShader> PixelShader = null;
    public ComPtr<ID3D11SamplerState> FontSampler = null;
    public ComPtr<ID3D11ShaderResourceView> FontTextureView = null;
    public ComPtr<ID3D11RasterizerState> RasterizerState = null;
    public ComPtr<ID3D11BlendState> BlendState = null;
    public ComPtr<ID3D11DepthStencilState> DepthStencilState = null;
    public int VertexBufferSize = DEFAULT_VERTEX_BUFFER_SIZE;
    public int IndexBufferSize = DEFAULT_INDEX_BUFFER_SIZE;

    private const int DEFAULT_VERTEX_BUFFER_SIZE = 5000;
    private const int DEFAULT_INDEX_BUFFER_SIZE = 10000;

    public ImGuiImplDX11Data() { }

    unsafe void*** IComVtbl.AsVtblPtr()
    {
        return (void***)Unsafe.AsPointer(ref Unsafe.AsRef(ref this));
    }
}
