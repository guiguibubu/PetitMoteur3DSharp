using PetitMoteur3D.Input;

namespace PetitMoteur3D;

internal interface IInputListener
{
    /// <summary>
    /// Initialize input manager for the object
    /// </summary>
    /// <param name="inputContext"></param>
    void InitInput(IInputContext? inputContext);
}
