

using System;
using System.Collections.Generic;
using System.Linq;
using PetitMoteur3D.Input.SilkNet.Extensions;

namespace PetitMoteur3D.Input.SilkNet
{
    internal class SilkJoystick : BaseSilkInputDeviceImpl<Silk.NET.Input.IJoystick>, IJoystick
    {
        public SilkJoystick(Silk.NET.Input.IJoystick silkInputDevice) : base(silkInputDevice)
        {
        }

        public IReadOnlyList<Axis> Axes => _silkInputDevice.Axes.Select(a => a.FromSilk()).ToArray();

        public IReadOnlyList<Button> Buttons => _silkInputDevice.Buttons.Select(b => b.FromSilk()).ToArray();

        public IReadOnlyList<Hat> Hats => _silkInputDevice.Hats.Select(h => h.FromSilk()).ToArray();

        public Deadzone Deadzone { get => _silkInputDevice.Deadzone.FromSilk(); set => _silkInputDevice.Deadzone = value.ToSilk(); }

        public event Action<IJoystick, Button>? ButtonDown
        {
            add { bool emptyAction = _buttonDownActions.Count == 0; _buttonDownActions.Add(value); if (emptyAction) _silkInputDevice.ButtonDown += OnButtonDown; }
            remove { bool lastAction = _buttonDownActions.Count == 1; _buttonDownActions.Remove(value); if (lastAction) _silkInputDevice.ButtonDown -= OnButtonDown; }
        }
        private List<Action<IJoystick, Button>?> _buttonDownActions = new();
        private void OnButtonDown(Silk.NET.Input.IJoystick joystick, Silk.NET.Input.Button button)
        {
            foreach (Action<IJoystick, Button>? action in _buttonDownActions)
            {
                action?.Invoke(SilkInputDevice.FromSilk(joystick), button.FromSilk());
            }
        }
        public event Action<IJoystick, Button>? ButtonUp
        {
            add { bool emptyAction = _buttonUpActions.Count == 0; _buttonUpActions.Add(value); if (emptyAction) _silkInputDevice.ButtonDown += OnButttonUp; }
            remove { bool lastAction = _buttonUpActions.Count == 1; _buttonUpActions.Remove(value); if (lastAction) _silkInputDevice.ButtonDown -= OnButttonUp; }
        }
        private List<Action<IJoystick, Button>?> _buttonUpActions = new();
        private void OnButttonUp(Silk.NET.Input.IJoystick joystick, Silk.NET.Input.Button button)
        {
            foreach (Action<IJoystick, Button>? action in _buttonUpActions)
            {
                action?.Invoke(SilkInputDevice.FromSilk(joystick), button.FromSilk());
            }
        }
        public event Action<IJoystick, Axis>? AxisMoved
        {
            add { bool emptyAction = _axisMovedActions.Count == 0; _axisMovedActions.Add(value); if (emptyAction) _silkInputDevice.AxisMoved += OnAxisMoved; }
            remove { bool lastAction = _axisMovedActions.Count == 1; _axisMovedActions.Remove(value); if (lastAction) _silkInputDevice.AxisMoved -= OnAxisMoved; }
        }
        private List<Action<IJoystick, Axis>?> _axisMovedActions = new();
        private void OnAxisMoved(Silk.NET.Input.IJoystick joystick, Silk.NET.Input.Axis axis)
        {
            foreach (Action<IJoystick, Axis>? action in _axisMovedActions)
            {
                action?.Invoke(SilkInputDevice.FromSilk(joystick), axis.FromSilk());
            }
        }
        public event Action<IJoystick, Hat>? HatMoved
        {
            add { bool emptyAction = _hatActions.Count == 0; _hatActions.Add(value); if (emptyAction) _silkInputDevice.HatMoved += OnHatMoved; }
            remove { bool lastAction = _hatActions.Count == 1; _hatActions.Remove(value); if (lastAction) _silkInputDevice.HatMoved -= OnHatMoved; }
        }
        private List<Action<IJoystick, Hat>?> _hatActions = new();
        private void OnHatMoved(Silk.NET.Input.IJoystick joystick, Silk.NET.Input.Hat hat)
        {
            foreach (Action<IJoystick, Hat>? action in _hatActions)
            {
                action?.Invoke(SilkInputDevice.FromSilk(joystick), hat.FromSilk());
            }
        }
    }
}
