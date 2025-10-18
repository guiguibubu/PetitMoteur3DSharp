namespace PetitMoteur3D.Input.SilkNet.Extensions
{
    internal static class StandardCursorExtensions
    {
        public static StandardCursor FromSilk(this Silk.NET.Input.StandardCursor buttonName)
        {
            switch (buttonName)
            {
                case Silk.NET.Input.StandardCursor.Default: return StandardCursor.Default;
                case Silk.NET.Input.StandardCursor.Arrow: return StandardCursor.Arrow;
                case Silk.NET.Input.StandardCursor.IBeam: return StandardCursor.IBeam;
                case Silk.NET.Input.StandardCursor.Crosshair: return StandardCursor.Crosshair;
                case Silk.NET.Input.StandardCursor.Hand: return StandardCursor.Hand;
                case Silk.NET.Input.StandardCursor.HResize: return StandardCursor.HResize;
                case Silk.NET.Input.StandardCursor.VResize: return StandardCursor.VResize;
                case Silk.NET.Input.StandardCursor.NwseResize: return StandardCursor.NwseResize;
                case Silk.NET.Input.StandardCursor.NeswResize: return StandardCursor.NeswResize;
                case Silk.NET.Input.StandardCursor.ResizeAll: return StandardCursor.ResizeAll;
                case Silk.NET.Input.StandardCursor.NotAllowed: return StandardCursor.NotAllowed;
                case Silk.NET.Input.StandardCursor.Wait: return StandardCursor.Wait;
                case Silk.NET.Input.StandardCursor.WaitArrow: return StandardCursor.WaitArrow;
                default:
                    return StandardCursor.Default;
            }
        }

        public static Silk.NET.Input.StandardCursor ToSilk(this StandardCursor buttonName)
        {
            switch (buttonName)
            {
                case StandardCursor.Default: return Silk.NET.Input.StandardCursor.Default;
                case StandardCursor.Arrow: return Silk.NET.Input.StandardCursor.Arrow;
                case StandardCursor.IBeam: return Silk.NET.Input.StandardCursor.IBeam;
                case StandardCursor.Crosshair: return Silk.NET.Input.StandardCursor.Crosshair;
                case StandardCursor.Hand: return Silk.NET.Input.StandardCursor.Hand;
                case StandardCursor.HResize: return Silk.NET.Input.StandardCursor.HResize;
                case StandardCursor.VResize: return Silk.NET.Input.StandardCursor.VResize;
                case StandardCursor.NwseResize: return Silk.NET.Input.StandardCursor.NwseResize;
                case StandardCursor.NeswResize: return Silk.NET.Input.StandardCursor.NeswResize;
                case StandardCursor.ResizeAll: return Silk.NET.Input.StandardCursor.ResizeAll;
                case StandardCursor.NotAllowed: return Silk.NET.Input.StandardCursor.NotAllowed;
                case StandardCursor.Wait: return Silk.NET.Input.StandardCursor.Wait;
                case StandardCursor.WaitArrow: return Silk.NET.Input.StandardCursor.WaitArrow;
                default:
                    return Silk.NET.Input.StandardCursor.Default;
            }
        }
    }
}
