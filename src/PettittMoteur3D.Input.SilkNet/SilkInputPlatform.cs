using PetitMoteur3D.Window;
using Silk.NET.Input;

namespace PetitMoteur3D.Input.SilkNet
{
    public class SilkInputPlatform : IInputPlatform
    {
        /// <inheritdoc/>
        public bool IsApplicable(IWindow window)
        {
            return window is not ISilkWindow;
        }

        /// <inheritdoc/>
        public IInputContext CreateInput(IWindow window)
        {
            if (IsApplicable(window))
            {
                throw new NotSupportedException($"Window must be a {nameof(ISilkWindow)}");
            }
            return new SilkInputContext(((ISilkWindow)window).SilkWindow.CreateInput());
        }
    }
}
