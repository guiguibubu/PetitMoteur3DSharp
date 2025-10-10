// Uncomment to have add a log of draw commands
//#define DEBUG_LOG_DRAW_COMMANDS

using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Numerics;
using ImGuiNET;
using OpenTelemetry.Metrics;
using PetitMoteur3D.Window;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace PetitMoteur3D.DebugGui
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Adaptation pour DX11 du ImGuiController officiel de Silk.NET (https://github.com/dotnet/Silk.NET/blob/main/src/OpenGL/Extensions/Silk.NET.OpenGL.Extensions.ImGui/ImGuiController.cs)
    /// </remarks>
    internal class ImGuiController : IDisposable
    {
        private readonly IImGuiBackendRenderer _backendRenderer;
        private readonly IWindow _view;
        private readonly IInputContext? _input;
        private bool _frameBegun;
        private readonly List<char> _pressedChars = new();
        private readonly IKeyboard? _keyboard;
        private float _windowWidth;
        private float _windowHeight;

        public IntPtr ContextPtr;

        #region Telemetry
        // Define a meter
        private static readonly Meter MyMeter = new("PetitMoteur3D.DebugGui.ImGuiController", "1.0");

        // Create a counter instrument
        private static readonly Gauge<long> CmdListsCounter = MyMeter.CreateGauge<long>("CmdListsCounter", "cmd list count", "Command list count");
        private static readonly Gauge<long> TotalIdxCounter = MyMeter.CreateGauge<long>("TotalIdxCounter", "index count", "Index count");
        private static readonly Gauge<long> TotalVtxCounter = MyMeter.CreateGauge<long>("TotalVtxCounter", "vertex count", "Vertex count");

        private MeterProvider _meterProvider;
        #endregion

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImGuiController(IImGuiBackendRenderer backendRenderer, IWindow view, IInputContext? input) : this(backendRenderer, view, input, null)
        {
        }

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImGuiController(DeviceD3D11 renderDevice, GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, GraphicPipelineFactory pipelineFactory, IWindow view, IInputContext? input)
            : this(new ImGuiImplDX11(renderDevice, graphicDeviceRessourceFactory, pipelineFactory), view, input, null)
        {
        }

        /// <summary>
        /// Constructs a new ImGuiController with font configuration and onConfigure Action.
        /// </summary>
        public ImGuiController(IImGuiBackendRenderer backendRenderer, IWindow view, IInputContext? input, Action? onConfigureIO)
        {
            _backendRenderer = backendRenderer;
            _view = view;
            _input = input;
            _windowWidth = view.Size.Width;
            _windowHeight = view.Size.Height;

            ContextPtr = ImGuiNET.ImGui.CreateContext();
            ImGuiNET.ImGui.SetCurrentContext(ContextPtr);
            // Setup Dear ImGui style
            ImGuiNET.ImGui.StyleColorsDark();

            ImGuiIOPtr io = ImGuiNET.ImGui.GetIO();

            onConfigureIO?.Invoke();

            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;     // Enable Keyboard Controls
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;      // Enable Gamepad Controls

            io.Fonts.AddFontDefault();

            SetPerFrameImGuiData(1f / 60f);

            if (_input is not null)
            {
                _keyboard = _input.Keyboards[0];
            }
            InitCallbacks();

            _backendRenderer.Init(in io);

            // Configure the OpenTelemetry MeterProvider
            _meterProvider = OpenTelemetry.Sdk.CreateMeterProviderBuilder()
                .AddMeter("PetitMoteur3D.DebugGui.ImGuiController")
                .AddConsoleExporter()
                .Build();
        }

        ~ImGuiController()
        {
            Dispose(disposing: false);
        }

        public void MakeCurrent()
        {
            ImGuiNET.ImGui.SetCurrentContext(ContextPtr);
        }

        private void InitCallbacks()
        {
            _view.Resize += WindowResized;
            if (_keyboard is not null)
            {
                _keyboard.KeyDown += OnKeyDown;
                _keyboard.KeyUp += OnKeyUp;
                _keyboard.KeyChar += OnKeyChar;
            }
        }

        /// <summary>
        /// Start a new ImGui frame. Call <see cref="Render"/> to render it.
        /// </summary>
        public void NewFrame()
        {
            _backendRenderer.NewFrame();
            ImGuiNET.ImGui.NewFrame();
            _frameBegun = true;
        }

        /// <summary>
        /// Close current frame. Call <see cref="Render"/> to render it.
        /// </summary>
        public void CloseFrame()
        {
            _frameBegun = false;
        }

        /// <summary>
        /// Delegate to receive keyboard key down events.
        /// </summary>
        /// <param name="keyboard">The keyboard context generating the event.</param>
        /// <param name="keycode">The native keycode of the pressed key.</param>
        /// <param name="scancode">The native scancode of the pressed key.</param>
        private static void OnKeyDown(IKeyboard keyboard, Key keycode, int scancode) =>
            OnKeyEvent(keyboard, keycode, scancode, down: true);

        /// <summary>
        /// Delegate to receive keyboard key up events.
        /// </summary>
        /// <param name="keyboard">The keyboard context generating the event.</param>
        /// <param name="keycode">The native keycode of the released key.</param>
        /// <param name="scancode">The native scancode of the released key.</param>
        private static void OnKeyUp(IKeyboard keyboard, Key keycode, int scancode) =>
            OnKeyEvent(keyboard, keycode, scancode, down: false);

        /// <summary>
        /// Delegate to receive keyboard key events.
        /// </summary>
        /// <param name="keyboard">The keyboard context generating the event.</param>
        /// <param name="keycode">The native keycode of the key generating the event.</param>
        /// <param name="scancode">The native scancode of the key generating the event.</param>
        /// <param name="down">True if the event is a key down event, otherwise False</param>
        private static void OnKeyEvent(IKeyboard keyboard, Key keycode, int scancode, bool down)
        {
            var io = ImGuiNET.ImGui.GetIO();
            var imGuiKey = TranslateInputKeyToImGuiKey(keycode);
            io.AddKeyEvent(imGuiKey, down);
            io.SetKeyEventNativeData(imGuiKey, (int)keycode, scancode);
        }

        private void OnKeyChar(IKeyboard arg1, char arg2)
        {
            _pressedChars.Add(arg2);
        }

        private void WindowResized(Size size)
        {
            _windowWidth = size.Width;
            _windowHeight = size.Height;
        }

#if DEBUG && DEBUG_LOG_DRAW_COMMANDS
        private uint totalIndexToDraw = 0;
#endif
        /// <summary>
        /// Renders the ImGui draw list data.
        /// Nothing happen if no frame is open. Use <see cref="NewFrame"/> to do it.
        /// </summary>
        public void Render(bool autoCloseFrame = true)
        {
            if (!_frameBegun)
            {
                return;
            }

            nint oldCtx = ImGuiNET.ImGui.GetCurrentContext();

            if (oldCtx != ContextPtr)
            {
                ImGuiNET.ImGui.SetCurrentContext(ContextPtr);
            }

            if (autoCloseFrame)
            {
                CloseFrame();
            }

            ImGuiNET.ImGui.Render();
            ImDrawDataPtr drawDataPtr = ImGuiNET.ImGui.GetDrawData();
            RenderImDrawData(in drawDataPtr);

#if DEBUG && DEBUG_LOG_DRAW_COMMANDS
            uint totalIndexToDrawTemp = 0;
            System.Console.WriteLine("drawDataPtr.CmdListsCount = " + drawDataPtr.CmdListsCount);
            for (int i = 0; i < drawDataPtr.CmdListsCount; i++)
            {
                ImDrawListPtr cmdList = drawDataPtr.CmdLists[i];
                System.Console.WriteLine("cmdList.CmdBuffer.Size = " + cmdList.CmdBuffer.Size);
                for (int j = 0; j < cmdList.CmdBuffer.Size; j++)
                {
                    ImDrawCmdPtr cmd = cmdList.CmdBuffer[j];
                    uint nbIndexToDraw = cmd.ElemCount;
                    System.Console.WriteLine("cmd.ElemCount = " + cmd.ElemCount);
                    totalIndexToDrawTemp += nbIndexToDraw;
                }
            }

            if (totalIndexToDrawTemp != totalIndexToDraw)
            {
                totalIndexToDraw = totalIndexToDrawTemp;
                System.Console.WriteLine("totalIndexToDraw = " + totalIndexToDraw);
            }
#endif

            // Not used yet, but ready for telemetry
            /*
            CmdListsCounter.Record(drawDataPtr.CmdListsCount);
            TotalIdxCounter.Record(drawDataPtr.TotalIdxCount);
            TotalVtxCounter.Record(drawDataPtr.TotalVtxCount);
            */

            if (oldCtx != ContextPtr)
            {
                ImGuiNET.ImGui.SetCurrentContext(oldCtx);
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// Nothing append if a frame has begun. You need to render it before with <see cref="Render"/>.
        /// </summary>
        public void Update(float deltaSeconds)
        {
            if (_frameBegun)
            {
                return;
            }

            nint oldCtx = ImGuiNET.ImGui.GetCurrentContext();

            if (oldCtx != ContextPtr)
            {
                ImGuiNET.ImGui.SetCurrentContext(ContextPtr);
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput();

            if (oldCtx != ContextPtr)
            {
                ImGuiNET.ImGui.SetCurrentContext(oldCtx);
            }
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGuiNET.ImGui.GetIO();
            io.DisplaySize.X = _windowWidth;
            io.DisplaySize.Y = _windowHeight;

            if (_windowWidth > 0 && _windowHeight > 0)
            {
                io.DisplayFramebufferScale.X = _view.FramebufferSize.Width / _windowWidth;
                io.DisplayFramebufferScale.Y = _view.FramebufferSize.Height / _windowHeight;
            }

            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }

        private void UpdateImGuiInput()
        {
            if(_input is null)
            {
                return;
            }

            ImGuiIOPtr io = ImGuiNET.ImGui.GetIO();

            IMouse mouseState = _input.Mice[0];

            io.MouseDown[0] = mouseState.IsButtonPressed(MouseButton.Left);
            io.MouseDown[1] = mouseState.IsButtonPressed(MouseButton.Right);
            io.MouseDown[2] = mouseState.IsButtonPressed(MouseButton.Middle);

            io.MousePos.X = (int)mouseState.Position.X;
            io.MousePos.Y = (int)mouseState.Position.Y;

            ScrollWheel wheel = mouseState.ScrollWheels[0];
            io.MouseWheel = wheel.Y;
            io.MouseWheelH = wheel.X;

            foreach (char c in _pressedChars)
            {
                io.AddInputCharacter(c);
            }

            _pressedChars.Clear();

            if (_keyboard is not null)
            {
                io.KeyCtrl = _keyboard.IsKeyPressed(Key.ControlLeft) || _keyboard.IsKeyPressed(Key.ControlRight);
                io.KeyAlt = _keyboard.IsKeyPressed(Key.AltLeft) || _keyboard.IsKeyPressed(Key.AltRight);
                io.KeyShift = _keyboard.IsKeyPressed(Key.ShiftLeft) || _keyboard.IsKeyPressed(Key.ShiftRight);
                io.KeySuper = _keyboard.IsKeyPressed(Key.SuperLeft) || _keyboard.IsKeyPressed(Key.SuperRight);
            }
        }

        internal void PressChar(char keyChar)
        {
            _pressedChars.Add(keyChar);
        }

        /// <summary>
        /// Translates a Silk.NET.Input.Key to an ImGuiKey.
        /// </summary>
        /// <param name="key">The Silk.NET.Input.Key to translate.</param>
        /// <returns>The corresponding ImGuiKey.</returns>
        /// <exception cref="NotImplementedException">When the key has not been implemented yet.</exception>
        private static ImGuiKey TranslateInputKeyToImGuiKey(Key key)
        {
            return key switch
            {
                Key.Tab => ImGuiKey.Tab,
                Key.Left => ImGuiKey.LeftArrow,
                Key.Right => ImGuiKey.RightArrow,
                Key.Up => ImGuiKey.UpArrow,
                Key.Down => ImGuiKey.DownArrow,
                Key.PageUp => ImGuiKey.PageUp,
                Key.PageDown => ImGuiKey.PageDown,
                Key.Home => ImGuiKey.Home,
                Key.End => ImGuiKey.End,
                Key.Insert => ImGuiKey.Insert,
                Key.Delete => ImGuiKey.Delete,
                Key.Backspace => ImGuiKey.Backspace,
                Key.Space => ImGuiKey.Space,
                Key.Enter => ImGuiKey.Enter,
                Key.Escape => ImGuiKey.Escape,
                Key.Apostrophe => ImGuiKey.Apostrophe,
                Key.Comma => ImGuiKey.Comma,
                Key.Minus => ImGuiKey.Minus,
                Key.Period => ImGuiKey.Period,
                Key.Slash => ImGuiKey.Slash,
                Key.Semicolon => ImGuiKey.Semicolon,
                Key.Equal => ImGuiKey.Equal,
                Key.LeftBracket => ImGuiKey.LeftBracket,
                Key.BackSlash => ImGuiKey.Backslash,
                Key.RightBracket => ImGuiKey.RightBracket,
                Key.GraveAccent => ImGuiKey.GraveAccent,
                Key.CapsLock => ImGuiKey.CapsLock,
                Key.ScrollLock => ImGuiKey.ScrollLock,
                Key.NumLock => ImGuiKey.NumLock,
                Key.PrintScreen => ImGuiKey.PrintScreen,
                Key.Pause => ImGuiKey.Pause,
                Key.Keypad0 => ImGuiKey.Keypad0,
                Key.Keypad1 => ImGuiKey.Keypad1,
                Key.Keypad2 => ImGuiKey.Keypad2,
                Key.Keypad3 => ImGuiKey.Keypad3,
                Key.Keypad4 => ImGuiKey.Keypad4,
                Key.Keypad5 => ImGuiKey.Keypad5,
                Key.Keypad6 => ImGuiKey.Keypad6,
                Key.Keypad7 => ImGuiKey.Keypad7,
                Key.Keypad8 => ImGuiKey.Keypad8,
                Key.Keypad9 => ImGuiKey.Keypad9,
                Key.KeypadDecimal => ImGuiKey.KeypadDecimal,
                Key.KeypadDivide => ImGuiKey.KeypadDivide,
                Key.KeypadMultiply => ImGuiKey.KeypadMultiply,
                Key.KeypadSubtract => ImGuiKey.KeypadSubtract,
                Key.KeypadAdd => ImGuiKey.KeypadAdd,
                Key.KeypadEnter => ImGuiKey.KeypadEnter,
                Key.KeypadEqual => ImGuiKey.KeypadEqual,
                Key.ShiftLeft => ImGuiKey.LeftShift,
                Key.ControlLeft => ImGuiKey.LeftCtrl,
                Key.AltLeft => ImGuiKey.LeftAlt,
                Key.SuperLeft => ImGuiKey.LeftSuper,
                Key.ShiftRight => ImGuiKey.RightShift,
                Key.ControlRight => ImGuiKey.RightCtrl,
                Key.AltRight => ImGuiKey.RightAlt,
                Key.SuperRight => ImGuiKey.RightSuper,
                Key.Menu => ImGuiKey.Menu,
                Key.Number0 => ImGuiKey._0,
                Key.Number1 => ImGuiKey._1,
                Key.Number2 => ImGuiKey._2,
                Key.Number3 => ImGuiKey._3,
                Key.Number4 => ImGuiKey._4,
                Key.Number5 => ImGuiKey._5,
                Key.Number6 => ImGuiKey._6,
                Key.Number7 => ImGuiKey._7,
                Key.Number8 => ImGuiKey._8,
                Key.Number9 => ImGuiKey._9,
                Key.A => ImGuiKey.A,
                Key.B => ImGuiKey.B,
                Key.C => ImGuiKey.C,
                Key.D => ImGuiKey.D,
                Key.E => ImGuiKey.E,
                Key.F => ImGuiKey.F,
                Key.G => ImGuiKey.G,
                Key.H => ImGuiKey.H,
                Key.I => ImGuiKey.I,
                Key.J => ImGuiKey.J,
                Key.K => ImGuiKey.K,
                Key.L => ImGuiKey.L,
                Key.M => ImGuiKey.M,
                Key.N => ImGuiKey.N,
                Key.O => ImGuiKey.O,
                Key.P => ImGuiKey.P,
                Key.Q => ImGuiKey.Q,
                Key.R => ImGuiKey.R,
                Key.S => ImGuiKey.S,
                Key.T => ImGuiKey.T,
                Key.U => ImGuiKey.U,
                Key.V => ImGuiKey.V,
                Key.W => ImGuiKey.W,
                Key.X => ImGuiKey.X,
                Key.Y => ImGuiKey.Y,
                Key.Z => ImGuiKey.Z,
                Key.F1 => ImGuiKey.F1,
                Key.F2 => ImGuiKey.F2,
                Key.F3 => ImGuiKey.F3,
                Key.F4 => ImGuiKey.F4,
                Key.F5 => ImGuiKey.F5,
                Key.F6 => ImGuiKey.F6,
                Key.F7 => ImGuiKey.F7,
                Key.F8 => ImGuiKey.F8,
                Key.F9 => ImGuiKey.F9,
                Key.F10 => ImGuiKey.F10,
                Key.F11 => ImGuiKey.F11,
                Key.F12 => ImGuiKey.F12,
                Key.F13 => ImGuiKey.F13,
                Key.F14 => ImGuiKey.F14,
                Key.F15 => ImGuiKey.F15,
                Key.F16 => ImGuiKey.F16,
                Key.F17 => ImGuiKey.F17,
                Key.F18 => ImGuiKey.F18,
                Key.F19 => ImGuiKey.F19,
                Key.F20 => ImGuiKey.F20,
                Key.F21 => ImGuiKey.F21,
                Key.F22 => ImGuiKey.F22,
                Key.F23 => ImGuiKey.F23,
                Key.F24 => ImGuiKey.F24,
                _ => throw new NotImplementedException(),
            };
        }

        private unsafe void RenderImDrawData(ref readonly ImDrawDataPtr drawDataPtr)
        {
            ref Vector2 displaySize = ref drawDataPtr.DisplaySize;
            ref Vector2 framebufferScale = ref drawDataPtr.FramebufferScale;
            int framebufferWidth = (int)(displaySize.X * framebufferScale.X);
            int framebufferHeight = (int)(displaySize.Y * framebufferScale.Y);
            if (framebufferWidth <= 0 || framebufferHeight <= 0)
                return;

            _backendRenderer.RenderDrawData(in drawDataPtr);
        }

        private bool _disposed = false;

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    _backendRenderer.Dispose();
                    _meterProvider.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                _view.Resize -= WindowResized;
                if (_keyboard is not null)
                {
                    _keyboard.KeyChar -= OnKeyChar;
                }

                ImGuiNET.ImGui.DestroyContext(ContextPtr);

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
