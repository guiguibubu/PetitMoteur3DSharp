using PetitMoteur3D.Input.WinUI.Extensions;

namespace PetitMoteur3D.Input.WinUI;

internal class WinUiCursor : ICursor
{
    private Microsoft.UI.Input.InputCursor _winUiCursor;

    public WinUiCursor(Microsoft.UI.Input.InputCursor winUiCursor)
    {
        _winUiCursor = winUiCursor;
        _cursorType = (winUiCursor is Microsoft.UI.Input.InputSystemCursor) ? CursorType.Standard : CursorType.Custom;
    }

    private CursorType _cursorType;
    public CursorType Type { get => _cursorType; set => _cursorType = value; }

    public StandardCursor StandardCursor { get => GetStandardCursor(); set => SetStandardCursor(value); }
    public CursorMode CursorMode { get => CursorMode.Normal; set { } }
    public bool IsConfined { get => false; set { } }
    public int HotspotX { get => 0; set { } }
    public int HotspotY { get => 0; set { } }
    public RawImage Image { get => new(); set { } }

    public bool IsSupported(CursorMode mode)
    {
        return mode == CursorMode.Normal;
    }

    public bool IsSupported(StandardCursor standardCursor)
    {
        return _winUiCursor is Microsoft.UI.Input.InputSystemCursor;
    }

    private StandardCursor GetStandardCursor()
    {
        if (_winUiCursor is Microsoft.UI.Input.InputSystemCursor inputSystemCursor)
        {
            return inputSystemCursor.CursorShape.FromWinUi();
        }
        return StandardCursor.Default;
    }

    private void SetStandardCursor(StandardCursor standardCursor)
    {
        if (_winUiCursor is Microsoft.UI.Input.InputSystemCursor inputSystemCursor)
        {
            _winUiCursor = Microsoft.UI.Input.InputSystemCursor.Create(standardCursor.ToWinUi());
        }
    }
}
