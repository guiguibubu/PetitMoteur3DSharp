using System.Drawing;
using System.Xml.Schema;

namespace PetitMoteur3D.Window
{
    public interface IWindow : IDisposable
    {
        /// <summary>
        /// The underlying native window handle
        /// </summary>
        nint? NativeHandle { get; }

        /// <summary>
        /// The size of the window in pixels.
        /// </summary>
        Size Size { get; set; }

        /// <summary>
        /// The size of the framebuffer. May differ from the window size.
        /// </summary>
        Size FramebufferSize { get; set; }

        /// <summary>
        /// Determines whether the underlying platform has requested the window to close.
        /// </summary>
        bool IsClosing { get; }

        /// <summary>
        /// Elapsed time in seconds since the View was initialized.
        /// </summary>
        double Time { get; }

        /// <summary>
        /// Determines if the window is initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Raised when the window is about to close.
        /// </summary>
        event Action? Closing;

        /// <summary>
        /// Raised when the window is resized.
        /// </summary>
        event Action<Size>? Resize;

        /// <summary>
        /// Creates the window on the underlying platform.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Sets focus to current window.
        /// </summary>
        void Focus();

        /// <summary>
        /// Close this window.
        /// </summary>
        void Close();

        /// <summary>
        /// Initiates a render loop in which the given callback is called as fast as the underlying platform can manage.
        /// </summary>
        /// <param name="onFrame">The callback to run each frame.</param>
        /// <param name="frameArgs">The arguments for the frame.</param>
        void Run(Action onFrame, object? frameArgs = null);
    }
}
