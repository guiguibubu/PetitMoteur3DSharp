using Silk.NET.Maths;

namespace PetitMoteur3D
{
    internal struct ShadersParams
    {
        public Matrix4X4<float> matWorldViewProj; // la matrice totale
        public Matrix4X4<float> matWorld; // matrice de transformation dans le monde
        public Vector4D<float> vLumiere; // la position de la source d’éclairage (Point)
        public Vector4D<float> vCamera; // la position de la caméra
        public Vector4D<float> vAEcl; // la valeur ambiante de l’éclairage
        public Vector4D<float> vAMat; // la valeur ambiante du matériau
        public Vector4D<float> vDEcl; // la valeur diffuse de l’éclairage
        public Vector4D<float> vDMat; // la valeur diffuse du matériau
    }
}
