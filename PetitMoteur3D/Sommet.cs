using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PetitMoteur3D
{
    internal class Sommet
    {
        public Vector3D<float> Position { get; private set; }
        public Vector3D<float> Normale { get; private set; }
    }
}
