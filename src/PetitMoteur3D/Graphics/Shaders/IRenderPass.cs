using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Shaders;

internal interface IRenderPass
{
    public string Name { get; }

    public ComPtr<ID3D11InputLayout> VertexLayout { get; }
    public ComPtr<ID3D11VertexShader> VertexShader { get; }
    public ComPtr<ID3D11GeometryShader> GeometryShader { get; }
    public ComPtr<ID3D11PixelShader> PixelShader { get; }

    #region Public methods
    #region Update Values
    public void UpdatePrimitiveTopology(D3DPrimitiveTopology topology);

    public void UpdateVertexBuffer(ComPtr<ID3D11Buffer> vertexBuffer, uint vertexStride);

    public void UpdateIndexBuffer(ComPtr<ID3D11Buffer> indexBuffer, Silk.NET.DXGI.Format format);
    #endregion

    #region Input Assembler
    public void SetPrimitiveTopology();

    public void SetVertexBuffer(uint offset = 0);

    public void SetIndexBuffer(uint offset = 0);

    public void SetInputLayout();
    #endregion

    #region Vertex Shader
    public void SetVertexShader();

    public void SetVertexShaderConstantBuffers();
    #endregion

    #region Geometry Shader
    public void SetGeometryShader();
    #endregion

    #region Pixel Shader
    public void SetPixelShader();

    public void SetPixelShaderConstantBuffers();

    public void SetPixelShaderRessources();

    public void SetSamplers();

    public void ClearPixelShaderResources();
    #endregion

    public void DrawIndexed(uint indexCount, uint startIndexLocation, int baseVertexLocation);
    #endregion
}
