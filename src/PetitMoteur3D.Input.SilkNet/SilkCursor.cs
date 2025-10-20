using PetitMoteur3D.Input.SilkNet.Extensions;

namespace PetitMoteur3D.Input.SilkNet;

internal class SilkCursor : ICursor
{
    private Silk.NET.Input.ICursor _silkCursor;

    public SilkCursor(Silk.NET.Input.ICursor silkInputDevice)
    {
        _silkCursor = silkInputDevice;
    }

    public CursorType Type { get => _silkCursor.Type.FromSilk(); set => _silkCursor.Type = value.ToSilk(); }
    public StandardCursor StandardCursor { get => _silkCursor.StandardCursor.FromSilk(); set => _silkCursor.StandardCursor = value.ToSilk(); }
    public CursorMode CursorMode { get => _silkCursor.CursorMode.FromSilk(); set => _silkCursor.CursorMode = value.ToSilk(); }
    public bool IsConfined { get => _silkCursor.IsConfined; set => _silkCursor.IsConfined = value; }
    public int HotspotX { get => _silkCursor.HotspotX; set => _silkCursor.HotspotX = value; }
    public int HotspotY { get => _silkCursor.HotspotY; set => _silkCursor.HotspotY = value; }
    public RawImage Image { get => _silkCursor.Image.FromSilk(); set => _silkCursor.Image = value.ToSilk(); }

    public bool IsSupported(CursorMode mode)
    {
        return _silkCursor.IsSupported(mode.ToSilk());
    }

    public bool IsSupported(StandardCursor standardCursor)
    {
        return _silkCursor.IsSupported(standardCursor.ToSilk());
    }
}
