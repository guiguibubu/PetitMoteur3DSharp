namespace PetitMoteur3D.Input.SilkNet.Extensions;

internal static class CursorModeExtensions
{
    public static CursorMode FromSilk(this Silk.NET.Input.CursorMode mode)
    {
        switch (mode)
        {
            case Silk.NET.Input.CursorMode.Normal: return CursorMode.Normal;
            case Silk.NET.Input.CursorMode.Hidden: return CursorMode.Hidden;
            case Silk.NET.Input.CursorMode.Disabled: return CursorMode.Disabled;
            case Silk.NET.Input.CursorMode.Raw: return CursorMode.Raw;
            default:
                return CursorMode.Normal;
        }
    }

    public static Silk.NET.Input.CursorMode ToSilk(this CursorMode mode)
    {
        switch (mode)
        {
            case CursorMode.Normal: return Silk.NET.Input.CursorMode.Normal;
            case CursorMode.Hidden: return Silk.NET.Input.CursorMode.Hidden;
            case CursorMode.Disabled: return Silk.NET.Input.CursorMode.Disabled;
            case CursorMode.Raw: return Silk.NET.Input.CursorMode.Raw;
            default:
                return Silk.NET.Input.CursorMode.Normal;
        }
    }
}
