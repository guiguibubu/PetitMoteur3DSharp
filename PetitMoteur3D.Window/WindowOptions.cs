using System;
using System.Numerics;

namespace PetitMoteur3D.Window
{
    public struct WindowOptions
    {
        /// <summary>
        /// Whether or not the window is visible.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// The position of the window. If set to -1, use the backend default.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// The size of the window in pixels.
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// The window title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The window state.
        /// </summary>
        public WindowState WindowState { get; set; }

        /// <summary>
        /// The window border.
        /// </summary>
        public WindowBorder WindowBorder { get; set; }

        /// <summary>
        /// Whether or not the window's framebuffer should be transparent.
        /// </summary>
        public bool TransparentFramebuffer { get; init; }

        /// <summary>
        /// Whether or not the window will be on the top of all the other windows.
        /// </summary>
        public bool TopMost { get; set; }

        static WindowOptions()
        {
            string name = "Window Title";
            try
            {
                string? asmName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
                if (asmName is not null)
                    name = asmName;
            }
            catch { /* cannot use reflection */ }

            Default = new WindowOptions()
            {
                IsVisible = true,
                Position = new Vector2(50, 50),
                Size = new Vector2(1280, 720),
                Title = name,
                WindowBorder = WindowBorder.Resizable,
                WindowState = WindowState.Normal,
                TransparentFramebuffer = false,
                TopMost = false,
            };
        }

        /// <summary>
        /// Convenience wrapper around creating a new WindowProperties with sensible defaults.
        /// </summary>
        public static WindowOptions Default { get; }
    }

    /// <summary>
    /// Represents the current state of the window.
    /// </summary>
    public enum WindowState
    {
        /// <summary>
        /// The window is in its regular configuration.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// The window has been minimized to the task bar.
        /// </summary>
        Minimized,

        /// <summary>
        /// The window has been maximized, covering the entire desktop, but not the taskbar.
        /// </summary>
        Maximized,

        /// <summary>
        /// The window has been fullscreened, covering the entire surface of the monitor.
        /// </summary>
        Fullscreen
    }

    /// <summary>
    /// Represents the window border.
    /// </summary>
    public enum WindowBorder
    {
        /// <summary>
        /// The window can be resized by clicking and dragging its border.
        /// </summary>
        Resizable = 0,

        /// <summary>
        /// The window border is visible, but cannot be resized. All window-resizings must happen solely in the code.
        /// </summary>
        Fixed,

        /// <summary>
        /// The window border is hidden.
        /// </summary>
        Hidden
    }
}
