namespace PetitMoteur3D.Input.SilkNet.Extensions
{
    internal static class CursorTypeExtensions
    {
        public static CursorType FromSilk(this Silk.NET.Input.CursorType buttonName)
        {
            switch (buttonName)
            {
                case Silk.NET.Input.CursorType.Standard: return CursorType.Standard;
                case Silk.NET.Input.CursorType.Custom: return CursorType.Custom;
                default:
                    return CursorType.Standard;
            }
        }

        public static Silk.NET.Input.CursorType ToSilk(this CursorType buttonName)
        {
            switch (buttonName)
            {
                case CursorType.Standard: return Silk.NET.Input.CursorType.Standard;
                case CursorType.Custom: return Silk.NET.Input.CursorType.Custom;
                default:
                    return Silk.NET.Input.CursorType.Standard;
            }
        }
    }
}
