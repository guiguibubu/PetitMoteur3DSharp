
using PetitMoteur3D.Input.SilkNet.Extensions;

namespace PetitMoteur3D.Input.SilkNet
{
    internal class SilkGamepad : BaseSilkInputDeviceImpl<Silk.NET.Input.IGamepad>, IGamepad
    {
        public event Action<IGamepad, Button>? ButtonDown
        {
            add { bool emptyAction = _buttonDownActions.Count == 0; _buttonDownActions.Add(value); if (emptyAction) _silkInputDevice.ButtonDown += OnButtonDown; }
            remove { bool lastAction = _buttonDownActions.Count == 1; _buttonDownActions.Remove(value); if (lastAction) _silkInputDevice.ButtonDown -= OnButtonDown; }
        }
        private List<Action<IGamepad, Button>?> _buttonDownActions = new();
        private void OnButtonDown(Silk.NET.Input.IGamepad gamepad, Silk.NET.Input.Button button)
        {
            foreach (Action<IGamepad, Button>? action in _buttonDownActions)
            {
                action?.Invoke(SilkInputDevice.FromSilk(gamepad), button.FromSilk());
            }
        }
        public event Action<IGamepad, Button>? ButtonUp
        {
            add { bool emptyAction = _buttonUpActions.Count == 0; _buttonUpActions.Add(value); if (emptyAction) _silkInputDevice.ButtonDown += OnButttonUp; }
            remove { bool lastAction = _buttonUpActions.Count == 1; _buttonUpActions.Remove(value); if (lastAction) _silkInputDevice.ButtonDown -= OnButttonUp; }
        }
        private List<Action<IGamepad, Button>?> _buttonUpActions = new();
        private void OnButttonUp(Silk.NET.Input.IGamepad gamepad, Silk.NET.Input.Button button)
        {
            foreach (Action<IGamepad, Button>? action in _buttonUpActions)
            {
                action?.Invoke(SilkInputDevice.FromSilk(gamepad), button.FromSilk());
            }
        }
        public event Action<IGamepad, Thumbstick>? ThumbstickMoved
        {
            add { bool emptyAction = _thumbstickMovedActions.Count == 0; _thumbstickMovedActions.Add(value); if (emptyAction) _silkInputDevice.ThumbstickMoved += OnThumbstickMoved; }
            remove { bool lastAction = _thumbstickMovedActions.Count == 1; _thumbstickMovedActions.Remove(value); if (lastAction) _silkInputDevice.ThumbstickMoved -= OnThumbstickMoved; }
        }
        private List<Action<IGamepad, Thumbstick>?> _thumbstickMovedActions = new();
        private void OnThumbstickMoved(Silk.NET.Input.IGamepad gamepad, Silk.NET.Input.Thumbstick stick)
        {
            foreach (Action<IGamepad, Thumbstick>? action in _thumbstickMovedActions)
            {
                action?.Invoke(SilkInputDevice.FromSilk(gamepad), stick.FromSilk());
            }
        }
        public event Action<IGamepad, Trigger>? TriggerMoved
        {
            add { bool emptyAction = _triggerMovedActions.Count == 0; _triggerMovedActions.Add(value); if (emptyAction) _silkInputDevice.TriggerMoved += OnTriggerMoved; }
            remove { bool lastAction = _triggerMovedActions.Count == 1; _triggerMovedActions.Remove(value); if (lastAction) _silkInputDevice.TriggerMoved -= OnTriggerMoved; }
        }
        private List<Action<IGamepad, Trigger>?> _triggerMovedActions = new();
        private void OnTriggerMoved(Silk.NET.Input.IGamepad gamepad, Silk.NET.Input.Trigger trigger)
        {
            foreach (Action<IGamepad, Trigger>? action in _triggerMovedActions)
            {
                action?.Invoke(SilkInputDevice.FromSilk(gamepad), trigger.FromSilk());
            }
        }

        public SilkGamepad(Silk.NET.Input.IGamepad silkInputDevice) : base(silkInputDevice)
        {
        }

        public IReadOnlyList<Button> Buttons => _silkInputDevice.Buttons.Select(b => b.FromSilk()).ToArray();

        public IReadOnlyList<Thumbstick> Thumbsticks => _silkInputDevice.Thumbsticks.Select(s => s.FromSilk()).ToArray();

        public IReadOnlyList<Trigger> Triggers => _silkInputDevice.Triggers.Select(t => t.FromSilk()).ToArray();

        public IReadOnlyList<IMotor> VibrationMotors => _silkInputDevice.VibrationMotors.Select(m => new SilkMotor(m)).ToArray();

        public Deadzone Deadzone { get => _silkInputDevice.Deadzone.FromSilk(); set => _silkInputDevice.Deadzone = value.ToSilk(); }
    }
}
