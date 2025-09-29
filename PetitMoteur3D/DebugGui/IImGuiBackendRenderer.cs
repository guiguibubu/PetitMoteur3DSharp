using System;
using ImGuiNET;

namespace PetitMoteur3D.DebugGui
{
    internal interface IImGuiBackendRenderer : IDisposable
    {
        unsafe bool Init(ImGuiIOPtr io);
        unsafe void NewFrame();
        unsafe void RenderDrawData(ImDrawDataPtr drawData);
    }
}
