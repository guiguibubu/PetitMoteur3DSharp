using System;
using PetitMoteur3D.Input;
using PetitMoteur3D.Window;

namespace PetitMoteur3D;

public readonly ref struct EngineConfiguration
{
    public readonly IWindow Window { get; init; }
    public readonly IInputContext? InputContext { get; init; }

    /// <summary>
    /// Constructeur par défaut
    /// </summary>
    /// <param name="window"></param>
    public EngineConfiguration(ref readonly IWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        Window = window;
    }
}
