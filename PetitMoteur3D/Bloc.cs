using System.Collections.Generic;
using Silk.NET.Maths;

namespace PetitMoteur3D
{
    internal class Bloc : BaseObjet3D
    {
        private readonly Vector3D<float>[] _vertices;
        private readonly Vector3D<float>[] _normales;
        private readonly Vector3D<float>[] _tangentes;
        private readonly Sommet[] _sommets;
        private readonly ushort[] _indices;

        private Matrix4X4<float> _transformation;

        public unsafe Bloc(float dx, float dy, float dz, DeviceD3D11 renderDevice, ShaderManager shaderManager) : base(renderDevice, shaderManager)
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
                new(0f, 0f, -1f), // devant
                new(0f, 0f, 1f), // arrière
                new(0f, -1f, 0f), // dessous
                new(0f, 1f, 0f), // dessus
                new(-1f, 0f, 0f), // face gauche
                new(1f, 0f, 0f) // face droite
            };

            _tangentes = new Vector3D<float>[]
            {
                new(1f, 0f, 0f), // devant
                new(-1f, 0f, 0f), // arrière
                new(1f, 0f, 0f), // dessous
                new(1f, 0f, 0f), // dessus
                new(0f, -1f, 0f), // face gauche
                new(0f, 1f, 0f) // face droite
            };

            _sommets = new Sommet[]
            {
                // Le devant du bloc
                new(_vertices[0], _normales[0], _tangentes[0], new Vector2D<float>(0f, 0f)),
                new(_vertices[1], _normales[0], _tangentes[0], new Vector2D<float>(1f, 0f)),
                new(_vertices[2], _normales[0], _tangentes[0], new Vector2D<float>(1f, 1f)),
                new(_vertices[3], _normales[0], _tangentes[0], new Vector2D<float>(0f, 1f)),
                // L’arrière du bloc
                new(_vertices[4], _normales[1], _tangentes[1], new Vector2D<float>(1f, 0f)),
                new(_vertices[5], _normales[1], _tangentes[1], new Vector2D<float>(1f, 1f)),
                new(_vertices[6], _normales[1], _tangentes[1], new Vector2D<float>(0f, 1f)),
                new(_vertices[7], _normales[1], _tangentes[1], new Vector2D<float>(0f, 0f)),
                // Le dessous du bloc
                new(_vertices[3], _normales[2], _tangentes[2], new Vector2D<float>(0f, 0f)),
                new(_vertices[2], _normales[2], _tangentes[2], new Vector2D<float>(1f, 0f)),
                new(_vertices[6], _normales[2], _tangentes[2], new Vector2D<float>(1f, 1f)),
                new(_vertices[5], _normales[2], _tangentes[2], new Vector2D<float>(0f, 1f)),
                // Le dessus du bloc
                new(_vertices[0], _normales[3], _tangentes[3], new Vector2D<float>(0f, 1f)),
                new(_vertices[4], _normales[3], _tangentes[3], new Vector2D<float>(0f, 0f)),
                new(_vertices[7], _normales[3], _tangentes[3], new Vector2D<float>(1f, 0f)),
                new(_vertices[1], _normales[3], _tangentes[3], new Vector2D<float>(1f, 1f)),
                // La face gauche
                new(_vertices[0], _normales[4], _tangentes[4], new Vector2D<float>(1f, 0f)),
                new(_vertices[3], _normales[4], _tangentes[4], new Vector2D<float>(1f, 1f)),
                new(_vertices[5], _normales[4], _tangentes[4], new Vector2D<float>(0f, 1f)),
                new(_vertices[4], _normales[4], _tangentes[4], new Vector2D<float>(0f, 0f)),
                // La face droite
                new(_vertices[1], _normales[5], _tangentes[5], new Vector2D<float>(0f, 0f)),
                new(_vertices[7], _normales[5], _tangentes[5], new Vector2D<float>(1f, 0f)),
                new(_vertices[6], _normales[5], _tangentes[5], new Vector2D<float>(1f, 1f)),
                new(_vertices[2], _normales[5], _tangentes[5], new Vector2D<float>(0f, 1f))
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

            _transformation = Matrix4X4<float>.Identity;

            Initialisation();
        }

        public void AddTransform(Matrix4X4<float> transformation)
        {
            _transformation = transformation * _transformation;
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<Sommet> InitVertex()
        {
            return _sommets;
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<ushort> InitIndex()
        {
            return _indices;
        }

        protected override IReadOnlyList<SubObjet3D> GetSubObjets()
        {
            return new SubObjet3D[] {new SubObjet3D()
                {
                    Indices = _indices,
                    Material = new Material(),
                    Transformation = _transformation
                }
            };
        }
    }
}
