using System;
using System.Collections.Generic;
using System.Linq;

namespace PetitMoteur3D.Input.SilkNet;

internal class SilkInputContext : IInputContext
{
    Silk.NET.Input.IInputContext _silkInputContext;
    public SilkInputContext(Silk.NET.Input.IInputContext silkInputContext)
    {
        ArgumentNullException.ThrowIfNull(silkInputContext);
        _silkInputContext = silkInputContext;
    }

    public nint Handle => _silkInputContext.Handle;

    public IReadOnlyList<IGamepad> Gamepads => _silkInputContext.Gamepads.Select(g => SilkInputDevice.FromSilk(g)).ToArray();

    public IReadOnlyList<IJoystick> Joysticks => _silkInputContext.Joysticks.Select(j => SilkInputDevice.FromSilk(j)).ToArray();

    public IReadOnlyList<IKeyboard> Keyboards => _silkInputContext.Keyboards.Select(k => SilkInputDevice.FromSilk(k)).ToArray();

    public IReadOnlyList<IMouse> Mice => _silkInputContext.Mice.Select(m => SilkInputDevice.FromSilk(m)).ToArray();

    public IReadOnlyList<IInputDevice> OtherDevices => _silkInputContext.OtherDevices.Select(d => SilkInputDevice.FromSilk(d)).ToArray();

    public event Action<IInputDevice, bool>? ConnexionChanged
    {
        add { bool emptyAction = _connnexionChangedActions.Count == 0; _connnexionChangedActions.Add(value); if (emptyAction) _silkInputContext.ConnectionChanged += OnConnexionChanged; }
        remove { bool lastAction = _connnexionChangedActions.Count == 1; _connnexionChangedActions.Remove(value); if (lastAction) _silkInputContext.ConnectionChanged -= OnConnexionChanged; }
    }
    private List<Action<IInputDevice, bool>?> _connnexionChangedActions = new();
    private void OnConnexionChanged(Silk.NET.Input.IInputDevice device, bool connected)
    {
        foreach (Action<IInputDevice, bool>? action in _connnexionChangedActions)
        {
            action?.Invoke(SilkInputDevice.FromSilk(device), connected);
        }
    }

    public void Dispose()
    {
        _silkInputContext?.Dispose();
    }
}
