using System;
using System.Runtime.InteropServices;

namespace PetitMoteur3D.Window
{
    internal class WindowManager
    {
        private WindowManager() { }

        public static IWindow Create(bool allowSilkWindow = true)
        {
            WindowOptions options = WindowOptions.Default;
            options.WindowState = WindowState.Maximized;
            options.Title = "PetitMoteur3D";
            return Create(options, allowSilkWindow);
        }

        public static IWindow Create(WindowOptions options, bool allowSilkWindow = true)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!allowSilkWindow)
                {
                    throw new NotImplementedException("Win32 window support not implemented yet");
                }
                Silk.NET.Windowing.WindowOptions silkOptions = options.ToSilkNet();
                return new SilkWindowImpl(Silk.NET.Windowing.Window.Create(silkOptions));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Silk.NET.Windowing.WindowOptions silkOptions = options.ToSilkNet();
                return new SilkWindowImpl(Silk.NET.Windowing.Window.Create(silkOptions));
            }
            else
            {
                throw new PlatformNotSupportedException("Support only for windows and Linux");
            }
        }
    }
}
