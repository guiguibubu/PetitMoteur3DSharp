using System;
using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace PetitMoteur3D
{
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct ObjectShadersParams
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
        public int hasTexture;
        /// <summary>
        /// Indique la présence d'une texture pour le "normal mapping"
        /// </summary>
        public int hasNormalMap;
        private readonly UInt64 alignement1_1;
    }
}
