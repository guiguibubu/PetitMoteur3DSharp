namespace PetitMoteur3D.Input.SilkNet.Extensions
{
    internal static class DeadzoneMethodExtensions
    {
        public static DeadzoneMethod FromSilk(this Silk.NET.Input.DeadzoneMethod position)
        {
            switch (position)
            {
                case Silk.NET.Input.DeadzoneMethod.Traditional: return DeadzoneMethod.Traditional;
                case Silk.NET.Input.DeadzoneMethod.AdaptiveGradient: return DeadzoneMethod.AdaptiveGradient;
                default:
                    return DeadzoneMethod.Traditional;
            }
        }

        public static Silk.NET.Input.DeadzoneMethod ToSilk(this DeadzoneMethod position)
        {
            switch (position)
            {
                case DeadzoneMethod.Traditional: return Silk.NET.Input.DeadzoneMethod.Traditional;
                case DeadzoneMethod.AdaptiveGradient: return Silk.NET.Input.DeadzoneMethod.AdaptiveGradient;
                default:
                    return Silk.NET.Input.DeadzoneMethod.Traditional;
            }
        }
    }
}
