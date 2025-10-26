using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetitMoteur3D.Input.SilkNet
{
    internal class SilkKeyboard : BaseSilkInputDeviceImpl<Silk.NET.Input.IKeyboard>, IKeyboard
    {
        public SilkKeyboard(Silk.NET.Input.IKeyboard silkInputDevice) : base(silkInputDevice)
        {
        }

        public IReadOnlyList<Key> SupportedKeys => _silkInputDevice.SupportedKeys.Select(k => k.FromSilk()).ToArray();

        public Task<string> GetClipboardTextAsync()
        {
            return Task.FromResult(_silkInputDevice.ClipboardText);
        }

        public Task SetClipboardTextAsync(string text)
        {
            _silkInputDevice.ClipboardText = text;
            return Task.CompletedTask;
        }

        public event Action<IKeyboard, Key, int>? KeyDown
        {
            add { bool emptyAction = _keyDownActions.Count == 0; _keyDownActions.Add(value); if (emptyAction) _silkInputDevice.KeyDown += OnKeyDown; }
            remove { bool lastAction = _keyDownActions.Count == 1; _keyDownActions.Remove(value); if (lastAction) _silkInputDevice.KeyDown -= OnKeyDown; }
        }
        private List<Action<IKeyboard, Key, int>?> _keyDownActions = new();
        private void OnKeyDown(Silk.NET.Input.IKeyboard keyboard, Silk.NET.Input.Key keycode, int scancode)
        {
            foreach (Action<IKeyboard, Key, int>? action in _keyDownActions)
            {
                action?.Invoke(SilkInputDevice.FromSilk(keyboard), keycode.FromSilk(), scancode);
            }
        }
        public event Action<IKeyboard, Key, int>? KeyUp
        {
            add { bool emptyAction = _keyUpActions.Count == 0; _keyUpActions.Add(value); if (emptyAction) _silkInputDevice.KeyUp += OnKeyUp; }
            remove { bool lastAction = _keyUpActions.Count == 1; _keyUpActions.Remove(value); if (lastAction) _silkInputDevice.KeyUp -= OnKeyUp; }
        }
        private List<Action<IKeyboard, Key, int>?> _keyUpActions = new();
        private void OnKeyUp(Silk.NET.Input.IKeyboard keyboard, Silk.NET.Input.Key keycode, int scancode)
        {
            foreach (Action<IKeyboard, Key, int>? action in _keyUpActions)
            {
                action?.Invoke(SilkInputDevice.FromSilk(keyboard), keycode.FromSilk(), scancode);
            }
        }
        public event Action<IKeyboard, char>? KeyChar
        {
            add { bool emptyAction = _keyCharActions.Count == 0; _keyCharActions.Add(value); if (emptyAction) _silkInputDevice.KeyChar += OnKeyChar; }
            remove { bool lastAction = _keyCharActions.Count == 1; _keyCharActions.Remove(value); if (lastAction) _silkInputDevice.KeyChar -= OnKeyChar; }
        }
        private List<Action<IKeyboard, char>?> _keyCharActions = new();
        private void OnKeyChar(Silk.NET.Input.IKeyboard keyboard, char character)
        {
            foreach (Action<IKeyboard, char>? action in _keyCharActions)
            {
                action?.Invoke(SilkInputDevice.FromSilk(keyboard), character);
            }
        }

        public void BeginInput()
        {
            _silkInputDevice.BeginInput();
        }

        public void EndInput()
        {
            _silkInputDevice.EndInput();
        }

        public bool IsKeyPressed(Key key)
        {
            return _silkInputDevice.IsKeyPressed(key.ToSilk());
        }

        public bool IsScancodePressed(int scancode)
        {
            return _silkInputDevice.IsScancodePressed(scancode);
        }
    }
}
