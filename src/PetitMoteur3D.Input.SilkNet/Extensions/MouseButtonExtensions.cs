namespace PetitMoteur3D.Input.SilkNet.Extensions
{
    internal static class MouseButtonExtensions
    {
        public static MouseButton FromSilk(this Silk.NET.Input.MouseButton buttonName)
        {
            switch (buttonName)
            {
                case Silk.NET.Input.MouseButton.Unknown: return MouseButton.Unknown;
                case Silk.NET.Input.MouseButton.Left: return MouseButton.Left;
                case Silk.NET.Input.MouseButton.Middle: return MouseButton.Middle;
                case Silk.NET.Input.MouseButton.Button4: return MouseButton.Button4;
                case Silk.NET.Input.MouseButton.Button5: return MouseButton.Button5;
                case Silk.NET.Input.MouseButton.Button6: return MouseButton.Button6;
                case Silk.NET.Input.MouseButton.Button7: return MouseButton.Button7;
                case Silk.NET.Input.MouseButton.Button8: return MouseButton.Button8;
                case Silk.NET.Input.MouseButton.Button9: return MouseButton.Button9;
                case Silk.NET.Input.MouseButton.Button10: return MouseButton.Button10;
                case Silk.NET.Input.MouseButton.Button11: return MouseButton.Button11;
                case Silk.NET.Input.MouseButton.Button12: return MouseButton.Button12;
                default:
                    return MouseButton.Unknown;
            }
        }

        public static Silk.NET.Input.MouseButton ToSilk(this MouseButton buttonName)
        {
            switch (buttonName)
            {
                case MouseButton.Unknown: return Silk.NET.Input.MouseButton.Unknown;
                case MouseButton.Left: return Silk.NET.Input.MouseButton.Left;
                case MouseButton.Middle: return Silk.NET.Input.MouseButton.Middle;
                case MouseButton.Button4: return Silk.NET.Input.MouseButton.Button4;
                case MouseButton.Button5: return Silk.NET.Input.MouseButton.Button5;
                case MouseButton.Button6: return Silk.NET.Input.MouseButton.Button6;
                case MouseButton.Button7: return Silk.NET.Input.MouseButton.Button7;
                case MouseButton.Button8: return Silk.NET.Input.MouseButton.Button8;
                case MouseButton.Button9: return Silk.NET.Input.MouseButton.Button9;
                case MouseButton.Button10: return Silk.NET.Input.MouseButton.Button10;
                case MouseButton.Button11: return Silk.NET.Input.MouseButton.Button11;
                case MouseButton.Button12: return Silk.NET.Input.MouseButton.Button12;
                default:
                    return Silk.NET.Input.MouseButton.Unknown;
            }
        }
    }
}
