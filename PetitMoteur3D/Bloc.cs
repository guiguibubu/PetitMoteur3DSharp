using Silk.NET.Direct3D.Compilers;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetitMoteur3D
{
    internal class Bloc : IObjet3D
    {
        public float DX { get; private set; }
        public float DY { get; private set; }
        public float DZ { get; private set; }
        public Bloc(float dx, float dy, float dz)
        {
            DX = dx;
            DY = dy;
            DZ = dz;

            Vector3D<float> points =
        }

        public void Draw()
        {
            throw new NotImplementedException();
        }
    }
}
