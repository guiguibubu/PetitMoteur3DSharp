using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace PetitMoteur3D
{
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct SceneShadersParams
    {
        /// <summary>
        /// la position de la source d’éclairage (Point)
        /// </summary>
        public Vector4D<float> lightPos;
        /// <summary>
        /// la position de la caméra
        /// </summary>
        public Vector4D<float> cameraPos;
        /// <summary>
        /// la valeur ambiante de l’éclairage
        /// </summary>
        public Vector4D<float> ambiantLightValue;
        /// <summary>
        /// la valeur diffuse de l’éclairage
        /// </summary>
        public Vector4D<float> diffuseLightValue;
    }
}
