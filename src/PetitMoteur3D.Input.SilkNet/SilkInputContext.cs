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
        _gamepads = _silkInputContext.Gamepads.ToDictionary(g => g, g => SilkInputDevice.FromSilk(g));
        _joysticks = _silkInputContext.Joysticks.ToDictionary(g => g, g => SilkInputDevice.FromSilk(g));
        _keyboards = _silkInputContext.Keyboards.ToDictionary(g => g, g => SilkInputDevice.FromSilk(g));
        _mice = _silkInputContext.Mice.ToDictionary(g => g, g => SilkInputDevice.FromSilk(g));
        _otherDevices = _silkInputContext.OtherDevices.ToDictionary(g => g, g => SilkInputDevice.FromSilk(g));
        _gamepadsCache = _gamepads.Values.ToArray();
        _joysticksCache = _joysticks.Values.ToArray();
        _keyboardsCache = _keyboards.Values.ToArray();
        _miceCache = _mice.Values.ToArray();
        _otherDevicesCache = _otherDevices.Values.ToArray();
    }

    public nint Handle => _silkInputContext.Handle;

    public IReadOnlyList<IGamepad> Gamepads => _gamepadsCache;
    public IReadOnlyList<IJoystick> Joysticks => _joysticksCache;
    public IReadOnlyList<IKeyboard> Keyboards => _keyboardsCache;
    public IReadOnlyList<IMouse> Mice => _miceCache;
    public IReadOnlyList<IInputDevice> OtherDevices => _otherDevicesCache;

    public Dictionary<Silk.NET.Input.IGamepad, IGamepad> _gamepads;
    public Dictionary<Silk.NET.Input.IJoystick, IJoystick> _joysticks;
    public Dictionary<Silk.NET.Input.IKeyboard, IKeyboard> _keyboards;
    public Dictionary<Silk.NET.Input.IMouse, IMouse> _mice;
    public Dictionary<Silk.NET.Input.IInputDevice, IInputDevice> _otherDevices;
    public IGamepad[] _gamepadsCache;
    public IJoystick[] _joysticksCache;
    public IKeyboard[] _keyboardsCache;
    public IMouse[] _miceCache;
    public IInputDevice[] _otherDevicesCache;

    public event Action<IInputDevice, bool>? ConnexionChanged
    {
        add { bool emptyAction = _connnexionChangedActions.Count == 0; _connnexionChangedActions.Add(value); if (emptyAction) _silkInputContext.ConnectionChanged += OnConnexionChanged; }
        remove { bool lastAction = _connnexionChangedActions.Count == 1; _connnexionChangedActions.Remove(value); if (lastAction) _silkInputContext.ConnectionChanged -= OnConnexionChanged; }
    }
    private List<Action<IInputDevice, bool>?> _connnexionChangedActions = new() { };
    private void OnConnexionChanged(Silk.NET.Input.IInputDevice silkDevice, bool connected)
    {
        IInputDevice device = GetOrCreateDevice(silkDevice);
        foreach (Action<IInputDevice, bool>? action in _connnexionChangedActions)
        {
            action?.Invoke(device, connected);
        }
        if (!connected)
        {
            RemoveDevice(silkDevice);
        }

    }

    private IInputDevice GetOrCreateDevice(Silk.NET.Input.IInputDevice silkDevice)
    {
        if (silkDevice is Silk.NET.Input.IGamepad silkGamepad)
        {
            if (!_gamepads.TryGetValue(silkGamepad, out IGamepad? gamepad))
            {
                gamepad = SilkInputDevice.FromSilk(silkGamepad);
                _gamepads.Add(silkGamepad, gamepad);
                _gamepadsCache = _gamepads.Values.ToArray();
            }
            return gamepad;
        }
        else if (silkDevice is Silk.NET.Input.IJoystick silkJoystick)
        {
            if (!_joysticks.TryGetValue(silkJoystick, out IJoystick? joystick))
            {
                joystick = SilkInputDevice.FromSilk(silkJoystick);
                _joysticks.Add(silkJoystick, joystick);
                _joysticksCache = _joysticks.Values.ToArray();
            }
            return joystick;
        }
        else if (silkDevice is Silk.NET.Input.IKeyboard silkKeyboard)
        {
            if (!_keyboards.TryGetValue(silkKeyboard, out IKeyboard? keyboard))
            {
                keyboard = SilkInputDevice.FromSilk(silkKeyboard);
                _keyboards.Add(silkKeyboard, keyboard);
                _keyboardsCache = _keyboards.Values.ToArray();
            }
            return keyboard;
        }
        else if (silkDevice is Silk.NET.Input.IMouse silkMouse)
        {
            if (!_mice.TryGetValue(silkMouse, out IMouse? mouse))
            {
                mouse = SilkInputDevice.FromSilk(silkMouse);
                _mice.Add(silkMouse, mouse);
                _otherDevicesCache = _otherDevices.Values.ToArray();
            }
            return mouse;
        }
        else
        {
            if (!_otherDevices.TryGetValue(silkDevice, out IInputDevice? otherDevice))
            {
                otherDevice = SilkInputDevice.FromSilk(silkDevice);
                _otherDevices.Add(silkDevice, otherDevice);
                _otherDevicesCache = _otherDevices.Values.ToArray();
            }
            return otherDevice;
        }
    }

    private void RemoveDevice(Silk.NET.Input.IInputDevice silkDevice)
    {
        if (silkDevice is Silk.NET.Input.IGamepad silkGamepad)
        {
            _gamepads.Remove(silkGamepad);
            _gamepadsCache = _gamepads.Values.ToArray();
        }
        else if (silkDevice is Silk.NET.Input.IJoystick silkJoystick)
        {
            _joysticks.Remove(silkJoystick);
            _joysticksCache = _joysticks.Values.ToArray();
        }
        else if (silkDevice is Silk.NET.Input.IKeyboard silkKeyboard)
        {
            _keyboards.Remove(silkKeyboard);
            _keyboardsCache = _keyboards.Values.ToArray();
        }
        else if (silkDevice is Silk.NET.Input.IMouse silkMouse)
        {
            _mice.Remove(silkMouse);
            _miceCache = _mice.Values.ToArray();
        }
        else
        {
            _otherDevices.Remove(silkDevice);
            _otherDevicesCache = _otherDevices.Values.ToArray();
        }
    }

    public void Dispose()
    {
        _silkInputContext?.Dispose();
    }
}
