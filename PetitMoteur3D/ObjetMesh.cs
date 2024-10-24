using System.Collections.Generic;
using System.Linq;

namespace PetitMoteur3D
{
    internal class ObjetMesh : BaseObjet3D
    {
        private readonly Mesh _mesh;
        public ObjetMesh(Mesh mesh, DeviceD3D11 renderDevice, ShaderManager shaderManager) : base(renderDevice, shaderManager)
        {
            _mesh = mesh;
            
            Initialisation();
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<Sommet> InitVertex()
        {
            return _mesh.Sommets;
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<ushort> InitIndex()
        {
            return _mesh.Indices;
        }

    }
}