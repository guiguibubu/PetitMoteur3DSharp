//#define PM3D_USE_RUNTIME_INTEROP
using System;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using WinRT;

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
                engine.Run();
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
            try
            {
                unsafe
                {
#if PM3D_USE_RUNTIME_INTEROP
                    ABI.WinUIDesktopInterop.ISwapChainPanelNative swapChainPanelNative = nativeReference.AsInterface<ABI.WinUIDesktopInterop.ISwapChainPanelNative>();
                    int errorCode = swapChainPanelNative.SetSwapChain((nint)engine.DeviceD3D11.Swapchain.Handle);
#else
                    int errorCode = ABI.WinUIDesktopInterop.SetSwapchain(nativeReference.ThisPtr, (nint)engine.DeviceD3D11.Swapchain.Handle);
#endif
                    System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] SetSwapwhain errorCode = " + errorCode);
                    System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(errorCode);
                }
            }
            catch (Exception)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
                throw;
            }
        }

        private void Window_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
        {

        }
    }
}
