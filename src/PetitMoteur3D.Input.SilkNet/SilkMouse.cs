using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using PetitMoteur3D.Input.SilkNet.Extensions;

namespace PetitMoteur3D.Input.SilkNet;

internal class SilkMouse : BaseSilkInputDeviceImpl<Silk.NET.Input.IMouse>, IMouse
{
    public SilkMouse(Silk.NET.Input.IMouse silkInputDevice) : base(silkInputDevice)
    {
    }

    public IReadOnlyList<MouseButton> SupportedButtons => _silkInputDevice.SupportedButtons.Select(b => b.FromSilk()).ToArray();

    public IReadOnlyList<ScrollWheel> ScrollWheels => _silkInputDevice.ScrollWheels.Select(w => w.FromSilk()).ToArray();

    public Vector2 Position { get => _silkInputDevice.Position; set => _silkInputDevice.Position = value; }

    public ICursor Cursor => new SilkCursor(_silkInputDevice.Cursor);

    public int DoubleClickTime { get => _silkInputDevice.DoubleClickTime; set => _silkInputDevice.DoubleClickTime = value; }
    public int DoubleClickRange { get => _silkInputDevice.DoubleClickRange; set => _silkInputDevice.DoubleClickRange = value; }

    public event Action<IMouse, MouseButton> MouseDown
    {
        add { bool emptyAction = _mouseDownActions.Count == 0; _mouseDownActions.Add(value); if (emptyAction) _silkInputDevice.MouseDown += OnMouseDown; }
        remove { bool lastAction = _mouseDownActions.Count == 1; _mouseDownActions.Remove(value); if (lastAction) _silkInputDevice.MouseDown -= OnMouseDown; }
    }
    private List<Action<IMouse, MouseButton>?> _mouseDownActions = new();
    private void OnMouseDown(Silk.NET.Input.IMouse mouse, Silk.NET.Input.MouseButton button)
    {
        foreach (Action<IMouse, MouseButton>? action in _mouseDownActions)
        {
            action?.Invoke(SilkInputDevice.FromSilk(mouse), button.FromSilk());
        }
    }
    public event Action<IMouse, MouseButton> MouseUp
    {
        add { bool emptyAction = _mouseUpActions.Count == 0; _mouseUpActions.Add(value); if (emptyAction) _silkInputDevice.MouseUp += OnMouseUp; }
        remove { bool lastAction = _mouseUpActions.Count == 1; _mouseUpActions.Remove(value); if (lastAction) _silkInputDevice.MouseUp -= OnMouseUp; }
    }
    private List<Action<IMouse, MouseButton>?> _mouseUpActions = new();
    private void OnMouseUp(Silk.NET.Input.IMouse mouse, Silk.NET.Input.MouseButton button)
    {
        foreach (Action<IMouse, MouseButton>? action in _mouseUpActions)
        {
            action?.Invoke(SilkInputDevice.FromSilk(mouse), button.FromSilk());
        }
    }
    public event Action<IMouse, MouseButton, Vector2> Click
    {
        add { bool emptyAction = _clickActions.Count == 0; _clickActions.Add(value); if (emptyAction) _silkInputDevice.Click += OnClick; }
        remove { bool lastAction = _clickActions.Count == 1; _clickActions.Remove(value); if (lastAction) _silkInputDevice.Click -= OnClick; }
    }
    private List<Action<IMouse, MouseButton, Vector2>?> _clickActions = new();
    private void OnClick(Silk.NET.Input.IMouse mouse, Silk.NET.Input.MouseButton button, Vector2 position)
    {
        foreach (Action<IMouse, MouseButton, Vector2>? action in _clickActions)
        {
            action?.Invoke(SilkInputDevice.FromSilk(mouse), button.FromSilk(), position);
        }
    }
    public event Action<IMouse, MouseButton, Vector2> DoubleClick
    {
        add { bool emptyAction = _doubleClickActions.Count == 0; _doubleClickActions.Add(value); if (emptyAction) _silkInputDevice.DoubleClick += OnDoubleClick; }
        remove { bool lastAction = _doubleClickActions.Count == 1; _doubleClickActions.Remove(value); if (lastAction) _silkInputDevice.DoubleClick -= OnDoubleClick; }
    }
    private List<Action<IMouse, MouseButton, Vector2>?> _doubleClickActions = new();
    private void OnDoubleClick(Silk.NET.Input.IMouse mouse, Silk.NET.Input.MouseButton button, Vector2 position)
    {
        foreach (Action<IMouse, MouseButton, Vector2>? action in _doubleClickActions)
        {
            action?.Invoke(SilkInputDevice.FromSilk(mouse), button.FromSilk(), position);
        }
    }
    public event Action<IMouse, Vector2> MouseMove
    {
        add { bool emptyAction = _mouseMoveActions.Count == 0; _mouseMoveActions.Add(value); if (emptyAction) _silkInputDevice.MouseMove += OnMouseMove; }
        remove { bool lastAction = _mouseMoveActions.Count == 1; _mouseMoveActions.Remove(value); if (lastAction) _silkInputDevice.MouseMove -= OnMouseMove; }
    }
    private List<Action<IMouse, Vector2>?> _mouseMoveActions = new();
    private void OnMouseMove(Silk.NET.Input.IMouse mouse, Vector2 position)
    {
        foreach (Action<IMouse, Vector2>? action in _mouseMoveActions)
        {
            action?.Invoke(SilkInputDevice.FromSilk(mouse), position);
        }
    }
    public event Action<IMouse, ScrollWheel> Scroll
    {
        add { bool emptyAction = _scrollActions.Count == 0; _scrollActions.Add(value); if (emptyAction) _silkInputDevice.Scroll += OnScroll; }
        remove { bool lastAction = _scrollActions.Count == 1; _scrollActions.Remove(value); if (lastAction) _silkInputDevice.Scroll -= OnScroll; }
    }
    private List<Action<IMouse, ScrollWheel>?> _scrollActions = new();
    private void OnScroll(Silk.NET.Input.IMouse mouse, Silk.NET.Input.ScrollWheel wheel)
    {
        foreach (Action<IMouse, ScrollWheel>? action in _scrollActions)
        {
            action?.Invoke(SilkInputDevice.FromSilk(mouse), wheel.FromSilk());
        }
    }

    public bool IsButtonPressed(MouseButton btn)
    {
        return _silkInputDevice.IsButtonPressed(btn.ToSilk());
    }
}
