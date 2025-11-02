using System.Drawing;

namespace PetitMoteur3D.Window.WinUI;

public class WinUIWindow : IWindow, ICompositionWindow
{
    private readonly Microsoft.UI.Windowing.AppWindow _nativeWindow;
    private readonly DXPanel _dxPanel;

    public WinUIWindow(Microsoft.UI.Windowing.AppWindow window, DXPanel dxPanel)
    {
        _nativeWindow = window;
        _dxPanel = dxPanel;
        _nativeWindow.Closing += OnClosing;
        _isInitialized = true;
    }

    /// <inheritdoc/>
    public nint? NativeHandle => nint.CreateChecked(_nativeWindow.Id.Value);

    public DXPanel DirectXPanel => _dxPanel;

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
        add { bool emptyAction = _loadActions.Count == 0; _loadActions.Add(value); if (emptyAction) _dxPanel.Loaded += OnLoad; }
        remove { bool lastAction = _loadActions.Count == 1; _loadActions.Remove(value); if (lastAction) _dxPanel.Loaded -= OnLoad; }
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
        add { bool emptyAction = _resizeActions.Count == 0; _resizeActions.Add(value); if (emptyAction) _dxPanel.SizeChanged += OnResizeSwapchain; }
        remove { bool lastAction = _resizeActions.Count == 1; _resizeActions.Remove(value); if (lastAction) _dxPanel.SizeChanged -= OnResizeSwapchain; }
    }
    private List<Action<Size>?> _resizeActions = new();
    private void OnResizeSwapchain(object? sender, Microsoft.UI.Xaml.SizeChangedEventArgs args)
    {
        foreach (Action<Size>? action in _resizeActions)
        {
            action?.Invoke(new Size((int)args.NewSize.Width, (int)args.NewSize.Height));
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

    private ulong _lastFramePushed = 0;
    /// <inheritdoc/>
    public void Run(Action onFrame, object? frameArgs = null)
    {
        Engine? engine = null;
        if (frameArgs is not null && frameArgs is Engine engineArg)
        {
            engine = engineArg;
        }
        bool runFrame = true;
        _lastFramePushed = 0;
        while (!IsClosing)
        {
            if (engine is not null)
            {
                // We push in dispatccher queue only 5 frames in advance
                runFrame = engine.CurrentFrameCount - _lastFramePushed < 5;
            }
            if (runFrame)
            {
                _lastFramePushed++;
                //LogHelper.Log($"[PetitMoteur3D] WinUiWindow Run called runFrame {_lastFramePushed}");
                //LogHelper.Log($"[PetitMoteur3D] WinUiWindow Run Ennqueue frame");
                _dxPanel.SwapChainPanel.DispatcherQueue.TryEnqueue(onFrame.Invoke);
                //_inputPointerSource?.DispatcherQueue.TryEnqueue(onFrame.Invoke);
                //LogHelper.Log($"[PetitMoteur3D] WinUiWindow Run Ennqueue frame finished");
            }
        }
    }
}
