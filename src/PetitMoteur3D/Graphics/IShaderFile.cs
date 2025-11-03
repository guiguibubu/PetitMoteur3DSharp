namespace PetitMoteur3D.Graphics;

internal interface IShaderFile
{
    public string Name { get; }
    public string FilePath { get; }
    public byte[] Data { get; }
}
