using System.Collections.Generic;
using Silk.NET.Maths;

namespace PetitMoteur3D
{
    internal class ObjetMesh : BaseObjet3D
    {
        public SceneMesh Mesh => _sceneMesh;
        private readonly SceneMesh _sceneMesh;
        public ObjetMesh(SceneMesh sceneMesh, DeviceD3D11 renderDevice, GraphicBufferFactory bufferFactory, ShaderManager shaderManager, TextureManager textureManager) 
            : base(renderDevice, bufferFactory, shaderManager, textureManager)
        {
            _sceneMesh = sceneMesh;

            Initialisation();
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<Sommet> InitVertex()
        {
            return _sceneMesh.GetAllVertices();
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<ushort> InitIndex()
        {
            return _sceneMesh.GetAllIndices();
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<SubObjet3D> GetSubObjets()
        {
            return GetSubObjets(_sceneMesh);
        }

        private static IReadOnlyList<SubObjet3D> GetSubObjets(SceneMesh sceneMesh)
        {
            List<SubObjet3D> subObjects = new();
            subObjects.Add(ToSubObjet3D(sceneMesh, Matrix4X4<float>.Identity));
            foreach (SceneMesh child in sceneMesh.Children)
            {
                subObjects.AddRange(GetSubObjets(child));
            }
            return subObjects;
        }

        private static SubObjet3D ToSubObjet3D(SceneMesh sceneMesh, Matrix4X4<float> parentTransform)
        {
            return new SubObjet3D()
            {
                Indices = sceneMesh.Mesh.Indices,
                Material = sceneMesh.Mesh.Material,
                Transformation = parentTransform * sceneMesh.Transformation
            };
        }
    }
}