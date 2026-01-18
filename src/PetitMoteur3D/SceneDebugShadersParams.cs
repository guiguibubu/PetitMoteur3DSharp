using System.Runtime.InteropServices;

namespace PetitMoteur3D;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
internal struct SceneDebugShadersParams
{
    /// <summary>
    /// Scene is drawn or debug camera
    /// </summary>
    public int IsDebugCameraUsed;
    private readonly int alignement1_1; // 4 bytes
    private readonly ulong alignement1_2; // 8 bytes
}
