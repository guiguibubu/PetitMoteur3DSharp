using System.Numerics;

namespace PetitMoteur3D.Window
{
    public class WinUIWindow : IWindow
    {
        private nint _nativeHandle;

        public WinUIWindow(nint nativeHandle)
        {
            _nativeHandle = nativeHandle;
        }

        /// <inheritdoc/>
        public nint? NativeHandle => _nativeHandle;

        /// <inheritdoc/>
        public Vector2 Size
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Vector2 FramebufferSize
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool IsClosing => throw new NotImplementedException();

        /// <inheritdoc/>
        public double Time => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool IsInitialized => throw new NotImplementedException();

        /// <inheritdoc/>
        public event Action? Load
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public event Action? Closing
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public event Action<Vector2>? Resize
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        /// <inheritdoc/>
        public void Close()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Focus()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Run(Action onFrame)
        {
            throw new NotImplementedException();
        }
    }
}
