namespace PetitMoteur3D.Input.SilkNet.Extensions
{
    internal static class AxisExtensions
    {
        public static Axis FromSilk(this Silk.NET.Input.Axis axis)
        {
            return new Axis(axis.Index, axis.Position);
        }

        public static Silk.NET.Input.Axis ToSilk(this Axis axis)
        {
            return new Silk.NET.Input.Axis(axis.Index, axis.Position);
        }
    }
}
