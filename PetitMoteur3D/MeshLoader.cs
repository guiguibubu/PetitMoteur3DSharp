using System;
using System.Collections.Generic;
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

        public unsafe IReadOnlyList<Mesh>? Load(string filePath)
        {
            byte[] fileData = System.IO.File.ReadAllBytes(filePath);
            uint postProcessFlags = (uint)PostProcessSteps.Triangulate
            | (uint)PostProcessSteps.MakeLeftHanded
            | (uint)PostProcessSteps.FlipWindingOrder
            | (uint)PostProcessSteps.FlipUVs;
            Silk.NET.Assimp.Scene* scenePtr = _importer.ImportFileFromMemory(in fileData[0], (uint)fileData.Length, postProcessFlags, (byte*)0);
            if (scenePtr is null)
            {
                return null;
            }
            Silk.NET.Assimp.Scene scene = *scenePtr;

            IReadOnlyList<Material> materials = ReadMaterials(scene, _importer);

            IReadOnlyList<Mesh> meshes = ReadMeshes(scene, materials);

            _importer.ReleaseImport(scenePtr);
            return meshes;
        }

        private static unsafe IReadOnlyList<Material> ReadMaterials(Silk.NET.Assimp.Scene scene, Assimp importer)
        {
            List<Material> materials = new();
            for (int i = 0; i < scene.MNumMaterials; i++)
            {
                Silk.NET.Assimp.Material* material = scene.MMaterials[i];
                System.Numerics.Vector4 diffuse = System.Numerics.Vector4.Zero;
                System.Numerics.Vector4 specular = System.Numerics.Vector4.Zero;
                System.Numerics.Vector4 ambient = System.Numerics.Vector4.Zero;
                System.Numerics.Vector4 emission = System.Numerics.Vector4.Zero;
                System.Numerics.Vector4 reflexion = System.Numerics.Vector4.Zero;
                float shininess = 0;
                uint max = 1;
                ThrowIfFailed(importer.GetMaterialColor(material, Assimp.MatkeyColorDiffuse, 0, 0, ref diffuse));
                ThrowIfFailed(importer.GetMaterialColor(material, Assimp.MatkeyColorSpecular, 0, 0, ref specular));
                ThrowIfFailed(importer.GetMaterialColor(material, Assimp.MatkeyColorAmbient, 0, 0, ref ambient));
                ThrowIfFailed(importer.GetMaterialColor(material, Assimp.MatkeyColorEmissive, 0, 0, ref emission));
                ThrowIfFailed(importer.GetMaterialColor(material, Assimp.MatkeyColorReflective, 0, 0, ref reflexion));
                ThrowIfFailed(importer.GetMaterialFloatArray(material, Assimp.MatkeyShininess, 0, 0, ref shininess, ref max));
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
                uint nbVertices = mesh.MNumVertices;
                Sommet[] vertices = new Sommet[nbVertices];
                for (var k = 0; k < nbVertices; k++)
                {
                    Sommet vertex = new Sommet(
                        mesh.MVertices[k].ToGeneric(),
                        mesh.MNormals[k].ToGeneric(),
                        new Vector2D<float>(mesh.MTextureCoords[k]->X, mesh.MTextureCoords[k]->Y)
                    );

                    vertices[k] = vertex;
                }

                uint nbFaces = mesh.MNumFaces;
                List<ushort> indices = new();
                for (var j = 0; j < nbFaces; j++)
                {
                    Silk.NET.Assimp.Face face = mesh.MFaces[j];
                    Span<uint> indicesFace = new Span<uint>(face.MIndices, (int)face.MNumIndices);
                    for (int l = 0; l < indicesFace.Length; l++)
                    {
                        indices.Add((ushort)indicesFace[l]);
                    }
                }

                meshes[i] = new Mesh(vertices, indices, sceneMaterials[(int)mesh.MMaterialIndex]);
            }
            return meshes;
        }

        private static void ThrowIfFailed(Silk.NET.Assimp.Return returnCode)
        {
            if (returnCode == Silk.NET.Assimp.Return.Success)
            {
                return;
            }
            if (returnCode == Silk.NET.Assimp.Return.Outofmemory)
            {
                throw new OutOfMemoryException("'ReturnOutofmemory' returned by ASSIMP");
            }
            else
            {
                throw new ApplicationException("ASSIMP returned a generic error code");
            }
        }
        /*
        for (var i = 0; i < pScene->MNumMeshes; i++)
            {
                SimpMesh* pMesh = pScene->MMeshes[i];
                var vertices = new TexturedVertex[pMesh->MNumVertices];

                var material = new Material();
                var simpMaterial = pScene->MMaterials[pMesh->MMaterialIndex];

                ThrowIfFailed(_assimp.GetMaterialColor(simpMaterial, (byte*)DiffuseKey.Pointer, 0, 0, &material.DiffuseAlbedo));
                ThrowIfFailed(_assimp.GetMaterialFloatArray(simpMaterial, (byte*)ShininessKey.Pointer, 0, 0, &material.Shininess, null));
                ThrowIfFailed(_assimp.GetMaterialColor(simpMaterial, (byte*)ReflectionFactorKey.Pointer, 0, 0, &material.ReflectionFactor));

                for (var k = 0; k < pMesh->MNumVertices; k++)
                {
                    var vertex = new TexturedVertex(
                        pMesh->MVertices[k],
                        pMesh->MNormals[k],
                        pMesh->MTangents[k],
                        ToVector2(pMesh->MTextureCoords_0[k])
                    );

                    vertices[k] = vertex;
                }

                var indices = new ArrayBuilder<uint>();
                for (var j = 0; j < pMesh->MNumFaces; j++)
                {
                    SimpFace pFace = pMesh->MFaces[j];
                    indices.Add(new Span<uint>(pFace.MIndices, (int)pFace.MNumIndices));
                }

                meshes.Add(new RenderObject<TexturedVertex>(vertices, indices.MoveTo(), material));
            }
        */
    }
}