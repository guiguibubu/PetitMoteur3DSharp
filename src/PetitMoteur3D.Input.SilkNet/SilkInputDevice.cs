namespace PetitMoteur3D.Input.SilkNet
{
    public static class SilkInputDevice
    {
        internal static IInputDevice FromSilk(Silk.NET.Input.IInputDevice inputDevice)
        {
            if (inputDevice is Silk.NET.Input.IGamepad gamepad)
            {
                return FromSilk(gamepad);
            }
            else if (inputDevice is Silk.NET.Input.IJoystick joystick)
            {
                return FromSilk(joystick);
            }
            else if (inputDevice is Silk.NET.Input.IMouse mouse)
            {
                return FromSilk(mouse);
            }
            else if (inputDevice is Silk.NET.Input.IKeyboard keyboard)
            {
                return FromSilk(keyboard);
            }
            else
            {
                return new SilkInputDeviceImpl(inputDevice);
            }
        }

        public static IGamepad FromSilk(Silk.NET.Input.IGamepad inputDevice)
        {
            return new SilkGamepad(inputDevice);
        }

        public static IJoystick FromSilk(Silk.NET.Input.IJoystick inputDevice)
        {
            return new SilkJoystick(inputDevice);
        }

        public static IMouse FromSilk(Silk.NET.Input.IMouse inputDevice)
        {
            return new SilkMouse(inputDevice);
        }

        public static IKeyboard FromSilk(Silk.NET.Input.IKeyboard inputDevice)
        {
            return new SilkKeyboard(inputDevice);
        }
    }

    internal class SilkInputDeviceImpl : BaseSilkInputDeviceImpl<Silk.NET.Input.IInputDevice>
    {
        public SilkInputDeviceImpl(Silk.NET.Input.IInputDevice silkInputDevice) : base(silkInputDevice)
        {
        }
    }

    internal abstract class BaseSilkInputDeviceImpl<T> : IInputDevice where T : Silk.NET.Input.IInputDevice
    {
        protected T _silkInputDevice;

        public BaseSilkInputDeviceImpl(T silkInputDevice)
        {
            _silkInputDevice = silkInputDevice;
        }
        public string Name => _silkInputDevice.Name;

        public int Index => _silkInputDevice.Index;

        public bool IsConnected => _silkInputDevice.IsConnected;
    }
}
