using System.Numerics;
using System.Runtime.InteropServices;

namespace PetitMoteur3D.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
internal struct LightShadersParams
{
    /// <summary>
    /// la position de la source d’éclairage (Source Point)
    /// </summary>
    public Vector4 Position;
    /// <summary>
    /// la direction de la source d’éclairage (Source Directionnelle)
    /// </summary>
    public Vector4 Direction;
    /// <summary>
    /// la valeur ambiante de l’éclairage
    /// </summary>
    public Vector4 AmbiantColor;
    /// <summary>
    /// la valeur diffuse de l’éclairage
    /// </summary>
    public Vector4 DiffuseColor;
}
