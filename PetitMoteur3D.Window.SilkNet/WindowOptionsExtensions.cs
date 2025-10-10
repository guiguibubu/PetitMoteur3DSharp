namespace PetitMoteur3D.Window.SilkNet
{
    internal static class WindowOptionsExtensions
    {
        public static Silk.NET.Windowing.WindowOptions ToSilkNet(this WindowOptions options)
        {
            Silk.NET.Windowing.WindowOptions silkOptions = new()
            {
                IsVisible = options.IsVisible,
                Position = new((int)System.Math.Round(options.Position.X), (int)System.Math.Round(options.Position.Y)),
                Size = new((int)System.Math.Round(options.Size.X), (int)System.Math.Round(options.Size.Y)),
                Title = options.Title,
                WindowState = options.WindowState.ToSilkNet(),
                WindowBorder = options.WindowBorder.ToSilkNet(),
                TransparentFramebuffer = options.TransparentFramebuffer,
                TopMost = options.TopMost,
                API = Silk.NET.Windowing.GraphicsAPI.None // <-- This bit is important, as your window will be configured for OpenGL by default.
            };
            return silkOptions;
        }

        public static Silk.NET.Windowing.WindowState ToSilkNet(this WindowState state)
        {
            switch (state)
            {
                case WindowState.Normal:
                    return Silk.NET.Windowing.WindowState.Normal;
                case WindowState.Minimized:
                    return Silk.NET.Windowing.WindowState.Minimized;
                case WindowState.Maximized:
                    return Silk.NET.Windowing.WindowState.Maximized;
                case WindowState.Fullscreen:
                    return Silk.NET.Windowing.WindowState.Fullscreen;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), string.Format("State not supported in Silk.Net : {State}", state.ToString()));
            }
        }

        public static Silk.NET.Windowing.WindowBorder ToSilkNet(this WindowBorder border)
        {
            switch (border)
            {
                case WindowBorder.Resizable:
                    return Silk.NET.Windowing.WindowBorder.Resizable;
                case WindowBorder.Fixed:
                    return Silk.NET.Windowing.WindowBorder.Fixed;
                case WindowBorder.Hidden:
                    return Silk.NET.Windowing.WindowBorder.Hidden;
                default:
                    throw new ArgumentOutOfRangeException(nameof(border), string.Format("Border not supported in Silk.Net : {Border}", border.ToString()));
            }
        }
    }
}
