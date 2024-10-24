using System;
using System.Collections.Generic;

namespace PetitMoteur3D
{
    internal class Mesh
    {
        public IReadOnlyList<Sommet> Sommets { get { return _sommets; } }
        public IReadOnlyList<ushort> Indices { get { return _indices; } }
        public Material Material { get { return _material; } }

        private readonly IReadOnlyList<Sommet> _sommets;
        private readonly IReadOnlyList<ushort> _indices;
        private readonly Material _material; // Vecteur des matériaux

        public Mesh(IReadOnlyList<Sommet> sommets, IReadOnlyList<ushort> indices, Material material)
        {
            _sommets = sommets;
            _indices = indices;
            _material = material;
        }
    }
}