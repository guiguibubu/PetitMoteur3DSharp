using System;
using System.Collections.Generic;

namespace PetitMoteur3D.Input
{
    /// <summary>
    /// An interface representing the input context.
    /// </summary>
    public interface IInputContext : IDisposable
    {
        /// <summary>
        /// A handle to the underlying input context.
        /// </summary>
        nint Handle { get; }

        /// <summary>
        /// A list of all available gamepads.
        /// </summary>
        IReadOnlyList<IGamepad> Gamepads { get; }

        /// <summary>
        /// A list of all available joysticks.
        /// </summary>
        IReadOnlyList<IJoystick> Joysticks { get; }

        /// <summary>
        /// A list of all available keyboards.
        /// <remarks>
        /// On some backends, this list may only contain 1 item. This is most likely because the underlying API doesn't
        /// support multiple keyboards.
        /// </remarks>
        /// </summary>
        IReadOnlyList<IKeyboard> Keyboards { get; }

        /// <summary>
        /// A list of all available mice.
        /// </summary>
        /// <remarks>
        /// On some backends, this list may only contain 1 item. This is most likely because the underlying API doesn't
        /// support multiple mice.
        /// </remarks>
        IReadOnlyList<IMouse> Mice { get; }

        /// <summary>
        /// A list of all other available input devices.
        /// </summary>
        /// <remarks>
        /// On some backends, this list might be empty. This is most likely because the underlying API doesn't
        /// support other devices.
        /// </remarks>
        IReadOnlyList<IInputDevice> OtherDevices { get; }

        /// <summary>
        /// Called when the connection status of a device changes.
        /// </summary>
        event Action<IInputDevice, bool>? ConnexionChanged;
    }
}
