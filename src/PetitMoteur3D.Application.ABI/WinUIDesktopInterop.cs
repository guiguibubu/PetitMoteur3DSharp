#define PM3D_USEITEROP_LIBRARY_IMPORT

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PetitMoteur3D.Application.ABI;

public static partial class WinUIDesktopInterop
{
#if WINDOWS
    private const string DirectorySeparator = "\\";
#else
    private const string DirectorySeparator = "/";
#endif

    /// <summary>
    /// Interface from microsoft.ui.xaml.media.dxinterop.h
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("63aad0b8-7c24-40ff-85a8-640d944cc325")]
    public partial interface ISwapChainPanelNative
    {
        //public static readonly Guid IID = Guid.Parse("63aad0b8-7c24-40ff-85a8-640d944cc325");

        //[PreserveSig] 
        int SetSwapChain([In] IntPtr swapChain);
    }
#if PM3D_USEITEROP_LIBRARY_IMPORT
            [LibraryImport(
            "native" + DirectorySeparator + "PetitMoteur3D.Application.Native",
            EntryPoint = "SwapchainPanelNativeWrapper_Add", StringMarshalling = StringMarshalling.Utf8)]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
    public static partial Int32 Add(Int32 a, Int32 b);
#else
    [DllImport(
        "PetitMoteur3D.Application.Native",
        EntryPoint = "SwapchainPanelNativeWrapper_Add",
        CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 Add(Int32 a, Int32 b);
#endif
}
