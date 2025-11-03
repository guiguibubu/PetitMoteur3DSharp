using System;
using System.IO;

namespace PetitMoteur3D;

internal class ShaderByteCodeFile : IShaderFile
{
    public string Name { get; }
    public string FilePath { get; }
    public byte[] Data { get; }

    public ShaderByteCodeFile(string filePath, string name = "")
    {
        ArgumentNullException.ThrowIfNullOrEmpty(filePath);
        Name = name;
        FilePath = filePath;
        Data = File.ReadAllBytes(filePath);
    }
}
