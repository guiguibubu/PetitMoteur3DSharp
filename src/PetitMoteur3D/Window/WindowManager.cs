using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace PetitMoteur3D.Window
{
    internal class WindowManager
    {
        private WindowManager() { }
        public static IWindow Create()
        {
            WindowOptions options = WindowOptions.Default;
            options.WindowState = WindowState.Maximized;
            options.Title = "PetitMoteur3D";
            options.API = GraphicsAPI.None; // <-- This bit is important, as your window will be configured for OpenGL by default.
            return Silk.NET.Windowing.Window.Create(options);
        }
    }
}
