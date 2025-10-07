using System;
using System.Runtime.InteropServices;
using PetitMoteur3D;

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    throw new PlatformNotSupportedException("Currently this engine only supports DirectX 11 so is Windows only or DX on VK on Linux");
}
Engine engine = new();
engine.Run();
