using System.Drawing;

namespace PetitMoteur3D.Window
{
    public class WinUIWindow : IWindow, IWinUiWindow
    {
        private readonly Microsoft.UI.Windowing.AppWindow _nativeWindow;
        private readonly Microsoft.UI.Xaml.Window _window;
        private readonly Microsoft.UI.Xaml.Controls.SwapChainPanel _swapchainPanel;

        public WinUIWindow(Microsoft.UI.Xaml.Window window, Microsoft.UI.Xaml.Controls.SwapChainPanel swapchainPanel)
        {
            _window = window;
            _nativeWindow = window.AppWindow;
            _swapchainPanel = swapchainPanel;
            _nativeWindow.Closing += OnClosing;
            _isInitialized = true;
        }

        /// <inheritdoc/>
        public nint? NativeHandle => nint.CreateChecked(_nativeWindow.Id.Value);

        /// <inheritdoc/>
        public Size Size
        {
            get => new Size(_nativeWindow.Size.Width, _nativeWindow.Size.Height);
            set => _nativeWindow.Resize(new Windows.Graphics.SizeInt32(value.Width, value.Height));
        }

        /// <inheritdoc/>
        public Size FramebufferSize
        {
            get => new Size(_nativeWindow.ClientSize.Width, _nativeWindow.ClientSize.Height);
            set => _nativeWindow.ResizeClient(new Windows.Graphics.SizeInt32(value.Width, value.Height));
        }

        /// <inheritdoc/>
        public bool IsClosing => _idClosing;
        private bool _idClosing = false;

        /// <inheritdoc/>
        public double Time => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool IsInitialized => _isInitialized;
        private bool _isInitialized = false;

        /// <inheritdoc/>
        public event Action? Load
        {
            add { bool emptyAction = _loadActions.Count == 0; _loadActions.Add(value); if (emptyAction) _swapchainPanel.Loaded += OnLoad; }
            remove { bool lastAction = _loadActions.Count == 1; _loadActions.Remove(value); if (lastAction) _swapchainPanel.Loaded -= OnLoad; }
        }
        private List<Action?> _loadActions = new();
        private void OnLoad(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            foreach (Action? action in _loadActions)
            {
                action?.Invoke();
            }
        }

        /// <inheritdoc/>
        public event Action? Closing
        {
            add { bool emptyAction = _closingActions.Count == 0; _closingActions.Add(value); if (emptyAction) _nativeWindow.Destroying += OnClosing; }
            remove { bool lastAction = _closingActions.Count == 1; _closingActions.Remove(value); if (lastAction) _nativeWindow.Destroying -= OnClosing; }
        }
        private List<Action?> _closingActions = new();
        private void OnClosing(Microsoft.UI.Windowing.AppWindow sender, object args)
        {
            _idClosing = true;
            foreach (Action? action in _closingActions)
            {
                action?.Invoke();
            }
        }

        /// <inheritdoc/>
        public event Action<Size>? Resize
        {
            add { bool emptyAction = _resizeActions.Count == 0; _resizeActions.Add(value); if (emptyAction) _nativeWindow.Changed += OnResize; }
            remove { bool lastAction = _resizeActions.Count == 1; _resizeActions.Remove(value); if (lastAction) _nativeWindow.Changed -= OnResize; }
        }
        private List<Action<Size>?> _resizeActions = new();
        private void OnResize(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowChangedEventArgs args)
        {
            if (!args.DidSizeChange)
            {
                return;
            }
            foreach (Action<Size>? action in _resizeActions)
            {
                action?.Invoke(new Size(_nativeWindow.Size.Width, _nativeWindow.Size.Height));
            }
        }

        /// <inheritdoc/>
        public void Close()
        {
            _nativeWindow.Destroy();
        }

        /// <inheritdoc/>
        public void Dispose()
        {

        }

        /// <inheritdoc/>
        public void Focus()
        {
            _nativeWindow.Show();
        }

        /// <inheritdoc/>
        public void Initialize()
        {
            _nativeWindow.Show();
        }

        /// <inheritdoc/>
        public void Run(Action onFrame)
        {
            while (!IsClosing)
            {
                _swapchainPanel.DispatcherQueue.TryEnqueue(onFrame.Invoke);
            }
        }
    }
}
