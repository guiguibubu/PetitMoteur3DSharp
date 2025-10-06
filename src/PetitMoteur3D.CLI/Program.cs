using System;
using System.Runtime.InteropServices;
using PetitMoteur3D;

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    throw new NotSupportedException("Currently this engine only supports DirectX 11 so is Windows only");
}
Engine engine = new();
engine.Run();
