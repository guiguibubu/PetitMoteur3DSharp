using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using PetitMoteur3D.Logging;
using Assimp = Silk.NET.Assimp;

namespace PetitMoteur3D.Graphics;

internal sealed class MeshLoader
{
    private readonly Assimp.Assimp _importer;
    public MeshLoader()
    {
        _importer = Assimp.Assimp.GetApi();
    }

    public unsafe SceneMesh[] Load(string filePath)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        uint postProcessFlags = (uint)Assimp.PostProcessSteps.Triangulate
        | (uint)Assimp.PostProcessSteps.FindInvalidData
        | (uint)Assimp.PostProcessSteps.GenerateNormals
        | (uint)Assimp.PostProcessSteps.CalculateTangentSpace
        | (uint)Assimp.PostProcessSteps.MakeLeftHanded
        | (uint)Assimp.PostProcessSteps.FlipWindingOrder
        | (uint)Assimp.PostProcessSteps.FlipUVs;
        Assimp.Scene* scenePtr = _importer.ImportFileFromMemory(in fileData[0], (uint)fileData.Length, postProcessFlags, (byte*)0);
        SceneMesh[] sceneMeshes;
        try
        {
            if (scenePtr is null)
            {
                throw new MeshLoaderException("Failed to import : {FilePath}", filePath);
            }
            Assimp.Scene scene = *scenePtr;

            Material[] materials = ReadMaterials(scene);

            Mesh[] meshes = ReadMeshes(scene, materials);

            Assimp.Node* rootNode = scene.MRootNode;
            if (rootNode is null)
            {
                sceneMeshes = meshes.Select(m => new SceneMesh(m)).ToArray();
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

    private unsafe Material[] ReadMaterials(Assimp.Scene scene)
    {
        uint nbMaterials = scene.MNumMaterials;
        Material[] materials = new Material[nbMaterials];
        for (int i = 0; i < nbMaterials; i++)
        {
            Assimp.Material* material = scene.MMaterials[i];
            Vector4 diffuse = new Vector4(0.8f, 0.8f, 0.8f, 1f);
            Vector4 specular = Vector4.Zero;
            Vector4 ambient = new Vector4(0.2f, 0.2f, 0.2f, 1f);
            Vector4 emission = Vector4.Zero;
            Vector4 reflexion = Vector4.Zero;
            float shininess = 0;
            Assimp.AssimpString materialNameTmp = "";
            string materialName;
            if (_importer.GetMaterialString(material, Assimp.Assimp.MatkeyName, (uint)Assimp.TextureType.None, 0, ref materialNameTmp) == Assimp.Return.Success)
            {
                materialName = materialNameTmp.AsString;
            }
            else
            {
                materialName = scene.MName.AsString + " - Material " + i;
            }

            Log.Information("[MeshLoader] Loading of {0} started", materialName);

            if (_importer.GetMaterialColor(material, Assimp.Assimp.MatkeyColorDiffuse, (uint)Assimp.TextureType.None, 0, ref diffuse) == Assimp.Return.Success)
            {
                Log.Information("[MeshLoader] {0} : DiffuseColor = {1}", materialName, diffuse);
            }
            else
            {
                Log.Information("[MeshLoader] {0} : No DiffuseColor", materialName);
            }
            if (_importer.GetMaterialColor(material, Assimp.Assimp.MatkeyColorSpecular, (uint)Assimp.TextureType.None, 0, ref specular) == Assimp.Return.Success)
            {
                Log.Information("[MeshLoader] {0} : SpecularColor = {1}", materialName, specular);
            }
            else
            {
                Log.Information("[MeshLoader] {0} : No SpecularColor", materialName);
            }
            if (_importer.GetMaterialColor(material, Assimp.Assimp.MatkeyColorAmbient, (uint)Assimp.TextureType.None, 0, ref ambient) == Assimp.Return.Success)
            {
                Log.Information("[MeshLoader] {0} : AmbientColor = {1}", materialName, ambient);
            }
            else
            {
                Log.Information("[MeshLoader] {0} : No AmbientColor", materialName);
            }
            if (_importer.GetMaterialColor(material, Assimp.Assimp.MatkeyColorEmissive, (uint)Assimp.TextureType.None, 0, ref emission) == Assimp.Return.Success)
            {
                Log.Information("[MeshLoader] {0} : EmissiveColor = {1}", materialName, emission);
            }
            else
            {
                Log.Information("[MeshLoader] {0} : No EmissiveColor", materialName);
            }
            if (_importer.GetMaterialColor(material, Assimp.Assimp.MatkeyColorReflective, (uint)Assimp.TextureType.None, 0, ref reflexion) == Assimp.Return.Success)
            {
                Log.Information("[MeshLoader] {0} : ReflectiveColor = {1}", materialName, reflexion);
            }
            else
            {
                Log.Information("[MeshLoader] {0} : No ReflectiveColor", materialName);
            }
            // Defines the shininess of a phong-shaded material. This is actually the exponent of the phong specular equation
            if (_importer.GetMaterialFloatArray(material, Assimp.Assimp.MatkeyShininess, (uint)Assimp.TextureType.None, 0, ref shininess, null) == Assimp.Return.Success)
            {
                Log.Information("[MeshLoader] {0} : Shininess = {1}", materialName, reflexion);
            }
            else
            {
                Log.Information("[MeshLoader] {0} : No Shininess", materialName);
            }

            materials[i] = new Material(
                    ambient,
                    diffuse,
                    specular,
                    emission,
                    reflexion,
                    shininess,
                    false
                );

            Log.Information("[MeshLoader] Loading of {0} finished", materialName);
        }
        return materials;
    }

    private static unsafe Mesh[] ReadMeshes(Assimp.Scene scene, Material[] sceneMaterials)
    {
        uint nbSubmesh = scene.MNumMeshes;
        Mesh[] meshes = new Mesh[nbSubmesh];
        for (int i = 0; i < nbSubmesh; i++)
        {
            Assimp.Mesh mesh = *scene.MMeshes[i];
            Sommet[] vertices = ReadVertices(mesh);

            ushort[] indices = ReadIndices(mesh);

            meshes[i] = new Mesh(vertices, indices, sceneMaterials[(int)mesh.MMaterialIndex]);
        }
        return meshes;
    }

    private static unsafe SceneMesh[] ReadNode(Assimp.Node node, Mesh[] meshes)
    {
        uint nbSubmesh = node.MNumMeshes;
        SceneMesh[] sceneMeshes;
        if (nbSubmesh > 0)
        {
            sceneMeshes = new SceneMesh[(int)nbSubmesh];
            for (int i = 0; i < nbSubmesh; i++)
            {
                uint indexMesh = node.MMeshes[i];
                Mesh mesh = meshes[(int)indexMesh];
                Matrix4x4 transformation = node.MTransformation;
                SceneMesh sceneMesh = new(mesh, transformation);
                uint nbChildren = node.MNumChildren;
                for (int j = 0; j < nbChildren; j++)
                {
                    Assimp.Node* child = node.MChildren[j];
                    if (child is null)
                    {
                        continue;
                    }
                    sceneMesh.AddChildren(ReadNode(*child, meshes));
                }
                sceneMeshes[i] = sceneMesh;
            }
        }
        else
        {
            uint nbChildren = node.MNumChildren;
            List<SceneMesh> sceneMeshesTmp = new((int)nbChildren);
            for (int j = 0; j < nbChildren; j++)
            {
                Assimp.Node* child = node.MChildren[j];
                if (child is null)
                {
                    continue;
                }
                sceneMeshesTmp.AddRange(ReadNode(*child, meshes));
            }
            sceneMeshes = sceneMeshesTmp.ToArray();
        }
        return sceneMeshes;
    }

    private unsafe static Sommet[] ReadVertices(Assimp.Mesh mesh)
    {
        uint nbVertices = mesh.MNumVertices;
        Sommet[] vertices = new Sommet[nbVertices];
        bool hasTexture = mesh.MTextureCoords[0] is not null;
        bool hasTangent = mesh.MTangents is not null;
        for (int k = 0; k < nbVertices; k++)
        {
            Vector3 position = mesh.MVertices[k];
            Vector3 normal = mesh.MNormals[k];
            Vector3 tangent = hasTangent ? mesh.MTangents[k] : Vector3.Zero;
            Vector3 textureCoordPtr = hasTexture ? mesh.MTextureCoords[0][k] : Vector3.Zero;
            Sommet vertex = new Sommet(
                position,
                normal,
                tangent,
                new Vector2(textureCoordPtr.X, textureCoordPtr.Y)
            );

            vertices[k] = vertex;
        }
        return vertices;
    }

    private unsafe static ushort[] ReadIndices(Assimp.Mesh mesh)
    {
        uint nbFaces = mesh.MNumFaces;
        ushort[][] indicesPerFace = new ushort[nbFaces][];
        int indicesCount = 0;
        for (int j = 0; j < nbFaces; j++)
        {
            Assimp.Face face = mesh.MFaces[j];
            int nbIndices = (int)face.MNumIndices;
            indicesCount += nbIndices;
            Span<uint> indicesFace = new Span<uint>(face.MIndices, nbIndices);
            for (int l = 0; l < indicesFace.Length; l++)
            {
                indicesPerFace[j] = indicesFace.ToArray().Select(x => (ushort)x).ToArray();
            }
        }
        ushort[] indices = new ushort[indicesCount];
        int currentIndex = 0;
        foreach (ushort[] faceIndices in indicesPerFace)
        {
            foreach (ushort faceIndice in faceIndices)
            {
                indices[currentIndex] = faceIndice;
                currentIndex++;
            }
        }
        return indices;
    }

    private static void ThrowIfFailed(Assimp.Return returnCode)
    {
        if (returnCode == Assimp.Return.Success)
        {
            return;
        }
        if (returnCode == Assimp.Return.Outofmemory)
        {
            throw new MeshLoaderException("'ReturnOutofmemory' returned by ASSIMP");
        }
        else
        {
            throw new MeshLoaderException("ASSIMP returned a generic error code");
        }
    }

    public sealed class MeshLoaderException : ApplicationException
    {
        public MeshLoaderException(string text) : base(text) { }
        public MeshLoaderException([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args) : this(string.Format(format, args)) { }
    }
}