using System.Collections.Generic;
using System.Linq;

namespace PetitMoteur3D
{
    internal class Mesh
    {
        public IReadOnlyList<Sommet> Sommets { get { return _sommets; } }
        public IReadOnlyList<ushort> Indices { get { return _indices; } }
        public Material Material { get { return _material; } }

        private readonly IReadOnlyList<Sommet> _sommets;
        private readonly IReadOnlyList<ushort> _indices;
        private readonly Material _material;

        public Mesh(IReadOnlyList<Sommet> sommets, IReadOnlyList<ushort> indices, Material material)
        {
            _sommets = sommets;
            _indices = indices;
            _material = material;
        }

        public Mesh Clone()
        {
            IReadOnlyList<Sommet> sommets = _sommets.Select(x => x.Clone()).ToList();
            IReadOnlyList<ushort> indices = _indices.ToList();
            Material material = _material.Clone();
            return new Mesh(sommets, indices, material);
        }
    }
}