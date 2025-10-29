//#define PM3D_USE_RUNTIME_INTEROP
using System;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using PetitMoteur3D.Input;
using PetitMoteur3D.Input.WinUI;
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

        private DispatcherQueueController _dispatcherQueueController;

        private void DXSwapChainPanel_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] DXSwapChainPanel_Loaded Begin");
            Thread.CurrentThread.Name = "UiThread";
            PetitMoteur3D.Window.WinUI.WinUIWindow window = new(this.AppWindow, DXSwapChainPanel);
            DXSwapChainPanel.Focus(FocusState.Programmatic);
            _dispatcherQueueController.DispatcherQueue.TryEnqueue(() => EngineAction(window));
            System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] DXSwapChainPanel_Loaded End");
        }

        private void EngineAction(PetitMoteur3D.Window.WinUI.WinUIWindow window)
        {
            System.Diagnostics.Trace.WriteLine("[PetitMoteur3D] EngineAction Begin");
            try
            {
                Thread.CurrentThread.Name = "EngineThread";
                WinUiInputPlatform platform = new();
                IInputContext inputContext = platform.CreateInput(window);
                Engine engine = new(window, inputContext);
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
            IObjectReference nativeReference = ((IWinRTObject)DXSwapChainPanel.SwapChainPanel).NativeObject;
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
            _dispatcherQueueController = DispatcherQueueController.CreateOnDedicatedThread();
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            _dispatcherQueueController.ShutdownQueue();
        }

        private void DXSwapChainPanel_GettingFocus(UIElement sender, Microsoft.UI.Xaml.Input.GettingFocusEventArgs args)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }
        }
    }
}
