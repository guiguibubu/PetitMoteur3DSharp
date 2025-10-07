using System;
using System.Collections.Generic;
using System.Numerics;

namespace PetitMoteur3D.Window
{
    internal interface ISilkWindow
    {
        Silk.NET.Windowing.IWindow SilkWindow { get; }
    }

    internal class SilkWindowImpl : IWindow, ISilkWindow
    {
        public Silk.NET.Windowing.IWindow SilkWindow { get; init; }

        public SilkWindowImpl(Silk.NET.Windowing.IWindow window)
        {
            SilkWindow = window;
        }

        /// <inheritdoc/>
        public nint? NativeHandle => SilkWindow.Native!.DXHandle;

        /// <inheritdoc/>
        public Vector2 Size
        {
            get => new Vector2(SilkWindow.Size.X, SilkWindow.Size.Y);
            set => SilkWindow.Size = new Silk.NET.Maths.Vector2D<int>((int)value.X, (int)value.Y);
        }

        /// <inheritdoc/>
        public Vector2 FramebufferSize
        {
            get => new Vector2(SilkWindow.FramebufferSize.X, SilkWindow.FramebufferSize.Y);
            set => SilkWindow.Size = new Silk.NET.Maths.Vector2D<int>((int)value.X, (int)value.Y);
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
        public event Action<Vector2>? Resize
        {
            add { bool emptyAction = _resizeActions.Count == 0; _resizeActions.Add(value); if (emptyAction) SilkWindow.Resize += ResizeHandle; }
            remove { bool lastAction = _resizeActions.Count == 1; _resizeActions.Remove(value); if (lastAction) SilkWindow.Resize -= ResizeHandle; }
        }
        private List<Action<Vector2>?> _resizeActions = new();
        private void ResizeHandle(Silk.NET.Maths.Vector2D<int> x)
        {
            foreach (Action<Vector2>? action in _resizeActions)
            {
                action?.Invoke(new Vector2(x.X, x.Y));
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
        public void Run(Action onFrame)
        {
            SilkWindow.Render += _ => onFrame?.Invoke();
            Silk.NET.Windowing.WindowExtensions.Run(SilkWindow);
        }
    }
}
