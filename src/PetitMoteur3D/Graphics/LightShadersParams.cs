using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace PetitMoteur3D.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
internal struct LightShadersParams
{
    /// <summary>
    /// la position de la source d’éclairage (Source Point)
    /// </summary>
    public Vector4D<float> Position;
    /// <summary>
    /// la direction de la source d’éclairage (Source Directionnelle)
    /// </summary>
    public Vector4D<float> Direction;
    /// <summary>
    /// la valeur ambiante de l’éclairage
    /// </summary>
    public Vector4D<float> AmbiantColor;
    /// <summary>
    /// la valeur diffuse de l’éclairage
    /// </summary>
    public Vector4D<float> DiffuseColor;
}
