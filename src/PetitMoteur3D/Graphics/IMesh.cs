namespace PetitMoteur3D.Graphics;

internal interface IMesh
{
    Sommet[] Sommets { get; }
    ushort[] Indices { get; }
}
