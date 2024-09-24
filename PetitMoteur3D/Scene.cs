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

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public Scene() : this(Array.Empty<IObjet3D>()) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public Scene(IObjet3D obj) : this(new IObjet3D[] { obj }) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objets"></param>
        public Scene(IReadOnlyList<IObjet3D> objets)
        {
            _objects = new List<IObjet3D>(objets);
        }

        public void AddObjet(IObjet3D obj)
        {
            _objects.Add(obj);
        }


    }
}
