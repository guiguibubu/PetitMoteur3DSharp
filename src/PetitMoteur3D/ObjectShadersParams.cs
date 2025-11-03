using System;
using System.Runtime.InteropServices;
using PetitMoteur3D.Core;
using Silk.NET.Maths;

namespace PetitMoteur3D;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
internal struct ObjectShadersParams : IResetable
{
    /// <summary>
    /// la matrice totale
    /// </summary>
    public Matrix4X4<float> matWorldViewProj;
    /// <summary>
    /// matrice de transformation dans le monde
    /// </summary>
    public Matrix4X4<float> matWorld;
    /// <summary>
    /// la valeur ambiante du matériau
    /// </summary>
    public Vector4D<float> ambiantMaterialValue;
    /// <summary>
    /// la valeur diffuse du matériau
    /// </summary>
    public Vector4D<float> diffuseMaterialValue;
    /// <summary>
    /// Indique la présence d'une texture
    /// </summary>
    public Int32 hasTexture;
    /// <summary>
    /// Indique la présence d'une texture pour le "normal mapping"
    /// </summary>
    public Int32 hasNormalMap;
    private readonly UInt64 alignement1_1;

    public void Reset()
    {
        MemoryHelper.ResetMemory(this);
    }
}
