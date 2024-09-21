using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetitMoteur3D
{
    internal class Scene
    {
        private List<IObjet3D> _objects;

        public void Initialise()
        {
            _objects = new();
            _objects.Add(new Bloc(2.0f, 2.0f, 2.0f));
        }


    }
}
