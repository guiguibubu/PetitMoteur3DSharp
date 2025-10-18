using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PetitMoteur3D;
using PetitMoteur3D.Input;
using PetitMoteur3D.Input.SilkNet;
using PetitMoteur3D.Window;
using PetitMoteur3D.Window.SilkNet;

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    throw new PlatformNotSupportedException("Currently this engine only supports DirectX 11 so is Windows only or DX on VK on Linux");
}

var myWriter = new ConsoleTraceListener();
Trace.Listeners.Add(myWriter);

WindowOptions options = WindowOptions.Default;
options.WindowState = WindowState.Maximized;
options.Title = "PetitMoteur3D";

SilkInputPlatform platform = new SilkInputPlatform();
using (IWindow window = SilkWindow.Create(options))
{
    window.Initialize();
    IInputContext inputContext = platform.CreateInput(window);

    Engine engine = new(window, inputContext);
    engine.Initialize();
    engine.Run();

    //dispose the window, and its internal resources
    window.Dispose();
}
