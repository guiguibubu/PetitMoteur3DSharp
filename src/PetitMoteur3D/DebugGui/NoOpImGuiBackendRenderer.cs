using System;
using ImGuiNET;

namespace PetitMoteur3D.DebugGui
{
    internal class NoOpImGuiBackendRenderer : IImGuiBackendRenderer
    {
        public void Dispose()
        {
            
        }

        public bool Init(ref readonly ImGuiIOPtr io)
        {
            io.Fonts.GetTexDataAsRGBA32(out nint _, out int _, out int _, out int _);
            return true;
        }

        public void NewFrame()
        {
            
        }

        public void RenderDrawData(ref readonly ImDrawDataPtr drawData)
        {

        }
    }
}
