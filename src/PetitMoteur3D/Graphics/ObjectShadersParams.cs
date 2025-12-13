using System.Numerics;
using System.Runtime.InteropServices;
using PetitMoteur3D.Core.Memory;

namespace PetitMoteur3D.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
internal struct ObjectShadersParams : IResetable
{
    /// <summary>
    /// la matrice totale
    /// </summary>
    public Matrix4x4 matWorldViewProj;
    /// <summary>
    /// matrice de transformation dans le monde
    /// </summary>
    public Matrix4x4 matWorld;
    /// <summary>
    /// la valeur ambiante du matériau
    /// </summary>
    public Vector4 ambiantMaterialValue;
    /// <summary>
    /// la valeur diffuse du matériau
    /// </summary>
    public Vector4 diffuseMaterialValue;
    /// <summary>
    /// Indique la présence d'une texture
    /// </summary>
    public int hasTexture;
    /// <summary>
    /// Indique la présence d'une texture pour le "normal mapping"
    /// </summary>
    public int hasNormalMap;
    private readonly ulong alignement1_1;

    public void Reset()
    {
        MemoryHelper.ResetMemory(this);
    }
}
