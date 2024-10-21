using Silk.NET.Maths;

namespace PetitMoteur3D
{
    internal struct ShadersParams
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
        /// la valeur ambiante du matériau
        /// </summary>
        public Vector4D<float> ambiantMaterialValue;
        /// <summary>
        /// la valeur diffuse de l’éclairage
        /// </summary>
        public Vector4D<float> diffuseLightValue;
        /// <summary>
        /// la valeur diffuse du matériau
        /// </summary>
        public Vector4D<float> diffuseMaterialValue;
    }
}
