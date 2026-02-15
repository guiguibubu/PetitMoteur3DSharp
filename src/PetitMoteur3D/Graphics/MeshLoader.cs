using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Silk.NET.Assimp;

namespace PetitMoteur3D.Graphics;

internal sealed class MeshLoader
{
    private readonly Assimp _importer;
    public MeshLoader()
    {
        _importer = Assimp.GetApi();
    }

    public unsafe SceneMesh[] Load(string filePath)
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
        SceneMesh[] sceneMeshes;
        try
        {
            if (scenePtr is null)
            {
                throw new MeshLoaderException("Failed to import : {FilePath}", filePath);
            }
            Silk.NET.Assimp.Scene scene = *scenePtr;

            Material[] materials = ReadMaterials(scene, _importer);

            Mesh[] meshes = ReadMeshes(scene, materials);

            Node* rootNode = scene.MRootNode;
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

    private static unsafe Material[] ReadMaterials(Silk.NET.Assimp.Scene scene, Assimp importer)
    {
        uint nbMaterials = scene.MNumMaterials;
        Material[] materials = new Material[nbMaterials];
        for (int i = 0; i < nbMaterials; i++)
        {
            Silk.NET.Assimp.Material* material = scene.MMaterials[i];
            Vector4 diffuse = new Vector4(0.8f, 0.8f, 0.8f, 1f);
            Vector4 specular = Vector4.Zero;
            Vector4 ambient = new Vector4(0.2f, 0.2f, 0.2f, 1f);
            Vector4 emission = Vector4.Zero;
            Vector4 reflexion = Vector4.Zero;
            float shininess = 0;
            uint max = 1;
            importer.GetMaterialColor(material, Assimp.MatkeyColorDiffuse, 0, 0, ref diffuse);
            importer.GetMaterialColor(material, Assimp.MatkeyColorSpecular, 0, 0, ref specular);
            importer.GetMaterialColor(material, Assimp.MatkeyColorAmbient, 0, 0, ref ambient);
            importer.GetMaterialColor(material, Assimp.MatkeyColorEmissive, 0, 0, ref emission);
            importer.GetMaterialColor(material, Assimp.MatkeyColorReflective, 0, 0, ref reflexion);
            importer.GetMaterialFloatArray(material, Assimp.MatkeyShininess, 0, 0, ref shininess, ref max);
            materials[i] = new Material(
                ambient,
                diffuse,
                specular,
                emission,
                reflexion,
                shininess,
                false
            );
        }
        return materials;
    }

    private static unsafe Mesh[] ReadMeshes(Silk.NET.Assimp.Scene scene, Material[] sceneMaterials)
    {
        uint nbSubmesh = scene.MNumMeshes;
        Mesh[] meshes = new Mesh[nbSubmesh];
        for (int i = 0; i < nbSubmesh; i++)
        {
            Silk.NET.Assimp.Mesh mesh = *scene.MMeshes[i];
            Sommet[] vertices = ReadVertices(mesh);

            ushort[] indices = ReadIndices(mesh);

            meshes[i] = new Mesh(vertices, indices, sceneMaterials[(int)mesh.MMaterialIndex]);
        }
        return meshes;
    }

    private static unsafe SceneMesh[] ReadNode(Node node, Mesh[] meshes)
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
                    Node* child = node.MChildren[j];
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
                Node* child = node.MChildren[j];
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

    private unsafe static Sommet[] ReadVertices(Silk.NET.Assimp.Mesh mesh)
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

    private unsafe static ushort[] ReadIndices(Silk.NET.Assimp.Mesh mesh)
    {
        uint nbFaces = mesh.MNumFaces;
        ushort[][] indicesPerFace = new ushort[nbFaces][];
        int indicesCount = 0;
        for (int j = 0; j < nbFaces; j++)
        {
            Face face = mesh.MFaces[j];
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

    private static void ThrowIfFailed(Return returnCode)
    {
        if (returnCode == Return.Success)
        {
            return;
        }
        if (returnCode == Return.Outofmemory)
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