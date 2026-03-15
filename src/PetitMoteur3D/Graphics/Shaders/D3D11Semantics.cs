namespace PetitMoteur3D.Graphics.Shaders;

/// <summary>
/// Static class containing HLSL input semantics for DirectX 11 (Vertex Shader).
/// https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics
/// </summary>
internal enum D3D11Semantics
{
    /// <summary>Vertex position (float3 or float4)</summary>
    Position,
    /// <summary>Vertex color (float4)</summary>
    Color,
    /// <summary>Texture coordinates (float2)</summary>
    TexCoord,
    /// <summary>Vertex normal (float3)</summary>
    Normal,
    /// <summary>Vertex tangent (float3 or float4)</summary>
    Tangent,
    /// <summary>Vertex binormal (float3)</summary>
    Binormal,
    /// <summary>Blend weights for skinning (float4)</summary>
    BlendWeight,
    /// <summary>Blend indices for skinning (uint4 or int4)</summary>
    BlendIndices

}
internal static class D3D11SemanticsExtensions
{
    /// <summary>
    /// Converts the semantic to its HLSL string representation.
    /// </summary>
    /// <param name="semantic">The semantic to convert.</param>
    /// <returns>The HLSL semantic string.</returns>
    public static string ToSemanticString(this D3D11Semantics semantic)
    {
        return semantic.ToString().ToUpperInvariant();
    }

    /// <summary>
    /// Converts the semantic to its HLSL string representation.
    /// </summary>
    /// <param name="semantic">The semantic to convert.</param>
    /// <returns>The HLSL semantic string.</returns>
    public static string ToSemanticString(this D3D11Semantics semantic, uint semanticIndex)
    {
        return semantic.ToString().ToUpperInvariant() + semanticIndex;
    }
}
