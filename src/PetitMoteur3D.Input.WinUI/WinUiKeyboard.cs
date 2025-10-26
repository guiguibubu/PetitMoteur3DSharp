using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using PetitMoteur3D.Input.WinUI.Extensions;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace PetitMoteur3D.Input.WinUI
{
    internal class WinUiKeyboard : IKeyboard
    {
        private readonly Microsoft.UI.Xaml.UIElement _window;
        public WinUiKeyboard(Microsoft.UI.Xaml.UIElement window, string? name = null)
        {
            _window = window;
            Name = name ?? "PetitMoteur3D - WinUiKeyboard";
        }

        #region IInputDevice implementation

        public string Name { get; }

        public bool IsConnected => IsKeyBoardConnected();
        private static bool IsKeyBoardConnected()
        {
            var keyboardCapabilities = new KeyboardCapabilities();
            return keyboardCapabilities.KeyboardPresent != 0;
        }
        #endregion

        public IReadOnlyList<Key> SupportedKeys => _availableKeys;

        public async Task<string> GetClipboardTextAsync()
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync().AsTask();

                return text;
            }
            else
            {
                return "";
            }
        }

        public Task SetClipboardTextAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Task.CompletedTask;
            }
            DataPackage dataPackage = new();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
            return Task.CompletedTask;
        }

        public event Action<IKeyboard, Key, int>? KeyDown
        {
            add { bool emptyAction = _keyDownActions.Count == 0; _keyDownActions.Add(value); if (emptyAction) _window.KeyDown += OnKeyDown; }
            remove { bool lastAction = _keyDownActions.Count == 1; _keyDownActions.Remove(value); if (lastAction) _window.KeyDown -= OnKeyDown; }
        }
        private List<Action<IKeyboard, Key, int>?> _keyDownActions = new();
        private void OnKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            int scanCode = (int)e.KeyStatus.ScanCode;
            _scanCodesPressedCache.AddOrUpdate(scanCode, true, (int _, bool _) => true);
            foreach (Action<IKeyboard, Key, int>? action in _keyDownActions)
            {
                action?.Invoke(this, e.Key.FromWinUi(), scanCode);
                e.Handled = true;
            }
        }
        public event Action<IKeyboard, Key, int>? KeyUp
        {
            add { bool emptyAction = _keyUpActions.Count == 0; _keyUpActions.Add(value); if (emptyAction) _window.KeyUp += OnKeyUp; }
            remove { bool lastAction = _keyUpActions.Count == 1; _keyUpActions.Remove(value); if (lastAction) _window.KeyUp -= OnKeyUp; }
        }
        private List<Action<IKeyboard, Key, int>?> _keyUpActions = new();
        private void OnKeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            int scanCode = (int)e.KeyStatus.ScanCode;
            _scanCodesPressedCache.AddOrUpdate(scanCode, false, (int _, bool _) => false);
            foreach (Action<IKeyboard, Key, int>? action in _keyUpActions)
            {
                action?.Invoke(this, e.Key.FromWinUi(), scanCode);
                e.Handled = true;
            }
        }
        public event Action<IKeyboard, char>? KeyChar
        {
            add { bool emptyAction = _keyCharActions.Count == 0; _keyCharActions.Add(value); if (emptyAction) _window.CharacterReceived += OnKeyChar; }
            remove { bool lastAction = _keyCharActions.Count == 1; _keyCharActions.Remove(value); if (lastAction) _window.CharacterReceived -= OnKeyChar; }
        }
        private List<Action<IKeyboard, char>?> _keyCharActions = new();
        private void OnKeyChar(UIElement sender, Microsoft.UI.Xaml.Input.CharacterReceivedRoutedEventArgs args)
        {
            foreach (Action<IKeyboard, char>? action in _keyCharActions)
            {
                action?.Invoke(this, args.Character);
                args.Handled = true;
            }
        }

        public void BeginInput()
        {
            nint handleId = Process.GetCurrentProcess().MainWindowHandle;
            InputPane inputPane = InputPaneInterop.GetForWindow(handleId);
            inputPane.TryShow();
        }

        public void EndInput()
        {
            nint handleId = Process.GetCurrentProcess().MainWindowHandle;
            InputPane inputPane = InputPaneInterop.GetForWindow(handleId);
            inputPane.TryHide();
        }

        public bool IsKeyPressed(Key key)
        {
            CoreVirtualKeyStates state = InputKeyboardSource.GetKeyStateForCurrentThread(key.ToWinUi());
            return state == CoreVirtualKeyStates.Down;
        }

        ConcurrentDictionary<int, bool> _scanCodesPressedCache = new();
        public bool IsScancodePressed(int scancode)
        {
            return _scanCodesPressedCache.GetOrAdd(scancode, false);
        }


        private static readonly IReadOnlyList<Windows.System.VirtualKey> _winUiVirtualKeys = new List<Windows.System.VirtualKey>()
        {
            Windows.System.VirtualKey.LeftButton,
            Windows.System.VirtualKey.RightButton,
            Windows.System.VirtualKey.Cancel,
            Windows.System.VirtualKey.MiddleButton,
            Windows.System.VirtualKey.XButton1,
            Windows.System.VirtualKey.XButton2,
            Windows.System.VirtualKey.Back,
            Windows.System.VirtualKey.Tab,
            Windows.System.VirtualKey.Clear,
            Windows.System.VirtualKey.Enter,
            Windows.System.VirtualKey.Shift,
            Windows.System.VirtualKey.Control,
            Windows.System.VirtualKey.Menu,
            Windows.System.VirtualKey.Pause,
            Windows.System.VirtualKey.CapitalLock,
            Windows.System.VirtualKey.Kana,
            Windows.System.VirtualKey.Hangul,
            Windows.System.VirtualKey.ImeOn,
            Windows.System.VirtualKey.Junja,
            Windows.System.VirtualKey.Final,
            Windows.System.VirtualKey.Hanja,
            Windows.System.VirtualKey.Kanji,
            Windows.System.VirtualKey.ImeOff,
            Windows.System.VirtualKey.Escape,
            Windows.System.VirtualKey.Convert,
            Windows.System.VirtualKey.NonConvert,
            Windows.System.VirtualKey.Accept,
            Windows.System.VirtualKey.ModeChange,
            Windows.System.VirtualKey.Space,
            Windows.System.VirtualKey.PageUp,
            Windows.System.VirtualKey.PageDown,
            Windows.System.VirtualKey.End,
            Windows.System.VirtualKey.Home,
            Windows.System.VirtualKey.Left,
            Windows.System.VirtualKey.Up,
            Windows.System.VirtualKey.Right,
            Windows.System.VirtualKey.Down,
            Windows.System.VirtualKey.Select,
            Windows.System.VirtualKey.Print,
            Windows.System.VirtualKey.Execute,
            Windows.System.VirtualKey.Snapshot,
            Windows.System.VirtualKey.Insert,
            Windows.System.VirtualKey.Delete,
            Windows.System.VirtualKey.Help,
            Windows.System.VirtualKey.Number0,
            Windows.System.VirtualKey.Number1,
            Windows.System.VirtualKey.Number2,
            Windows.System.VirtualKey.Number3,
            Windows.System.VirtualKey.Number4,
            Windows.System.VirtualKey.Number5,
            Windows.System.VirtualKey.Number6,
            Windows.System.VirtualKey.Number7,
            Windows.System.VirtualKey.Number8,
            Windows.System.VirtualKey.Number9,
            Windows.System.VirtualKey.A,
            Windows.System.VirtualKey.B,
            Windows.System.VirtualKey.C,
            Windows.System.VirtualKey.D,
            Windows.System.VirtualKey.E,
            Windows.System.VirtualKey.F,
            Windows.System.VirtualKey.G,
            Windows.System.VirtualKey.H,
            Windows.System.VirtualKey.I,
            Windows.System.VirtualKey.J,
            Windows.System.VirtualKey.K,
            Windows.System.VirtualKey.L,
            Windows.System.VirtualKey.M,
            Windows.System.VirtualKey.N,
            Windows.System.VirtualKey.O,
            Windows.System.VirtualKey.P,
            Windows.System.VirtualKey.Q,
            Windows.System.VirtualKey.R,
            Windows.System.VirtualKey.S,
            Windows.System.VirtualKey.T,
            Windows.System.VirtualKey.U,
            Windows.System.VirtualKey.V,
            Windows.System.VirtualKey.W,
            Windows.System.VirtualKey.X,
            Windows.System.VirtualKey.Y,
            Windows.System.VirtualKey.Z,
            Windows.System.VirtualKey.LeftWindows,
            Windows.System.VirtualKey.RightWindows,
            Windows.System.VirtualKey.Application,
            Windows.System.VirtualKey.Sleep,
            Windows.System.VirtualKey.NumberPad0,
            Windows.System.VirtualKey.NumberPad1,
            Windows.System.VirtualKey.NumberPad2,
            Windows.System.VirtualKey.NumberPad3,
            Windows.System.VirtualKey.NumberPad4,
            Windows.System.VirtualKey.NumberPad5,
            Windows.System.VirtualKey.NumberPad6,
            Windows.System.VirtualKey.NumberPad7,
            Windows.System.VirtualKey.NumberPad8,
            Windows.System.VirtualKey.NumberPad9,
            Windows.System.VirtualKey.Multiply,
            Windows.System.VirtualKey.Add,
            Windows.System.VirtualKey.Separator,
            Windows.System.VirtualKey.Subtract,
            Windows.System.VirtualKey.Decimal,
            Windows.System.VirtualKey.Divide,
            Windows.System.VirtualKey.F1,
            Windows.System.VirtualKey.F2,
            Windows.System.VirtualKey.F3,
            Windows.System.VirtualKey.F4,
            Windows.System.VirtualKey.F5,
            Windows.System.VirtualKey.F6,
            Windows.System.VirtualKey.F7,
            Windows.System.VirtualKey.F8,
            Windows.System.VirtualKey.F9,
            Windows.System.VirtualKey.F10,
            Windows.System.VirtualKey.F11,
            Windows.System.VirtualKey.F12,
            Windows.System.VirtualKey.F13,
            Windows.System.VirtualKey.F14,
            Windows.System.VirtualKey.F15,
            Windows.System.VirtualKey.F16,
            Windows.System.VirtualKey.F17,
            Windows.System.VirtualKey.F18,
            Windows.System.VirtualKey.F19,
            Windows.System.VirtualKey.F21,
            Windows.System.VirtualKey.F22,
            Windows.System.VirtualKey.F23,
            Windows.System.VirtualKey.F24,
            Windows.System.VirtualKey.NumberKeyLock,
            Windows.System.VirtualKey.Scroll,
            Windows.System.VirtualKey.LeftShift,
            Windows.System.VirtualKey.RightShift,
            Windows.System.VirtualKey.LeftControl,
            Windows.System.VirtualKey.RightControl
        };

        private static readonly IReadOnlyList<Key> _availableKeys = _winUiVirtualKeys.Select(k => k.FromWinUi()).ToArray();
    }
}
