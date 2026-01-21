using System.Numerics;
using System.Runtime.InteropServices;
using PetitMoteur3D.Core.Memory;

namespace PetitMoteur3D.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
internal struct DepthPixelShadersParams : IResetable
{
    /// <summary>
    /// la valeur ambiante du matériau
    /// </summary>
    public Vector4 successColor;
    /// <summary>
    /// la valeur diffuse du matériau
    /// </summary>
    public Vector4 failColor;

    public void Reset()
    {
        MemoryHelper.ResetMemory(this);
    }
}
