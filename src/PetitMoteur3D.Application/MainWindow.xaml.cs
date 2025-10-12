using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.UI.Xaml;
using WinRT;
using IUnknown = Windows.Win32.System.Com.IUnknown;
//using ISwapChainPanelNative = Windows.Win32.System.WinRT.Xaml.ISwapChainPanelNative;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PetitMoteur3D.Application
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : global::Microsoft.UI.Xaml.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DXSwapChainPanel_Loaded(object sender, RoutedEventArgs e)
        {
            //this.AppWindow.AssociateWithDispatcherQueue(this.DispatcherQueue);
            PetitMoteur3D.Window.WinUIWindow window = new(this, DXSwapChainPanel);
            Engine engine = new(window);
            engine.Initialized += () =>
            {
                bool success = DXSwapChainPanel.DispatcherQueue.TryEnqueue(() =>
                {
                    SetSwapwhain(engine);
                });
            };
            engine.Initialize();
            //Thread engineThread = new Thread(engine.Run);
            //engineThread.Start();
            //engine.Run();
        }

        private void SetSwapwhain(Engine engine)
        {
            //Guid interfaceUnknownIID = WinRT.Interop.IID.IID_IUnknown;
            //Guid interfacceIIDTypeOf = typeof(WinUIDesktopInterop.ISwapChainPanelNative).GUID;
            //System.Runtime.InteropServices.CustomQueryInterfaceResult resultCodeUnknown = ((System.Runtime.InteropServices.ICustomQueryInterface)DXSwapChainPanel).GetInterface(ref interfaceUnknownIID, out nint comUnknownPtr);
            //System.Runtime.InteropServices.CustomQueryInterfaceResult resultCodeSwapChainPanelNative = ((System.Runtime.InteropServices.ICustomQueryInterface)DXSwapChainPanel).GetInterface(ref interfacceIIDTypeOf, out nint swapChainPanelNativePtr);
            //nint swapChainPtr = ((IWinRTObject)DXSwapChainPanel).NativeObject.ThisPtr;
            WinUIDesktopInterop.ISwapChainPanelNative swapChainPanelNative = ((IWinRTObject)DXSwapChainPanel).NativeObject.AsInterface<WinUIDesktopInterop.ISwapChainPanelNative>();
            unsafe
            {
                swapChainPanelNative.SetSwapChain((nint)engine.DeviceD3D11.Swapchain.Handle);
            }
            //unsafe
            //{
            //    ref IUnknown comUnknown = ref Unsafe.AsRef<IUnknown>(swapChainPtr.ToPointer());
            //    ref IUnknown comUnknown2 = ref Unsafe.AsRef<IUnknown>(comUnknownPtr2.ToPointer());
            //    comUnknown.QueryInterface<WinUIDesktopInterop.ISwapChainPanelNative>(out WinUIDesktopInterop.ISwapChainPanelNative* swapChainPanelNativePtr);
            //    comUnknown2.QueryInterface<WinUIDesktopInterop.ISwapChainPanelNative>(out WinUIDesktopInterop.ISwapChainPanelNative* swapChainPanelNativePtr3);
            //    ref ISwapChainPanelNative swapChainPanelNative = ref Unsafe.AsRef<ISwapChainPanelNative>(swapChainPanelNativePtr);
            //    swapChainPanelNative.SetSwapChain((Windows.Win32.Graphics.Dxgi.IDXGISwapChain*)engine.DeviceD3D11.Swapchain.Handle);
            //    swapChainPanelNative.SetSwapChain((Windows.Win32.Graphics.Dxgi.IDXGISwapChain*)engine.DeviceD3D11.Swapchain.Handle);
            //}
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            PetitMoteur3D.Window.WinUIWindow window = new(this, DXSwapChainPanel);
        }

        internal static class WinUIDesktopInterop
        {
            /// <summary>
            /// Interface from microsoft.ui.xaml.media.dxinterop.h
            /// </summary>
            [System.Runtime.InteropServices.ComImport]
            [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
            [System.Runtime.InteropServices.Guid("63aad0b8-7c24-40ff-85a8-640d944cc325")]
            public interface ISwapChainPanelNative
            {
                [System.Runtime.InteropServices.PreserveSig] uint SetSwapChain([System.Runtime.InteropServices.In] IntPtr swapChain);
            }
        }
    }
}
