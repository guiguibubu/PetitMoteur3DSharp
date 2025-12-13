using System.Numerics;
using System.Runtime.InteropServices;
using PetitMoteur3D.Core.Memory;

namespace PetitMoteur3D.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
internal struct ObjectShadowMapShadersParams : IResetable
{
    /// <summary>
    /// la matrice totale
    /// </summary>
    public Matrix4x4 matWorldViewProjLight;

    public void Reset()
    {
        MemoryHelper.ResetMemory(this);
    }
}
