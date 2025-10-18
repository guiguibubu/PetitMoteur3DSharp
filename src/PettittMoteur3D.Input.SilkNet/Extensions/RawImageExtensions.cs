namespace PetitMoteur3D.Input.SilkNet.Extensions;

internal static class RawImageExtensions
{
    public static RawImage FromSilk(this Silk.NET.Core.RawImage image)
    {
        return new RawImage(image.Width, image.Height, image.Pixels);
    }

    public static Silk.NET.Core.RawImage ToSilk(this RawImage image)
    {
        return new Silk.NET.Core.RawImage(image.Width, image.Height, image.Pixels);
    }
}
