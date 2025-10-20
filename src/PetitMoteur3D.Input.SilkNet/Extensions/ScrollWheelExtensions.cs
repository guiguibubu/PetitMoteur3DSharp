namespace PetitMoteur3D.Input.SilkNet.Extensions
{
    internal static class ScrollWheelExtensions
    {
        public static ScrollWheel FromSilk(this Silk.NET.Input.ScrollWheel wheel)
        {
            return new ScrollWheel(wheel.X, wheel.Y);
        }

        public static Silk.NET.Input.ScrollWheel ToSilk(this ScrollWheel wheel)
        {
            return new Silk.NET.Input.ScrollWheel(wheel.X, wheel.Y);
        }
    }
}
