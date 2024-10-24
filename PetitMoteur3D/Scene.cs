using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

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
        public Scene(params IObjet3D[] obj) : this(obj.AsEnumerable()) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objets"></param>
        public Scene(IEnumerable<IObjet3D> objets)
        {
            _objects = new List<IObjet3D>(objets);
        }

        public void AddObjet(IObjet3D obj)
        {
            _objects.Add(obj);
        }

        public void Anime(float elapsedTime)
        {
            foreach(IObjet3D obj in _objects)
            {
                obj.Anime(elapsedTime);
            }
        }

        public void Draw(ComPtr<ID3D11DeviceContext> deviceContext, Matrix4X4<float> matViewProj)
        {
            foreach (IObjet3D obj in _objects)
            {
                obj.Draw(deviceContext, matViewProj);
            }
        }
    }
}
