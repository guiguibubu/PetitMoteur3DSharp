namespace PetitMoteur3D.Input
{
    /// <summary>
    /// Generic interface representing an input device.
    /// </summary>
    public interface IInputDevice
    {
        /// <summary>
        /// The name of this device, as reported by the hardware.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Whether or not this device is currently connected.
        /// </summary>
        bool IsConnected { get; }
    }
}
