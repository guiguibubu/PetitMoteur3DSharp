using Microsoft.UI.Input;

namespace PetitMoteur3D.Window.WinUI
{
    public class DXPanel : Microsoft.UI.Xaml.Controls.Grid
    {
        Microsoft.UI.Xaml.Controls.SwapChainPanel _swapChainPanel;
        public Microsoft.UI.Xaml.Controls.SwapChainPanel SwapChainPanel => _swapChainPanel;

        public DXPanel()
        {
            _swapChainPanel = new Microsoft.UI.Xaml.Controls.SwapChainPanel();
            this.Children.Add(_swapChainPanel);
        }
        public InputCursor GetCursor()
        {
            return ProtectedCursor;
        }

        public void SetCursor(InputCursor cursor)
        {
            ProtectedCursor = cursor;
        }
    }
}
