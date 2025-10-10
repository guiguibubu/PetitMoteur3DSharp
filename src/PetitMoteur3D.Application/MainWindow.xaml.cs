using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.UI.Xaml;
using Windows.Win32.System.Com;

using ISwapChainPanelNative = Windows.Win32.System.WinRT.Xaml.ISwapChainPanelNative;

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
            this.AppWindow.AssociateWithDispatcherQueue(this.DispatcherQueue);
            PetitMoteur3D.Window.WinUIWindow window = new(this.AppWindow);
            Engine engine = new(window);
            engine.Initialized += () =>
            {
                nint swapChainPtr = ABI.Microsoft.UI.Xaml.Controls.SwapChainPanel.FromManaged(DXSwapChainPanel);
                unsafe
                {
                    ref IUnknown comUnknown = ref Unsafe.AsRef<IUnknown>(swapChainPtr.ToPointer());
                    comUnknown.QueryInterface<ISwapChainPanelNative>(out ISwapChainPanelNative* swapChainPanelNativePtr);
                    ref ISwapChainPanelNative swapChainPanelNative = ref Unsafe.AsRef<ISwapChainPanelNative>(swapChainPanelNativePtr);
                    swapChainPanelNative.SetSwapChain((Windows.Win32.Graphics.Dxgi.IDXGISwapChain*)engine.DeviceD3D11.Swapchain.Handle);
                }
            };
            Thread engineThread = new Thread(engine.Run);
            engineThread.Start();
        }
    }
}
