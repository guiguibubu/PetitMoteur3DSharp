using System;
using System.Runtime.InteropServices;
using PetitMoteur3D;
using PetitMoteur3D.Window;
using PetitMoteur3D.Window.SilkNet;

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    throw new PlatformNotSupportedException("Currently this engine only supports DirectX 11 so is Windows only or DX on VK on Linux");
}

WindowOptions options = WindowOptions.Default;
options.WindowState = WindowState.Maximized;
options.Title = "PetitMoteur3D";

using (IWindow window = SilkWindow.Create(options))
{
    Engine engine = new();
    engine.Run(window);

    //dispose the window, and its internal resources
    window.Dispose();
}
