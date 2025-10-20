namespace PetitMoteur3D.Input.SilkNet.Extensions
{
    internal static class TriggerExtensions
    {
        public static Trigger FromSilk(this Silk.NET.Input.Trigger trigger)
        {
            return new Trigger(trigger.Index, trigger.Position);
        }

        public static Silk.NET.Input.Trigger ToSilk(this Trigger trigger)
        {
            return new Silk.NET.Input.Trigger(trigger.Index, trigger.Position);
        }
    }
}
