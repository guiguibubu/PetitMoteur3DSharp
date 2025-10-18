using System;
using System.Runtime.InteropServices;
using ImGuiNET;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.DebugGui
{
    internal static class ImDrawVertInputLayout
    {
        /// <summary>
        /// Defini l’organisation de notre sommet
        /// </summary>
        public static InputElementDesc[] InputLayoutDesc => s_inputElements;

        private static readonly GlobalMemory s_semanticNamePosition;
        private static readonly GlobalMemory s_semanticNameTexCoord;
        private static readonly GlobalMemory s_semanticNameColor;
        private static readonly InputElementDesc[] s_inputElements;

        static unsafe ImDrawVertInputLayout()
        {
            s_semanticNamePosition = SilkMarshal.StringToMemory("POSITION", NativeStringEncoding.Ansi);
            s_semanticNameTexCoord = SilkMarshal.StringToMemory("TEXCOORD", NativeStringEncoding.Ansi);
            s_semanticNameColor = SilkMarshal.StringToMemory("COLOR", NativeStringEncoding.Ansi);

            s_inputElements = new[]
            {
                new InputElementDesc(
                    s_semanticNamePosition.AsPtr<byte>(), 0, Silk.NET.DXGI.Format.FormatR32G32Float, 0, Convert.ToUInt32(Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.pos))), InputClassification.PerVertexData, 0
                ),
                new InputElementDesc(
                    s_semanticNameTexCoord.AsPtr<byte>(), 0, Silk.NET.DXGI.Format.FormatR32G32Float, 0, Convert.ToUInt32(Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.uv))), InputClassification.PerVertexData, 0
                ),
                new InputElementDesc(
                    s_semanticNameColor.AsPtr<byte>(), 0, Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm, 0, Convert.ToUInt32(Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.col))), InputClassification.PerVertexData, 0
                ),
            };
        }
    }
}
