using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Silk.NET.Assimp;
using Silk.NET.Maths;

namespace PetitMoteur3D
{
    internal class MeshLoader
    {
        private readonly Assimp _importer;
        public MeshLoader()
        {
            _importer = Assimp.GetApi();
        }

        public unsafe IReadOnlyList<SceneMesh> Load(string filePath)
        {
            byte[] fileData = System.IO.File.ReadAllBytes(filePath);
            uint postProcessFlags = (uint)PostProcessSteps.Triangulate
            | (uint)PostProcessSteps.FindInvalidData
            | (uint)PostProcessSteps.GenerateNormals
            | (uint)PostProcessSteps.CalculateTangentSpace
            | (uint)PostProcessSteps.MakeLeftHanded
            | (uint)PostProcessSteps.FlipWindingOrder
            | (uint)PostProcessSteps.FlipUVs;
            Silk.NET.Assimp.Scene* scenePtr = _importer.ImportFileFromMemory(in fileData[0], (uint)fileData.Length, postProcessFlags, (byte*)0);
            IReadOnlyList<SceneMesh> sceneMeshes;
            try
            {
                if (scenePtr is null)
                {
                    throw new MeshLoaderException("Failed to import : {FilePath}", filePath);
                }
                Silk.NET.Assimp.Scene scene = *scenePtr;

                IReadOnlyList<Material> materials = ReadMaterials(scene, _importer);

                IReadOnlyList<Mesh> meshes = ReadMeshes(scene, materials);

                Silk.NET.Assimp.Node* rootNode = scene.MRootNode;
                if (rootNode is null)
                {
                    sceneMeshes = meshes.Select(m => new SceneMesh(m)).ToList();
                }
                else
                {
                    sceneMeshes = ReadNode(*rootNode, meshes);
                }
            }
            finally
            {
                _importer.ReleaseImport(scenePtr);
            }
            return sceneMeshes;
        }

        private static unsafe IReadOnlyList<Material> ReadMaterials(Silk.NET.Assimp.Scene scene, Assimp importer)
        {
            List<Material> materials = new();
            for (int i = 0; i < scene.MNumMaterials; i++)
            {
                Silk.NET.Assimp.Material* material = scene.MMaterials[i];
                System.Numerics.Vector4 diffuse = new System.Numerics.Vector4(0.8f, 0.8f, 0.8f, 1f);
                System.Numerics.Vector4 specular = System.Numerics.Vector4.Zero;
                System.Numerics.Vector4 ambient = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1f);
                System.Numerics.Vector4 emission = System.Numerics.Vector4.Zero;
                System.Numerics.Vector4 reflexion = System.Numerics.Vector4.Zero;
                float shininess = 0;
                uint max = 1;
                importer.GetMaterialColor(material, Assimp.MatkeyColorDiffuse, 0, 0, ref diffuse);
                importer.GetMaterialColor(material, Assimp.MatkeyColorSpecular, 0, 0, ref specular);
                importer.GetMaterialColor(material, Assimp.MatkeyColorAmbient, 0, 0, ref ambient);
                importer.GetMaterialColor(material, Assimp.MatkeyColorEmissive, 0, 0, ref emission);
                importer.GetMaterialColor(material, Assimp.MatkeyColorReflective, 0, 0, ref reflexion);
                importer.GetMaterialFloatArray(material, Assimp.MatkeyShininess, 0, 0, ref shininess, ref max);
                materials.Add(new Material(
                    ambient.ToGeneric(),
                    diffuse.ToGeneric(),
                    specular.ToGeneric(),
                    emission.ToGeneric(),
                    reflexion.ToGeneric(),
                    shininess,
                    false
                ));
            }
            return materials;
        }

        private static unsafe IReadOnlyList<Mesh> ReadMeshes(Silk.NET.Assimp.Scene scene, IReadOnlyList<Material> sceneMaterials)
        {
            uint nbSubmesh = scene.MNumMeshes;
            Mesh[] meshes = new Mesh[nbSubmesh];
            for (int i = 0; i < nbSubmesh; i++)
            {
                Silk.NET.Assimp.Mesh mesh = *scene.MMeshes[i];
                IReadOnlyList<Sommet> vertices = ReadVertices(mesh);

                IReadOnlyList<ushort> indices = ReadIndices(mesh);

                meshes[i] = new Mesh(vertices, indices, sceneMaterials[(int)mesh.MMaterialIndex]);
            }
            return meshes;
        }

        private static unsafe IReadOnlyList<SceneMesh> ReadNode(Silk.NET.Assimp.Node node, IReadOnlyList<Mesh> meshes)
        {
            uint nbSubmesh = node.MNumMeshes;
            List<SceneMesh> sceneMeshes;
            if (nbSubmesh > 0)
            {
                sceneMeshes = new List<SceneMesh>((int)nbSubmesh);
                for (int i = 0; i < nbSubmesh; i++)
                {
                    uint indexMesh = node.MMeshes[i];
                    Mesh mesh = meshes[(int)indexMesh];
                    Matrix4X4<float> transformation = node.MTransformation.ToGeneric();
                    SceneMesh sceneMesh = new(mesh, transformation);
                    uint nbChildren = node.MNumChildren;
                    for (int j = 0; j < nbChildren; j++)
                    {
                        Silk.NET.Assimp.Node* child = node.MChildren[j];
                        if (child is null)
                        {
                            continue;
                        }
                        sceneMesh.AddChildren(ReadNode(*child, meshes));
                    }
                    sceneMeshes.Add(sceneMesh);
                }
            }
            else
            {
                uint nbChildren = node.MNumChildren;
                sceneMeshes = new List<SceneMesh>((int)nbChildren);
                for (int j = 0; j < nbChildren; j++)
                {
                    Silk.NET.Assimp.Node* child = node.MChildren[j];
                    if (child is null)
                    {
                        continue;
                    }
                    sceneMeshes.AddRange(ReadNode(*child, meshes));
                }
            }
            return sceneMeshes;
        }

        private unsafe static IReadOnlyList<Sommet> ReadVertices(Silk.NET.Assimp.Mesh mesh)
        {
            uint nbVertices = mesh.MNumVertices;
            Sommet[] vertices = new Sommet[nbVertices];
            bool hasTexture = mesh.MTextureCoords[0] is not null;
            bool hasTangent = mesh.MTangents is not null;
            for (int k = 0; k < nbVertices; k++)
            {
                System.Numerics.Vector3 position = mesh.MVertices[k];
                System.Numerics.Vector3 normal = mesh.MNormals[k];
                System.Numerics.Vector3 tangent = hasTangent ? mesh.MTangents[k] : System.Numerics.Vector3.Zero;
                System.Numerics.Vector3 textureCoordPtr = hasTexture ? mesh.MTextureCoords[0][k] : System.Numerics.Vector3.Zero;
                Sommet vertex = new Sommet(
                    position.ToGeneric(),
                    normal.ToGeneric(),
                    tangent.ToGeneric(),
                    new Vector2D<float>(textureCoordPtr.X, textureCoordPtr.Y)
                );

                vertices[k] = vertex;
            }
            return vertices;
        }

        private unsafe static IReadOnlyList<ushort> ReadIndices(Silk.NET.Assimp.Mesh mesh)
        {
            uint nbFaces = mesh.MNumFaces;
            List<ushort> indices = new();
            for (int j = 0; j < nbFaces; j++)
            {
                Silk.NET.Assimp.Face face = mesh.MFaces[j];
                Span<uint> indicesFace = new Span<uint>(face.MIndices, (int)face.MNumIndices);
                for (int l = 0; l < indicesFace.Length; l++)
                {
                    indices.Add((ushort)indicesFace[l]);
                }
            }
            return indices;
        }

        private static void ThrowIfFailed(Silk.NET.Assimp.Return returnCode)
        {
            if (returnCode == Silk.NET.Assimp.Return.Success)
            {
                return;
            }
            if (returnCode == Silk.NET.Assimp.Return.Outofmemory)
            {
                throw new MeshLoaderException("'ReturnOutofmemory' returned by ASSIMP");
            }
            else
            {
                throw new MeshLoaderException("ASSIMP returned a generic error code");
            }
        }

        public class MeshLoaderException : ApplicationException
        {
            public MeshLoaderException(string text) : base(text) { }
            public MeshLoaderException([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args) : this(string.Format(format, args)) { }
        }
    }
}