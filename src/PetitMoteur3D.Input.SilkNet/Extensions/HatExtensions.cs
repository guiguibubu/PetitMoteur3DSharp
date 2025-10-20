namespace PetitMoteur3D.Input.SilkNet.Extensions
{
    internal static class HatExtensions
    {
        public static Hat FromSilk(this Silk.NET.Input.Hat hat)
        {
            return new Hat(hat.Index, hat.Position.FromSilk());
        }

        public static Silk.NET.Input.Hat ToSilk(this Hat hat)
        {
            return new Silk.NET.Input.Hat(hat.Index, hat.Position.ToSilk());
        }
    }
}
