using Silk.NET.Maths;

namespace PetitMoteur3D.Graphics;

internal struct Material
{
    public readonly Vector4D<float> Ambient;
    public readonly Vector4D<float> Diffuse;
    public readonly Vector4D<float> Specular;
    public readonly Vector4D<float> Emission;
    public readonly Vector4D<float> Reflexion;
    public readonly float Puissance;
    public readonly bool Transparent;

    public Material()
    : this(Vector4D<float>.One, Vector4D<float>.One, Vector4D<float>.One, Vector4D<float>.One, Vector4D<float>.One, 1f, false)
    { }

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

    public Material Clone()
    {
        return new Material(
            new Vector4D<float>(Ambient.X, Ambient.Y, Ambient.Z, Ambient.W),
            new Vector4D<float>(Diffuse.X, Diffuse.Y, Diffuse.Z, Diffuse.W),
            new Vector4D<float>(Specular.X, Specular.Y, Specular.Z, Specular.W),
            new Vector4D<float>(Emission.X, Emission.Y, Emission.Z, Emission.W),
            new Vector4D<float>(Reflexion.X, Reflexion.Y, Reflexion.Z, Reflexion.W),
            Puissance,
            Transparent
        );
    }
}