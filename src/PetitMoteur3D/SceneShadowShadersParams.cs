using System.Runtime.InteropServices;

namespace PetitMoteur3D;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
internal struct SceneShadowShadersParams
{
    /// <summary>
    /// Draw or not the shadows
    /// </summary>
    public int DrawShadow;
    private readonly int alignement1_1; // 4 bytes
    private readonly ulong alignement1_2; // 8 bytes
}
