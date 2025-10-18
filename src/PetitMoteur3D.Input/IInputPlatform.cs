using PetitMoteur3D.Window;

namespace PetitMoteur3D.Input
{
    /// <summary>
    /// An interface representing an input platform.
    /// </summary>
    public interface IInputPlatform
    {
        /// <summary>
        /// If this platform is applicable to this window.
        /// </summary>
        /// <param name="window">The window to check.</param>
        /// <returns>Whether or not this platform is applicable.</returns>
        /// <remarks>
        /// Generally, each Input package will also have a matching Windowing package,
        /// and the Input package will reference the Windowing package. IsApplicable works
        /// by checking that the given window is an instance created by the Windowing
        /// package the Input package references. For example, GlfwInputPlatform will only
        /// be applicable for a GlfwWindow.
        /// </remarks>
        bool IsApplicable(IWindow window);

        /// <summary>
        /// Get an input context for this window.
        /// </summary>
        /// <param name="window">The view to get a context for.</param>
        /// <returns>The context.</returns>
        IInputContext CreateInput(IWindow window);
    }
}
