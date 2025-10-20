using System.Drawing;

namespace PetitMoteur3D.Window.SilkNet
{
    public static class SilkWindow
    {
        public static IWindow Create(WindowOptions options)
        {
            Silk.NET.Windowing.WindowOptions silkOptions = options.ToSilkNet();
            Silk.NET.Windowing.IWindow silkWindow = Silk.NET.Windowing.Window.Create(silkOptions);
            return new SilkWindowImpl(silkWindow);
        }
    }

    internal class SilkWindowImpl : IWindow, ISilkWindow
    {
        public Silk.NET.Windowing.IWindow SilkWindow { get; init; }

        public SilkWindowImpl(Silk.NET.Windowing.IWindow window)
        {
            ArgumentNullException.ThrowIfNull(window);
            SilkWindow = window;
        }

        /// <inheritdoc/>
        public nint? NativeHandle => SilkWindow.Native!.DXHandle;

        /// <inheritdoc/>
        public Size Size
        {
            get => new Size(SilkWindow.Size.X, SilkWindow.Size.Y);
            set => SilkWindow.Size = new Silk.NET.Maths.Vector2D<int>(value.Width, value.Height);
        }

        /// <inheritdoc/>
        public Size FramebufferSize
        {
            get => new Size(SilkWindow.FramebufferSize.X, SilkWindow.FramebufferSize.Y);
            set => SilkWindow.Size = new Silk.NET.Maths.Vector2D<int>(value.Width, value.Height);
        }

        /// <inheritdoc/>
        public bool IsClosing => SilkWindow.IsClosing;

        /// <inheritdoc/>
        public double Time => SilkWindow.Time;

        /// <inheritdoc/>
        public bool IsInitialized => SilkWindow.IsInitialized;

        /// <inheritdoc/>
        public event Action? Load
        {
            add => SilkWindow.Load += value;
            remove => SilkWindow.Load -= value;
        }

        /// <inheritdoc/>
        public event Action? Closing
        {
            add => SilkWindow.Closing += value;
            remove => SilkWindow.Closing -= value;
        }

        /// <inheritdoc/>
        public event Action<Size>? Resize
        {
            add { bool emptyAction = _resizeActions.Count == 0; _resizeActions.Add(value); if (emptyAction) SilkWindow.Resize += ResizeHandle; }
            remove { bool lastAction = _resizeActions.Count == 1; _resizeActions.Remove(value); if (lastAction) SilkWindow.Resize -= ResizeHandle; }
        }
        private List<Action<Size>?> _resizeActions = new();
        private void ResizeHandle(Silk.NET.Maths.Vector2D<int> x)
        {
            foreach (Action<Size>? action in _resizeActions)
            {
                action?.Invoke(new Size(x.X, x.Y));
            }
        }

        /// <inheritdoc/>
        public void Close()
        {
            SilkWindow.Close();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            SilkWindow.Dispose();
        }

        /// <inheritdoc/>
        public void Focus()
        {
            SilkWindow.Focus();
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            SilkWindow.Initialize();
        }

        /// <inheritdoc/>
        public void Run(Action onFrame, object? _ = null)
        {
            SilkWindow.Render += _ => onFrame?.Invoke();
            Silk.NET.Windowing.WindowExtensions.Run(SilkWindow);
        }
    }
}
