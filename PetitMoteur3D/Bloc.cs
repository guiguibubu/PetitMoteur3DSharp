using System.Collections.Generic;
using Silk.NET.Maths;

namespace PetitMoteur3D
{
    internal class Bloc : BaseObjet3D
    {
        private Vector3D<float>[] _vertices;
        private Vector3D<float>[] _normales;
        public unsafe Bloc(float dx, float dy, float dz, DeviceD3D11 renderDevice) : base(renderDevice)
        {
            _vertices = new Vector3D<float>[]
            {
                new(-dx / 2, dy / 2, -dz / 2),
                new(dx / 2, dy / 2, -dz / 2),
                new(dx / 2, -dy / 2, -dz / 2),
                new(-dx / 2, -dy / 2, -dz / 2),
                new(-dx / 2, dy / 2, dz / 2),
                new(-dx / 2, -dy / 2, dz / 2),
                new(dx / 2, -dy / 2, dz / 2),
                new(dx / 2, dy / 2, dz / 2)
            };

            _normales = new Vector3D<float>[]
            {
                new(0.0f, 0.0f, -1.0f), // devant
                new(0.0f, 0.0f, 1.0f), // arrière
                new(0.0f, -1.0f, 0.0f), // dessous
                new(0.0f, 1.0f, 0.0f), // dessus
                new(-1.0f, 0.0f, 0.0f), // face gauche
                new(1.0f, 0.0f, 0.0f) // face droite
            };

            _sommets = new Sommet[]
            {
                // Le devant du bloc
                new(_vertices[0], _normales[0]),
                new(_vertices[1], _normales[0]),
                new(_vertices[2], _normales[0]),
                new(_vertices[3], _normales[0]),
                // L’arrière du bloc
                new(_vertices[4], _normales[1]),
                new(_vertices[5], _normales[1]),
                new(_vertices[6], _normales[1]),
                new(_vertices[7], _normales[1]),
                // Le dessous du bloc
                new(_vertices[3], _normales[2]),
                new(_vertices[2], _normales[2]),
                new(_vertices[6], _normales[2]),
                new(_vertices[5], _normales[2]),
                // Le dessus du bloc
                new(_vertices[0], _normales[3]),
                new(_vertices[4], _normales[3]),
                new(_vertices[7], _normales[3]),
                new(_vertices[1], _normales[3]),
                // La face gauche
                new(_vertices[0], _normales[4]),
                new(_vertices[3], _normales[4]),
                new(_vertices[5], _normales[4]),
                new(_vertices[4], _normales[4]),
                // La face droite
                new(_vertices[1], _normales[5]),
                new(_vertices[7], _normales[5]),
                new(_vertices[6], _normales[5]),
                new(_vertices[2], _normales[5])
            };

            _indices = new ushort[]
            {
                0,1,2, // devant
                0,2,3, // devant
                5,6,7, // arrière
                5,7,4, // arrière
                8,9,10, // dessous
                8,10,11, // dessous
                13,14,15, // dessus
                13,15,12, // dessus
                19,16,17, // gauche
                19,17,18, // gauche
                20,21,22, // droite
                20,22,23 // droite
            };

            Initialisation();
        }

        /// <summary>
        /// Initialise les vertex
        /// </summary>
        /// <returns></returns>
        protected override IReadOnlyList<Sommet> InitVertex()
        {
            return _sommets;
        }

        /// <summary>
        /// Initialise l'index de rendu
        /// </summary>
        /// <returns></returns>
        protected override IReadOnlyList<ushort> InitIndex()
        {
            return _indices;
        }
    }
}
