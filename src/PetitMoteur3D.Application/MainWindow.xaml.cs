using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using WinRT;
using WinRT.Interop;

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
            System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] DXSwapChainPanel_Loaded Begin");
            PetitMoteur3D.Window.WinUIWindow window = new(this, DXSwapChainPanel);
            Thread.CurrentThread.Name = "UiThread";
            bool engineThreadStarted = ThreadPool.QueueUserWorkItem<PetitMoteur3D.Window.WinUIWindow>(EngineAction, window, true);
            System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] DXSwapChainPanel_Loaded End");
        }

        private void EngineAction(PetitMoteur3D.Window.WinUIWindow? window)
        {
            ArgumentNullException.ThrowIfNull(window);
            System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] EngineAction Begin");
            try
            {
                Thread.CurrentThread.Name = "EngineThread";
                Engine engine = new(window, null);
                engine.Initialized += () =>
                {
                    bool success = DXSwapChainPanel.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () =>
                    {
                        SetSwapwhain(engine);
                    });
                };
                System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] EngineAction Engine Initialzation");
                engine.Initialize();
                System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] EngineAction Engine Initialzation Finished");

                // IMPLEMENTATION POUR LA SOURIE A FAIRE PLUS TARD
                // The CoreIndependentInputSource will raise pointer events for the specified device types on whichever thread it's created on.
                //InputPointerSource m_coreInput = DXSwapChainPanel.CreateCoreIndependentInputSource(
                //    InputPointerSourceDeviceKinds.Mouse
                //    );

                //// Register for pointer events, which will be raised on the background thread.
                //m_coreInput.PointerPressed += ref new TypedEventHandler<Object^, PointerEventArgs ^> (this, &DrawingPanel::OnPointerPressed);
                //m_coreInput.PointerMoved += ref new TypedEventHandler<Object^, PointerEventArgs ^> (this, &DrawingPanel::OnPointerMoved);
                //m_coreInput.PointerReleased += ref new TypedEventHandler<Object^, PointerEventArgs ^> (this, &DrawingPanel::OnPointerReleased);

                //// Begin processing input messages as they're delivered.
                //m_coreInput.DispatcherQueue.RunEventLoop();

                System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] EngineAction Engine Run");
                //engine.Run();
                System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] EngineAction Engine Run Finished");
            }
            catch (Exception)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
                throw;
            }
            System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] EngineAction End");
        }

        private void SetSwapwhain(Engine engine)
        {
            System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] SetSwapwhain Begin");
            IObjectReference nativeReference = ((IWinRTObject)DXSwapChainPanel).NativeObject;
            //WinUIDesktopInterop.ISwapChainPanelNative swapChainPanelNative = nativeReference.AsInterface<WinUIDesktopInterop.ISwapChainPanelNative>();
            Guid iid = typeof(WinUIDesktopInterop.ISwapChainPanelNative).GUID;
            //int errorCode2 = System.Runtime.InteropServices.Marshal.QueryInterface(((IWinRTObject)DXSwapChainPanel).NativeObject.ThisPtr, WinRT.Interop.IID.IID_IUnknown, out var ppv2);
            int errorCode = nativeReference.TryAs(iid, out nint ppv);
            System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] SetSwapwhain errorCode = " + errorCode);
            System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(errorCode);

            int a = 1;
            int b = 2;
            int c = WinUIDesktopInterop.Add(a, b);

            try
            {
                //object iunknnown2 = System.Runtime.InteropServices.Marshal.GetTypedObjectForIUnknown(ppv, typeof(WinUIDesktopInterop.ISwapChainPanelNative));
                //object iunknnown = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(ppv);
                unsafe
                {
                    WinUIDesktopInterop.ISwapChainPanelNative* swapChainPanelNativePtr = (WinUIDesktopInterop.ISwapChainPanelNative*)(ppv.ToPointer());
                    swapChainPanelNativePtr->SetSwapChain((nint)engine.DeviceD3D11.Swapchain.Handle);
                    WinUIDesktopInterop.ISwapChainPanelNative swapChainPanelNative = Unsafe.AsRef<WinUIDesktopInterop.ISwapChainPanelNative>(ppv.ToPointer());
                    //object ttest = (WinUIDesktopInterop.ISwapChainPanelNative)System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(ppv);
                    System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] SetSwapwhain cast interface finished");
                    System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] SetSwapwhain SwapChainPanelNative.SetSwapChain");
                    swapChainPanelNative.SetSwapChain((nint)engine.DeviceD3D11.Swapchain.Handle);
                    System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] SetSwapwhain SwapChainPanelNative.SetSwapChain finished");
                }
            }
            catch(Exception ex)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
                throw;
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.Release(ppv);
            }
        }

        private void Window_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
        {

        }

        internal static partial class WinUIDesktopInterop
        {
            /// <summary>
            /// Interface from microsoft.ui.xaml.media.dxinterop.h
            /// </summary>
            [System.Runtime.InteropServices.ComImport]
            [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
            [System.Runtime.InteropServices.Guid("63aad0b8-7c24-40ff-85a8-640d944cc325")]
            public partial interface ISwapChainPanelNative
            {
                //public static readonly Guid IID = Guid.Parse("63aad0b8-7c24-40ff-85a8-640d944cc325");

                //[System.Runtime.InteropServices.PreserveSig] 
                int SetSwapChain([System.Runtime.InteropServices.In] IntPtr swapChain);
            }

            //[System.Runtime.InteropServices.LibraryImport(
            //"PetitMoteur3D.Application.Native",
            //EntryPoint = "SwapchainPanelNativeWrapper_Add", StringMarshalling = System.Runtime.InteropServices.StringMarshalling.Utf8)]
            //[System.Runtime.InteropServices.UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            //internal static partial Int32 Add(Int32 a, Int32 b);
            [System.Runtime.InteropServices.DllImport(
    "PetitMoteur3D.Application.Native",
    EntryPoint = "SwapchainPanelNativeWrapper_Add",
    CharSet = System.Runtime.InteropServices.CharSet.Unicode, ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
            internal static extern Int32 Add(Int32 a, Int32 b);
        }
    }
}
