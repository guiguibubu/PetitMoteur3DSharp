using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PetitMoteur3D;
using PetitMoteur3D.Input;
using PetitMoteur3D.Input.SilkNet;
using PetitMoteur3D.Logging;
using PetitMoteur3D.Window;
using PetitMoteur3D.Window.SilkNet;
using Serilog;

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    throw new PlatformNotSupportedException("Currently this engine only supports DirectX 11 so is Windows only or DX on VK on Linux");
}

WindowOptions options = WindowOptions.Default;
options.WindowState = WindowState.Maximized;
options.Title = "PetitMoteur3D";

SilkInputPlatform platform = new SilkInputPlatform();

try
{
    string logFilePath = $"logs/{PetitMoteur3D.Logging.Log.GenerateLogFileName()}";
    PetitMoteur3D.Logging.Log.Logger = new Serilog.LoggerConfiguration()
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Information()
#endif
#if DEBUG
        .WriteTo.Debug()
#endif
        .WriteTo.Console()
        .WriteTo.Async(c => c.File(logFilePath))
        .CreateLogger();

    using (IWindow window = SilkWindow.Create(options))
    {
        window.Initialize();
        IInputContext inputContext = platform.CreateInput(window);
        EngineConfiguration conf = new(in window)
        {
            Window = window,
            InputContext = inputContext
        };
        Engine engine = new(in conf);
        engine.Initialize();
        engine.Run();
    }
}
finally
{
    Serilog.Log.CloseAndFlush();
}
