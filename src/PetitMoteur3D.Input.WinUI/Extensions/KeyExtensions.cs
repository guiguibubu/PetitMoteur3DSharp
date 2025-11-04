using Windows.Win32;

namespace PetitMoteur3D.Input.WinUI.Extensions;

internal static class KeyExtensions
{
    public static Key FromWinUi(this Windows.System.VirtualKey key)
    {
        switch (key)
        {
            case Windows.System.VirtualKey.None: return Key.Unknown;
            case Windows.System.VirtualKey.Space: return Key.Space;
            case Windows.System.VirtualKey.Number0: return Key.Number0;
            case Windows.System.VirtualKey.Number1: return Key.Number1;
            case Windows.System.VirtualKey.Number2: return Key.Number2;
            case Windows.System.VirtualKey.Number3: return Key.Number3;
            case Windows.System.VirtualKey.Number4: return Key.Number4;
            case Windows.System.VirtualKey.Number5: return Key.Number5;
            case Windows.System.VirtualKey.Number6: return Key.Number6;
            case Windows.System.VirtualKey.Number7: return Key.Number7;
            case Windows.System.VirtualKey.Number8: return Key.Number8;
            case Windows.System.VirtualKey.Number9: return Key.Number9;
            case Windows.System.VirtualKey.A: return Key.A;
            case Windows.System.VirtualKey.B: return Key.B;
            case Windows.System.VirtualKey.C: return Key.C;
            case Windows.System.VirtualKey.D: return Key.D;
            case Windows.System.VirtualKey.E: return Key.E;
            case Windows.System.VirtualKey.F: return Key.F;
            case Windows.System.VirtualKey.G: return Key.G;
            case Windows.System.VirtualKey.H: return Key.H;
            case Windows.System.VirtualKey.I: return Key.I;
            case Windows.System.VirtualKey.J: return Key.J;
            case Windows.System.VirtualKey.K: return Key.K;
            case Windows.System.VirtualKey.L: return Key.L;
            case Windows.System.VirtualKey.M: return Key.M;
            case Windows.System.VirtualKey.N: return Key.N;
            case Windows.System.VirtualKey.O: return Key.O;
            case Windows.System.VirtualKey.P: return Key.P;
            case Windows.System.VirtualKey.Q: return Key.Q;
            case Windows.System.VirtualKey.R: return Key.R;
            case Windows.System.VirtualKey.S: return Key.S;
            case Windows.System.VirtualKey.T: return Key.T;
            case Windows.System.VirtualKey.U: return Key.U;
            case Windows.System.VirtualKey.V: return Key.V;
            case Windows.System.VirtualKey.W: return Key.W;
            case Windows.System.VirtualKey.X: return Key.X;
            case Windows.System.VirtualKey.Y: return Key.Y;
            case Windows.System.VirtualKey.Z: return Key.Z;
            case Windows.System.VirtualKey.Escape: return Key.Escape;
            case Windows.System.VirtualKey.Enter: return Key.Enter;
            case Windows.System.VirtualKey.Tab: return Key.Tab;
            case Windows.System.VirtualKey.Back: return Key.Backspace;
            case Windows.System.VirtualKey.Insert: return Key.Insert;
            case Windows.System.VirtualKey.Delete: return Key.Delete;
            case Windows.System.VirtualKey.Right: return Key.Right;
            case Windows.System.VirtualKey.Left: return Key.Left;
            case Windows.System.VirtualKey.Down: return Key.Down;
            case Windows.System.VirtualKey.Up: return Key.Up;
            case Windows.System.VirtualKey.PageUp: return Key.PageUp;
            case Windows.System.VirtualKey.PageDown: return Key.PageDown;
            case Windows.System.VirtualKey.Home: return Key.Home;
            case Windows.System.VirtualKey.End: return Key.End;
            case Windows.System.VirtualKey.CapitalLock: return Key.CapsLock;
            case Windows.System.VirtualKey.Scroll: return Key.ScrollLock;
            case Windows.System.VirtualKey.NumberKeyLock: return Key.NumLock;
            case Windows.System.VirtualKey.Print: return Key.PrintScreen;
            case Windows.System.VirtualKey.Pause: return Key.Pause;
            case Windows.System.VirtualKey.F1: return Key.F1;
            case Windows.System.VirtualKey.F2: return Key.F2;
            case Windows.System.VirtualKey.F3: return Key.F3;
            case Windows.System.VirtualKey.F4: return Key.F4;
            case Windows.System.VirtualKey.F5: return Key.F5;
            case Windows.System.VirtualKey.F6: return Key.F6;
            case Windows.System.VirtualKey.F7: return Key.F7;
            case Windows.System.VirtualKey.F8: return Key.F8;
            case Windows.System.VirtualKey.F9: return Key.F9;
            case Windows.System.VirtualKey.F10: return Key.F10;
            case Windows.System.VirtualKey.F11: return Key.F11;
            case Windows.System.VirtualKey.F12: return Key.F12;
            case Windows.System.VirtualKey.F13: return Key.F13;
            case Windows.System.VirtualKey.F14: return Key.F14;
            case Windows.System.VirtualKey.F15: return Key.F15;
            case Windows.System.VirtualKey.F16: return Key.F16;
            case Windows.System.VirtualKey.F17: return Key.F17;
            case Windows.System.VirtualKey.F18: return Key.F18;
            case Windows.System.VirtualKey.F19: return Key.F19;
            case Windows.System.VirtualKey.F20: return Key.F20;
            case Windows.System.VirtualKey.F21: return Key.F21;
            case Windows.System.VirtualKey.F22: return Key.F22;
            case Windows.System.VirtualKey.F23: return Key.F23;
            case Windows.System.VirtualKey.F24: return Key.F24;
            case Windows.System.VirtualKey.NumberPad0: return Key.Keypad0;
            case Windows.System.VirtualKey.NumberPad1: return Key.Keypad1;
            case Windows.System.VirtualKey.NumberPad2: return Key.Keypad2;
            case Windows.System.VirtualKey.NumberPad3: return Key.Keypad3;
            case Windows.System.VirtualKey.NumberPad4: return Key.Keypad4;
            case Windows.System.VirtualKey.NumberPad5: return Key.Keypad5;
            case Windows.System.VirtualKey.NumberPad6: return Key.Keypad6;
            case Windows.System.VirtualKey.NumberPad7: return Key.Keypad7;
            case Windows.System.VirtualKey.NumberPad8: return Key.Keypad8;
            case Windows.System.VirtualKey.NumberPad9: return Key.Keypad9;
            case Windows.System.VirtualKey.Decimal: return Key.KeypadDecimal;
            case Windows.System.VirtualKey.Separator: return Key.KeypadDecimal;
            case Windows.System.VirtualKey.Divide: return Key.KeypadDivide;
            case Windows.System.VirtualKey.Multiply: return Key.KeypadMultiply;
            case Windows.System.VirtualKey.Subtract: return Key.KeypadSubtract;
            case Windows.System.VirtualKey.Add: return Key.KeypadAdd;
            case Windows.System.VirtualKey.LeftShift: return Key.ShiftLeft;
            case Windows.System.VirtualKey.LeftControl: return Key.ControlLeft;
            case Windows.System.VirtualKey.LeftMenu: return Key.AltLeft;
            case Windows.System.VirtualKey.LeftWindows: return Key.SuperLeft;
            case Windows.System.VirtualKey.RightShift: return Key.ShiftRight;
            case Windows.System.VirtualKey.RightControl: return Key.ControlRight;
            case Windows.System.VirtualKey.RightMenu: return Key.AltRight;
            case Windows.System.VirtualKey.RightWindows: return Key.SuperRight;
            case Windows.System.VirtualKey.Menu: return Key.Menu;
            default:
                return Key.Unknown;
        }
    }

    public static Windows.System.VirtualKey ToWinUi(this Key key)
    {
        switch (key)
        {
            case Key.Unknown: return Windows.System.VirtualKey.None;
            case Key.Space: return Windows.System.VirtualKey.Space;
            case Key.Number0: return Windows.System.VirtualKey.Number0;
            case Key.Number1: return Windows.System.VirtualKey.Number1;
            case Key.Number2: return Windows.System.VirtualKey.Number2;
            case Key.Number3: return Windows.System.VirtualKey.Number3;
            case Key.Number4: return Windows.System.VirtualKey.Number4;
            case Key.Number5: return Windows.System.VirtualKey.Number5;
            case Key.Number6: return Windows.System.VirtualKey.Number6;
            case Key.Number7: return Windows.System.VirtualKey.Number7;
            case Key.Number8: return Windows.System.VirtualKey.Number8;
            case Key.Number9: return Windows.System.VirtualKey.Number9;
            case Key.A: return Windows.System.VirtualKey.A;
            case Key.B: return Windows.System.VirtualKey.B;
            case Key.C: return Windows.System.VirtualKey.C;
            case Key.D: return Windows.System.VirtualKey.D;
            case Key.E: return Windows.System.VirtualKey.E;
            case Key.F: return Windows.System.VirtualKey.F;
            case Key.G: return Windows.System.VirtualKey.G;
            case Key.H: return Windows.System.VirtualKey.H;
            case Key.I: return Windows.System.VirtualKey.I;
            case Key.J: return Windows.System.VirtualKey.J;
            case Key.K: return Windows.System.VirtualKey.K;
            case Key.L: return Windows.System.VirtualKey.L;
            case Key.M: return Windows.System.VirtualKey.M;
            case Key.N: return Windows.System.VirtualKey.N;
            case Key.O: return Windows.System.VirtualKey.O;
            case Key.P: return Windows.System.VirtualKey.P;
            case Key.Q: return Windows.System.VirtualKey.Q;
            case Key.R: return Windows.System.VirtualKey.R;
            case Key.S: return Windows.System.VirtualKey.S;
            case Key.T: return Windows.System.VirtualKey.T;
            case Key.U: return Windows.System.VirtualKey.U;
            case Key.V: return Windows.System.VirtualKey.V;
            case Key.W: return Windows.System.VirtualKey.W;
            case Key.X: return Windows.System.VirtualKey.X;
            case Key.Y: return Windows.System.VirtualKey.Y;
            case Key.Z: return Windows.System.VirtualKey.Z;
            case Key.Escape: return Windows.System.VirtualKey.Escape;
            case Key.Enter: return Windows.System.VirtualKey.Enter;
            case Key.Tab: return Windows.System.VirtualKey.Tab;
            case Key.Backspace: return Windows.System.VirtualKey.Back;
            case Key.Insert: return Windows.System.VirtualKey.Insert;
            case Key.Delete: return Windows.System.VirtualKey.Delete;
            case Key.Right: return Windows.System.VirtualKey.Right;
            case Key.Left: return Windows.System.VirtualKey.Left;
            case Key.Down: return Windows.System.VirtualKey.Down;
            case Key.Up: return Windows.System.VirtualKey.Up;
            case Key.PageUp: return Windows.System.VirtualKey.PageUp;
            case Key.PageDown: return Windows.System.VirtualKey.PageDown;
            case Key.Home: return Windows.System.VirtualKey.Home;
            case Key.End: return Windows.System.VirtualKey.End;
            case Key.CapsLock: return Windows.System.VirtualKey.CapitalLock;
            case Key.ScrollLock: return Windows.System.VirtualKey.Scroll;
            case Key.NumLock: return Windows.System.VirtualKey.NumberKeyLock;
            case Key.PrintScreen: return Windows.System.VirtualKey.Print;
            case Key.Pause: return Windows.System.VirtualKey.Pause;
            case Key.F1: return Windows.System.VirtualKey.F1;
            case Key.F2: return Windows.System.VirtualKey.F2;
            case Key.F3: return Windows.System.VirtualKey.F3;
            case Key.F4: return Windows.System.VirtualKey.F4;
            case Key.F5: return Windows.System.VirtualKey.F5;
            case Key.F6: return Windows.System.VirtualKey.F6;
            case Key.F7: return Windows.System.VirtualKey.F7;
            case Key.F8: return Windows.System.VirtualKey.F8;
            case Key.F9: return Windows.System.VirtualKey.F9;
            case Key.F10: return Windows.System.VirtualKey.F10;
            case Key.F11: return Windows.System.VirtualKey.F11;
            case Key.F12: return Windows.System.VirtualKey.F12;
            case Key.F13: return Windows.System.VirtualKey.F13;
            case Key.F14: return Windows.System.VirtualKey.F14;
            case Key.F15: return Windows.System.VirtualKey.F15;
            case Key.F16: return Windows.System.VirtualKey.F16;
            case Key.F17: return Windows.System.VirtualKey.F17;
            case Key.F18: return Windows.System.VirtualKey.F18;
            case Key.F19: return Windows.System.VirtualKey.F19;
            case Key.F20: return Windows.System.VirtualKey.F20;
            case Key.F21: return Windows.System.VirtualKey.F21;
            case Key.F22: return Windows.System.VirtualKey.F22;
            case Key.F23: return Windows.System.VirtualKey.F23;
            case Key.F24: return Windows.System.VirtualKey.F24;
            case Key.Keypad0: return Windows.System.VirtualKey.NumberPad0;
            case Key.Keypad1: return Windows.System.VirtualKey.NumberPad1;
            case Key.Keypad2: return Windows.System.VirtualKey.NumberPad2;
            case Key.Keypad3: return Windows.System.VirtualKey.NumberPad3;
            case Key.Keypad4: return Windows.System.VirtualKey.NumberPad4;
            case Key.Keypad5: return Windows.System.VirtualKey.NumberPad5;
            case Key.Keypad6: return Windows.System.VirtualKey.NumberPad6;
            case Key.Keypad7: return Windows.System.VirtualKey.NumberPad7;
            case Key.Keypad8: return Windows.System.VirtualKey.NumberPad8;
            case Key.Keypad9: return Windows.System.VirtualKey.NumberPad9;
            case Key.KeypadDecimal: return Windows.System.VirtualKey.Decimal;
            case Key.KeypadDivide: return Windows.System.VirtualKey.Divide;
            case Key.KeypadMultiply: return Windows.System.VirtualKey.Multiply;
            case Key.KeypadSubtract: return Windows.System.VirtualKey.Subtract;
            case Key.KeypadAdd: return Windows.System.VirtualKey.Add;
            case Key.KeypadEnter: return Windows.System.VirtualKey.Enter;
            case Key.ShiftLeft: return Windows.System.VirtualKey.LeftShift;
            case Key.ControlLeft: return Windows.System.VirtualKey.LeftControl;
            case Key.AltLeft: return Windows.System.VirtualKey.LeftMenu;
            case Key.SuperLeft: return Windows.System.VirtualKey.LeftWindows;
            case Key.ShiftRight: return Windows.System.VirtualKey.RightShift;
            case Key.ControlRight: return Windows.System.VirtualKey.RightControl;
            case Key.AltRight: return Windows.System.VirtualKey.RightMenu;
            case Key.SuperRight: return Windows.System.VirtualKey.RightWindows;
            case Key.Menu: return Windows.System.VirtualKey.Menu;
            default:
                return Windows.System.VirtualKey.None;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>cf. https://learn.microsoft.com/en-us/windows/win32/inputdev/about-keyboard-input#scan-codes</remarks>
    /// <param name="key"></param>
    /// <returns></returns>
    public static uint ToWin32ScanCode(this Key key)
    {
        switch (key)
        {
            case Key.A: return PInvoke.DIK_A;
            case Key.B: return PInvoke.DIK_B;
            case Key.C: return PInvoke.DIK_C;
            case Key.D: return PInvoke.DIK_D;
            case Key.E: return PInvoke.DIK_E;
            case Key.F: return PInvoke.DIK_F;
            case Key.G: return PInvoke.DIK_G;
            case Key.H: return PInvoke.DIK_H;
            case Key.I: return PInvoke.DIK_I;
            case Key.J: return PInvoke.DIK_J;
            case Key.K: return PInvoke.DIK_K;
            case Key.L: return PInvoke.DIK_L;
            case Key.M: return PInvoke.DIK_M;
            case Key.N: return PInvoke.DIK_N;
            case Key.O: return PInvoke.DIK_O;
            case Key.P: return PInvoke.DIK_P;
            case Key.Q: return PInvoke.DIK_Q;
            case Key.R: return PInvoke.DIK_R;
            case Key.S: return PInvoke.DIK_S;
            case Key.T: return PInvoke.DIK_T;
            case Key.U: return PInvoke.DIK_U;
            case Key.V: return PInvoke.DIK_V;
            case Key.W: return PInvoke.DIK_W;
            case Key.X: return PInvoke.DIK_X;
            case Key.Y: return PInvoke.DIK_Y;
            case Key.Z: return PInvoke.DIK_Z;
            default:
                return 0;
        }
    }
}
