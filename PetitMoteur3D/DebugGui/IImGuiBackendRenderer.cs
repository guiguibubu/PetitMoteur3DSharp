using System;
using ImGuiNET;

namespace PetitMoteur3D.DebugGui
{
    internal interface IImGuiBackendRenderer : IDisposable
    {
        unsafe bool Init(ref readonly ImGuiIOPtr io);
        unsafe void NewFrame();
        unsafe void RenderDrawData(ref readonly ImDrawDataPtr drawData);
    }
}
