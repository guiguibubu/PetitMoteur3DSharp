using System.Linq;

namespace PetitMoteur3D.Graphics;

internal sealed class Mesh
{
    public Sommet[] Sommets { get { return _sommets; } }
    public ushort[] Indices { get { return _indices; } }
    public Material Material { get { return _material; } }

    private readonly Sommet[] _sommets;
    private readonly ushort[] _indices;
    private readonly Material _material;

    public Mesh(Sommet[] sommets, ushort[] indices, Material material)
    {
        _sommets = sommets;
        _indices = indices;
        _material = material;
    }

    public Mesh Clone()
    {
        Sommet[] sommets = _sommets.Select(x => x.Clone()).ToArray();
        ushort[] indices = _indices.ToArray();
        Material material = _material.Clone();
        return new Mesh(sommets, indices, material);
    }
}