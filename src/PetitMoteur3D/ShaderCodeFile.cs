using System;
using System.IO;

namespace PetitMoteur3D;

internal class ShaderCodeFile : IShaderFile
{
    public string Name { get; init; }
    public string FilePath { get; init; }
    public byte[] Data { get; init; }
    public string EntryPoint { get; init; }
    public string Target { get; init; }
    public uint CompilationFlags { get; init; }

    public ShaderCodeFile(string filePath, string entryPoint, string target, uint compilationFlags, string name = "")
    {
        ArgumentNullException.ThrowIfNullOrEmpty(filePath);
        Name = name;
        FilePath = filePath;
        Data = File.ReadAllBytes(filePath);
        EntryPoint = entryPoint;
        Target = target;
        CompilationFlags = compilationFlags;
    }
}
