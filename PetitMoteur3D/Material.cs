using Silk.NET.Maths;

namespace PetitMoteur3D
{
    internal struct Material
    {
        public readonly Vector4D<float> Ambient;
        public readonly Vector4D<float> Diffuse;
        public readonly Vector4D<float> Specular;
        public readonly Vector4D<float> Emission;
        public readonly Vector4D<float> Reflexion;
        public readonly float Puissance;
        public readonly bool Transparent;

        public Material(Vector4D<float> ambient,
        Vector4D<float> diffuse,
        Vector4D<float> specular,
        Vector4D<float> emission,
        Vector4D<float> reflexion,
        float puissance,
        bool transparent)
        {
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
            Emission = emission;
            Reflexion = reflexion;
            Puissance = puissance;
            Transparent = transparent;
        }
    }
}