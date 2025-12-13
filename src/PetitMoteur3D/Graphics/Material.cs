using System.Numerics;

namespace PetitMoteur3D.Graphics;

internal struct Material
{
    public readonly Vector4 Ambient;
    public readonly Vector4 Diffuse;
    public readonly Vector4 Specular;
    public readonly Vector4 Emission;
    public readonly Vector4 Reflexion;
    public readonly float Puissance;
    public readonly bool Transparent;

    public Material()
    : this(Vector4.One, Vector4.One, Vector4.One, Vector4.One, Vector4.One, 1f, false)
    { }

    public Material(Vector4 ambient,
    Vector4 diffuse,
    Vector4 specular,
    Vector4 emission,
    Vector4 reflexion,
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
            new Vector4(Ambient.X, Ambient.Y, Ambient.Z, Ambient.W),
            new Vector4(Diffuse.X, Diffuse.Y, Diffuse.Z, Diffuse.W),
            new Vector4(Specular.X, Specular.Y, Specular.Z, Specular.W),
            new Vector4(Emission.X, Emission.Y, Emission.Z, Emission.W),
            new Vector4(Reflexion.X, Reflexion.Y, Reflexion.Z, Reflexion.W),
            Puissance,
            Transparent
        );
    }
}