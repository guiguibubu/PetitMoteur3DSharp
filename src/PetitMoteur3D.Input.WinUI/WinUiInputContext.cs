using System;
using System.Collections.Generic;
using PetitMoteur3D.Window.WinUI;

namespace PetitMoteur3D.Input.WinUI;

internal class WinUiInputContext : IInputContext
{
    public WinUiInputContext(WinUIWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        _keyboards = [new WinUiKeyboard(window.DirectXPanel)];
        _mice = [new WinUiMouse(window)];
    }

    public nint Handle => IntPtr.Zero;

    public IReadOnlyList<IGamepad> Gamepads => Array.Empty<IGamepad>();

    public IReadOnlyList<IJoystick> Joysticks => Array.Empty<IJoystick>();

    private readonly IKeyboard[] _keyboards;
    public IReadOnlyList<IKeyboard> Keyboards => _keyboards;

    private readonly IMouse[] _mice;
    public IReadOnlyList<IMouse> Mice => _mice;

    public IReadOnlyList<IInputDevice> OtherDevices => Array.Empty<IJoystick>();

    public event Action<IInputDevice, bool>? ConnexionChanged
    {
        add { }
        remove { }
    }

    public void Dispose()
    {
    }
}
