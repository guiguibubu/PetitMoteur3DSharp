using System.Numerics;
using System.Runtime.InteropServices;
using PetitMoteur3D.Graphics;

namespace PetitMoteur3D;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
internal struct SceneShadersParams
{
    /// <summary>
    /// les infos de la lumiere
    /// </summary>
    public LightShadersParams LightParams;
    /// <summary>
    /// la position de la caméra
    /// </summary>
    public Vector4 CameraPos;
}
