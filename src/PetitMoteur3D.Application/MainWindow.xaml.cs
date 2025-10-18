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
            PetitMoteur3D.Window.WinUIWindow window = new(this, DXSwapChainPanel);
            Thread.CurrentThread.Name = "UiThread";
            bool engineThreadStarted = ThreadPool.QueueUserWorkItem<PetitMoteur3D.Window.WinUIWindow>(EngineAction, window, true);
        }

        private void EngineAction(PetitMoteur3D.Window.WinUIWindow? window)
        {
            ArgumentNullException.ThrowIfNull(window);
            Thread.CurrentThread.Name = "EngineThread";
            Engine engine = new(window, null);
            engine.Initialized += () =>
            {
                bool success = DXSwapChainPanel.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, () =>
                {
                    SetSwapwhain(engine);
                });
            };
            engine.Initialize();

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

            engine.Run();
        }

        private void SetSwapwhain(Engine engine)
        {
            WinUIDesktopInterop.ISwapChainPanelNative swapChainPanelNative = ((IWinRTObject)DXSwapChainPanel).NativeObject.AsInterface<WinUIDesktopInterop.ISwapChainPanelNative>();
            unsafe
            {
                swapChainPanelNative.SetSwapChain((nint)engine.DeviceD3D11.Swapchain.Handle);
            }
        }

        private void Window_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
        {

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
