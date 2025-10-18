namespace PetitMoteur3D.Input.SilkNet.Extensions
{
    internal static class DeadzoneExtensions
    {
        public static Deadzone FromSilk(this Silk.NET.Input.Deadzone deadzone)
        {
            return new Deadzone(deadzone.Value, deadzone.Method.FromSilk());
        }

        public static Silk.NET.Input.Deadzone ToSilk(this Deadzone deadzone)
        {
            return new Silk.NET.Input.Deadzone(deadzone.Value, deadzone.Method.ToSilk());
        }
    }
}
