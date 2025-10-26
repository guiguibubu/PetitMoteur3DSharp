namespace PetitMoteur3D.Input.WinUI.Extensions
{
    internal static class StandardCursorExtensions
    {
        public static StandardCursor FromWinUi(this Microsoft.UI.Input.InputSystemCursorShape buttonName)
        {
            switch (buttonName)
            {
                case Microsoft.UI.Input.InputSystemCursorShape.Arrow: return StandardCursor.Arrow;
                case Microsoft.UI.Input.InputSystemCursorShape.IBeam: return StandardCursor.IBeam;
                case Microsoft.UI.Input.InputSystemCursorShape.Cross: return StandardCursor.Crosshair;
                case Microsoft.UI.Input.InputSystemCursorShape.Hand: return StandardCursor.Hand;
                case Microsoft.UI.Input.InputSystemCursorShape.SizeWestEast: return StandardCursor.HResize;
                case Microsoft.UI.Input.InputSystemCursorShape.SizeNorthSouth: return StandardCursor.VResize;
                case Microsoft.UI.Input.InputSystemCursorShape.SizeNorthwestSoutheast: return StandardCursor.NwseResize;
                case Microsoft.UI.Input.InputSystemCursorShape.SizeNortheastSouthwest: return StandardCursor.NeswResize;
                case Microsoft.UI.Input.InputSystemCursorShape.SizeAll: return StandardCursor.ResizeAll;
                case Microsoft.UI.Input.InputSystemCursorShape.UniversalNo: return StandardCursor.NotAllowed;
                case Microsoft.UI.Input.InputSystemCursorShape.Wait: return StandardCursor.Wait;
                case Microsoft.UI.Input.InputSystemCursorShape.AppStarting: return StandardCursor.WaitArrow;
                default:
                    return StandardCursor.Default;
            }
        }

        public static Microsoft.UI.Input.InputSystemCursorShape ToWinUi(this StandardCursor buttonName)
        {
            switch (buttonName)
            {
                case StandardCursor.Default: return Microsoft.UI.Input.InputSystemCursorShape.Arrow;
                case StandardCursor.Arrow: return Microsoft.UI.Input.InputSystemCursorShape.Arrow;
                case StandardCursor.IBeam: return Microsoft.UI.Input.InputSystemCursorShape.IBeam;
                case StandardCursor.Crosshair: return Microsoft.UI.Input.InputSystemCursorShape.Cross;
                case StandardCursor.Hand: return Microsoft.UI.Input.InputSystemCursorShape.Hand;
                case StandardCursor.HResize: return Microsoft.UI.Input.InputSystemCursorShape.SizeWestEast;
                case StandardCursor.VResize: return Microsoft.UI.Input.InputSystemCursorShape.SizeNorthSouth;
                case StandardCursor.NwseResize: return Microsoft.UI.Input.InputSystemCursorShape.SizeNorthwestSoutheast;
                case StandardCursor.NeswResize: return Microsoft.UI.Input.InputSystemCursorShape.SizeNortheastSouthwest;
                case StandardCursor.ResizeAll: return Microsoft.UI.Input.InputSystemCursorShape.SizeAll;
                case StandardCursor.NotAllowed: return Microsoft.UI.Input.InputSystemCursorShape.UniversalNo;
                case StandardCursor.Wait: return Microsoft.UI.Input.InputSystemCursorShape.Wait;
                case StandardCursor.WaitArrow: return Microsoft.UI.Input.InputSystemCursorShape.AppStarting;
                default:
                    return Microsoft.UI.Input.InputSystemCursorShape.Arrow;
            }
        }
    }
}
