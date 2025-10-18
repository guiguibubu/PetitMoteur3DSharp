namespace PetitMoteur3D.Input.SilkNet.Extensions
{
    internal static class ThumbstickExtensions
    {
        public static Thumbstick FromSilk(this Silk.NET.Input.Thumbstick stick)
        {
            return new Thumbstick(stick.Index, stick.X, stick.Y);
        }

        public static Silk.NET.Input.Thumbstick ToSilk(this Thumbstick stick)
        {
            return new Silk.NET.Input.Thumbstick(stick.Index, stick.X, stick.Y);
        }
    }
}
