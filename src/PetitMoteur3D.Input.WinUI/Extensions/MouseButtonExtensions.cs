namespace PetitMoteur3D.Input.WinUI.Extensions;

internal static class MouseButtonExtensions
{
    public static MouseButton FromWinUi(this Microsoft.UI.Input.PointerUpdateKind buttonName)
    {
        switch (buttonName)
        {
            case Microsoft.UI.Input.PointerUpdateKind.Other: return MouseButton.Unknown;
            case Microsoft.UI.Input.PointerUpdateKind.RightButtonPressed: return MouseButton.Right;
            case Microsoft.UI.Input.PointerUpdateKind.RightButtonReleased: return MouseButton.Right;
            case Microsoft.UI.Input.PointerUpdateKind.LeftButtonPressed: return MouseButton.Left;
            case Microsoft.UI.Input.PointerUpdateKind.LeftButtonReleased: return MouseButton.Left;
            case Microsoft.UI.Input.PointerUpdateKind.MiddleButtonPressed: return MouseButton.Middle;
            case Microsoft.UI.Input.PointerUpdateKind.MiddleButtonReleased: return MouseButton.Middle;
            case Microsoft.UI.Input.PointerUpdateKind.XButton1Pressed: return MouseButton.Button4;
            case Microsoft.UI.Input.PointerUpdateKind.XButton1Released: return MouseButton.Button4;
            case Microsoft.UI.Input.PointerUpdateKind.XButton2Pressed: return MouseButton.Button5;
            case Microsoft.UI.Input.PointerUpdateKind.XButton2Released: return MouseButton.Button5;
            default:
                return MouseButton.Unknown;
        }
    }
}
