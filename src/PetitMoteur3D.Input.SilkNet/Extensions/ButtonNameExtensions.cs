namespace PetitMoteur3D.Input.SilkNet.Extensions
{
    internal static class ButtonNameExtensions
    {
        public static ButtonName FromSilk(this Silk.NET.Input.ButtonName buttonName)
        {
            switch (buttonName)
            {
                case Silk.NET.Input.ButtonName.Unknown: return ButtonName.Unknown;
                case Silk.NET.Input.ButtonName.A: return ButtonName.A;
                case Silk.NET.Input.ButtonName.B: return ButtonName.B;
                case Silk.NET.Input.ButtonName.X: return ButtonName.X;
                case Silk.NET.Input.ButtonName.Y: return ButtonName.Y;
                case Silk.NET.Input.ButtonName.LeftBumper: return ButtonName.LeftBumper;
                case Silk.NET.Input.ButtonName.RightBumper: return ButtonName.RightBumper;
                case Silk.NET.Input.ButtonName.Back: return ButtonName.Back;
                case Silk.NET.Input.ButtonName.Start: return ButtonName.Start;
                case Silk.NET.Input.ButtonName.Home: return ButtonName.Home;
                case Silk.NET.Input.ButtonName.LeftStick: return ButtonName.LeftStick;
                case Silk.NET.Input.ButtonName.RightStick: return ButtonName.RightStick;
                case Silk.NET.Input.ButtonName.DPadUp: return ButtonName.DPadUp;
                case Silk.NET.Input.ButtonName.DPadRight: return ButtonName.DPadRight;
                case Silk.NET.Input.ButtonName.DPadDown: return ButtonName.DPadDown;
                case Silk.NET.Input.ButtonName.DPadLeft: return ButtonName.DPadLeft;
                default:
                    return ButtonName.Unknown;
            }
        }

        public static Silk.NET.Input.ButtonName ToSilk(this ButtonName buttonName)
        {
            switch (buttonName)
            {
                case ButtonName.Unknown: return Silk.NET.Input.ButtonName.Unknown;
                case ButtonName.A: return Silk.NET.Input.ButtonName.A;
                case ButtonName.B: return Silk.NET.Input.ButtonName.B;
                case ButtonName.X: return Silk.NET.Input.ButtonName.X;
                case ButtonName.Y: return Silk.NET.Input.ButtonName.Y;
                case ButtonName.LeftBumper: return Silk.NET.Input.ButtonName.LeftBumper;
                case ButtonName.RightBumper: return Silk.NET.Input.ButtonName.RightBumper;
                case ButtonName.Back: return Silk.NET.Input.ButtonName.Back;
                case ButtonName.Start: return Silk.NET.Input.ButtonName.Start;
                case ButtonName.Home: return Silk.NET.Input.ButtonName.Home;
                case ButtonName.LeftStick: return Silk.NET.Input.ButtonName.LeftStick;
                case ButtonName.RightStick: return Silk.NET.Input.ButtonName.RightStick;
                case ButtonName.DPadUp: return Silk.NET.Input.ButtonName.DPadUp;
                case ButtonName.DPadRight: return Silk.NET.Input.ButtonName.DPadRight;
                case ButtonName.DPadDown: return Silk.NET.Input.ButtonName.DPadDown;
                case ButtonName.DPadLeft: return Silk.NET.Input.ButtonName.DPadLeft;
                default:
                    return Silk.NET.Input.ButtonName.Unknown;
            }
        }
    }
}
