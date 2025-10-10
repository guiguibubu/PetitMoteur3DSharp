using PetitMoteur3D.Window;

namespace PetitMoteur3D.Input
{
    /// <summary>
    /// An interface representing an input platform.
    /// </summary>
    public interface IInputPlatform
    {
        /// <summary>
        /// Get an input context for this window.
        /// </summary>
        /// <param name="view">The view to get a context for.</param>
        /// <returns>The context.</returns>
        IInputContext CreateInput(IWindow view);
    }
}
