using System.Runtime.InteropServices;
using PetitMoteur3D.Core.Memory;
using Silk.NET.Maths;

namespace PetitMoteur3D.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
internal struct ObjectShadowMapShadersParams : IResetable
{
    /// <summary>
    /// la matrice totale
    /// </summary>
    public Matrix4X4<float> matWorldViewProjLight;

    public void Reset()
    {
        MemoryHelper.ResetMemory(this);
    }
}
