using System;
using PetitMoteur3D.Window;
using PetitMoteur3D.Window.WinUI;

namespace PetitMoteur3D.Input.WinUI;

public class WinUiInputPlatform : IInputPlatform
{
    /// <inheritdoc/>
    public bool IsApplicable(IWindow window)
    {
        return window is WinUIWindow;
    }

    /// <inheritdoc/>
    public IInputContext CreateInput(IWindow window)
    {
        if (!IsApplicable(window))
        {
            throw new NotSupportedException($"Window must be a {nameof(WinUIWindow)}");
        }
        return new WinUiInputContext(((WinUIWindow)window));
    }
}
