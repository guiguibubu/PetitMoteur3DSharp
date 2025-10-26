using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Input;
using PetitMoteur3D.Input.WinUI.Extensions;
using PetitMoteur3D.Window.WinUI;
using Windows.Win32;

namespace PetitMoteur3D.Input.WinUI;

internal class WinUiMouse : IMouse
{
    private readonly InputPointerSource _inputPointerSource;
    private readonly DXPanel _dxPanel;
    public WinUiMouse(WinUIWindow window, string? name = null)
    {
        _inputPointerSource = window.InputPointerSource;
        _dxPanel = window.DirectXPanel;
        Name = name ?? "PetitMoteur3D - WinUiMouse";
    }

    #region IInputDevice implementation

    public string Name { get; }

    public bool IsConnected => IsMouseConnected();
    private static bool IsMouseConnected()
    {
        var keyboardCapabilities = new Windows.Devices.Input.MouseCapabilities();
        return keyboardCapabilities.MousePresent != 0;
    }
    #endregion

    public IReadOnlyList<MouseButton> SupportedButtons => _availableButtons;

    private ScrollWheel[] _scrollWheels = new ScrollWheel[1];
    public IReadOnlyList<ScrollWheel> ScrollWheels => _scrollWheels;

    private Vector2 _position = Vector2.Zero;
    public Vector2 Position { get => _position; set => _position = value; }

    public ICursor Cursor => new WinUiCursor(_dxPanel.GetCursor());

    public int DoubleClickTime { get => (int)PInvoke.GetDoubleClickTime(); set => PInvoke.SetDoubleClickTime((uint)value); }
    public int DoubleClickRange { get => 0; set { } }

    public event Action<IMouse, MouseButton> MouseDown
    {
        add { bool emptyAction = _mouseDownActions.Count == 0; _mouseDownActions.Add(value); if (emptyAction) _dxPanel.PointerPressed += OnMouseDown; }
        remove { bool lastAction = _mouseDownActions.Count == 1; _mouseDownActions.Remove(value); if (lastAction) _dxPanel.PointerPressed -= OnMouseDown; }
    }
    private List<Action<IMouse, MouseButton>?> _mouseDownActions = new();
    private void OnMouseDown(object sender, PointerRoutedEventArgs e)
    {
        PointerPoint pointerPoint = e.GetCurrentPoint(_dxPanel);
        if (pointerPoint.PointerDeviceType != PointerDeviceType.Mouse)
        {
            return;
        }
        MouseButton button = pointerPoint.Properties.PointerUpdateKind.FromWinUi();
        _btnPressedCache.AddOrUpdate(button, true, (MouseButton _, bool _) => true);
        foreach (Action<IMouse, MouseButton>? action in _mouseDownActions)
        {
            action?.Invoke(this, button);
        }
        e.Handled = true;
    }
    public event Action<IMouse, MouseButton> MouseUp
    {
        add { bool emptyAction = _mouseUpActions.Count == 0; _mouseUpActions.Add(value); if (emptyAction) _dxPanel.PointerReleased += OnMouseUp; }
        remove { bool lastAction = _mouseUpActions.Count == 1; _mouseUpActions.Remove(value); if (lastAction) _dxPanel.PointerReleased -= OnMouseUp; }
    }
    private List<Action<IMouse, MouseButton>?> _mouseUpActions = new();
    private void OnMouseUp(object sender, PointerRoutedEventArgs e)
    {
        PointerPoint pointerPoint = e.GetCurrentPoint(_dxPanel);
        if (pointerPoint.PointerDeviceType != PointerDeviceType.Mouse)
        {
            return;
        }
        MouseButton button = pointerPoint.Properties.PointerUpdateKind.FromWinUi();
        _btnPressedCache.AddOrUpdate(button, true, (MouseButton _, bool _) => false);
        foreach (Action<IMouse, MouseButton>? action in _mouseUpActions)
        {
            action?.Invoke(this, button);
        }
        e.Handled = true;
    }
    public event Action<IMouse, MouseButton, Vector2> Click
    {
        add { bool emptyAction = _clickActions.Count == 0; _clickActions.Add(value); if (emptyAction) { _dxPanel.Tapped += OnClickLeft; _dxPanel.RightTapped += OnClickRight; } }
        remove { bool lastAction = _clickActions.Count == 1; _clickActions.Remove(value); if (lastAction) { _dxPanel.Tapped -= OnClickLeft; _dxPanel.RightTapped -= OnClickRight; } }
    }
    private List<Action<IMouse, MouseButton, Vector2>?> _clickActions = new();
    private void OnClickLeft(object sender, TappedRoutedEventArgs e)
    {
        if (e.PointerDeviceType != PointerDeviceType.Mouse)
        {
            return;
        }
        foreach (Action<IMouse, MouseButton, Vector2>? action in _clickActions)
        {
            action?.Invoke(this, MouseButton.Left, e.GetPosition(_dxPanel).ToVector2());
        }
        e.Handled = true;
    }
    private void OnClickRight(object sender, RightTappedRoutedEventArgs e)
    {
        if (e.PointerDeviceType != PointerDeviceType.Mouse)
        {
            return;
        }
        foreach (Action<IMouse, MouseButton, Vector2>? action in _clickActions)
        {
            action?.Invoke(this, MouseButton.Right, e.GetPosition(_dxPanel).ToVector2());
        }
        e.Handled = true;
    }
    public event Action<IMouse, MouseButton, Vector2> DoubleClick
    {
        add { bool emptyAction = _doubleClickActions.Count == 0; _doubleClickActions.Add(value); if (emptyAction) _dxPanel.DoubleTapped += OnDoubleClick; }
        remove { bool lastAction = _doubleClickActions.Count == 1; _doubleClickActions.Remove(value); if (lastAction) _dxPanel.DoubleTapped -= OnDoubleClick; }
    }
    private List<Action<IMouse, MouseButton, Vector2>?> _doubleClickActions = new();
    private void OnDoubleClick(object sender, DoubleTappedRoutedEventArgs e)
    {
        if (e.PointerDeviceType != PointerDeviceType.Mouse)
        {
            return;
        }
        foreach (Action<IMouse, MouseButton, Vector2>? action in _doubleClickActions)
        {
            action?.Invoke(this, MouseButton.Left, e.GetPosition(_dxPanel).ToVector2());
        }
        e.Handled = true;
    }
    public event Action<IMouse, Vector2> MouseMove
    {
        add { bool emptyAction = _mouseMoveActions.Count == 0; _mouseMoveActions.Add(value); if (emptyAction) _dxPanel.PointerMoved += OnMouseMove; }
        remove { bool lastAction = _mouseMoveActions.Count == 1; _mouseMoveActions.Remove(value); if (lastAction) _dxPanel.PointerMoved -= OnMouseMove; }
    }
    private List<Action<IMouse, Vector2>?> _mouseMoveActions = new();
    private void OnMouseMove(object sender, PointerRoutedEventArgs e)
    {
        PointerPoint pointerPoint = e.GetCurrentPoint(_dxPanel);
        if (pointerPoint.PointerDeviceType != PointerDeviceType.Mouse)
        {
            return;
        }
        _position = pointerPoint.Position.ToVector2();
        foreach (Action<IMouse, Vector2>? action in _mouseMoveActions)
        {
            action?.Invoke(this, _position);
        }
        e.Handled = true;
    }
    public event Action<IMouse, ScrollWheel> Scroll
    {
        add { bool emptyAction = _scrollActions.Count == 0; _scrollActions.Add(value); if (emptyAction) _dxPanel.PointerWheelChanged += OnScroll; }
        remove { bool lastAction = _scrollActions.Count == 1; _scrollActions.Remove(value); if (lastAction) _dxPanel.PointerWheelChanged -= OnScroll; }
    }
    private List<Action<IMouse, ScrollWheel>?> _scrollActions = new();
    private void OnScroll(object sender, PointerRoutedEventArgs e)
    {
        PointerPoint pointerPoint = e.GetCurrentPoint(_dxPanel);
        if (pointerPoint.PointerDeviceType != PointerDeviceType.Mouse)
        {
            return;
        }
        ScrollWheel scrollWheel = new ScrollWheel(x: 0, y: pointerPoint.Properties.MouseWheelDelta);
        _scrollWheels[0] = scrollWheel;
        foreach (Action<IMouse, ScrollWheel>? action in _scrollActions)
        {
            action?.Invoke(this, scrollWheel);
        }
        e.Handled = true;
    }

    ConcurrentDictionary<MouseButton, bool> _btnPressedCache = new();
    public bool IsButtonPressed(MouseButton btn)
    {
        return _btnPressedCache.GetOrAdd(btn, false);
    }

    private static readonly IReadOnlyList<Microsoft.UI.Input.PointerUpdateKind> _winUiMouseActions = new List<Microsoft.UI.Input.PointerUpdateKind>()
    {
        Microsoft.UI.Input.PointerUpdateKind.Other,
        Microsoft.UI.Input.PointerUpdateKind.LeftButtonPressed,
        Microsoft.UI.Input.PointerUpdateKind.LeftButtonReleased,
        Microsoft.UI.Input.PointerUpdateKind.RightButtonPressed,
        Microsoft.UI.Input.PointerUpdateKind.RightButtonReleased,
        Microsoft.UI.Input.PointerUpdateKind.MiddleButtonPressed,
        Microsoft.UI.Input.PointerUpdateKind.MiddleButtonReleased,
        Microsoft.UI.Input.PointerUpdateKind.XButton1Pressed,
        Microsoft.UI.Input.PointerUpdateKind.XButton1Released,
        Microsoft.UI.Input.PointerUpdateKind.XButton2Pressed,
        Microsoft.UI.Input.PointerUpdateKind.XButton2Released
    };

    private static readonly IReadOnlyList<MouseButton> _availableButtons = _winUiMouseActions.Select(k => k.FromWinUi()).Distinct().ToArray();
}
