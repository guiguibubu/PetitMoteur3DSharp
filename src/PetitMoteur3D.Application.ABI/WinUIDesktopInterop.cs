#if !PM3D_USE_RUNTIME_INTEROP && !PM3D_USEITEROP_LIBRARY_IMPORT
#define PM3D_USEITEROP_LIBRARY_IMPORT
#endif
#if PM3D_USE_RUNTIME_INTEROP && !PM3D_USECOM_IMPORT
#define PM3D_USECOM_IMPORT
#endif

using System;
using System.Runtime.InteropServices;

namespace PetitMoteur3D.Application.ABI;

public static partial class WinUIDesktopInterop
{
#if WINDOWS
    private const string DirectorySeparator = "\\";
#else
    private const string DirectorySeparator = "/";
#endif

    private const string NativeLibraryPath = "native" + DirectorySeparator + "PetitMoteur3D.Application.Native";
    private const string SwapchainPanelNativeWrapper_Add_Name = "SwapchainPanelNativeWrapper_Add";
    private const string SwapchainPanelNativeWrapper_SetSwapchain_Name = "SwapchainPanelNativeWrapper_SetSwapchain";

    /// <summary>
    /// Interface from microsoft.ui.xaml.media.dxinterop.h
    /// </summary>
#if PM3D_USECOM_IMPORT
    [ComImport]
#else
    [System.Runtime.InteropServices.Marshalling.GeneratedComInterface(
        Options = System.Runtime.InteropServices.Marshalling.ComInterfaceOptions.ComObjectWrapper, 
        StringMarshalling = StringMarshalling.Utf8)]
#endif
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("63aad0b8-7c24-40ff-85a8-640d944cc325")]
    public partial interface ISwapChainPanelNative
    {
        public static readonly Guid IID = Guid.Parse("63aad0b8-7c24-40ff-85a8-640d944cc325");

        [PreserveSig]
        Int32 SetSwapChain(IntPtr swapChain);
    }
#if PM3D_USEITEROP_LIBRARY_IMPORT
    [LibraryImport(NativeLibraryPath,
            EntryPoint = SwapchainPanelNativeWrapper_Add_Name, StringMarshalling = StringMarshalling.Utf8)]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Int32 Add(Int32 a, Int32 b);

    [LibraryImport(NativeLibraryPath,
            EntryPoint = SwapchainPanelNativeWrapper_SetSwapchain_Name, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial Int32 SetSwapchain(IntPtr panelIUnknown, IntPtr swapChain);
#else
    [DllImport(NativeLibraryPath,
        EntryPoint = SwapchainPanelNativeWrapper_Add_Name,
        CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 Add(Int32 a, Int32 b);
    [DllImport(NativeLibraryPath,
        EntryPoint = SwapchainPanelNativeWrapper_SetSwapchain_Name,
        CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 SetSwapchain(IntPtr panelIUnknown, IntPtr swapChain);
#endif
}
