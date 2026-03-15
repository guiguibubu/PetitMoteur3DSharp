using System;
using System.Runtime.InteropServices;
using ImGuiNET;
using PetitMoteur3D.Graphics.Shaders;

namespace PetitMoteur3D.DebugGui;

internal static class ImDrawVertInputLayout
{
    /// <summary>
    /// Defini l’organisation de notre sommet
    /// </summary>
    public static InputLayoutDesc InputLayoutDesc => _inputLayoutDesc;

    private static InputLayoutDesc _inputLayoutDesc;

    static unsafe ImDrawVertInputLayout()
    {
        InputLayoutDescBuilder builder = new();

        builder.Add(D3D11Semantics.Position, Silk.NET.DXGI.Format.FormatR32G32Float, inputSlot: 0, alignedByteOffset: Convert.ToUInt32(Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.pos))));
        builder.Add(D3D11Semantics.TexCoord, Silk.NET.DXGI.Format.FormatR32G32Float, inputSlot: 0, alignedByteOffset: Convert.ToUInt32(Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.uv))));
        builder.Add(D3D11Semantics.Color, Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm, inputSlot: 0, alignedByteOffset: Convert.ToUInt32(Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.col))));

        _inputLayoutDesc = builder.Build();
    }
}
