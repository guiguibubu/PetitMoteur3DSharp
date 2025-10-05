namespace PetitMoteur3D
{
    public class Vector2Resetter : IIResetter<System.Numerics.Vector2>
    {
        public unsafe void Reset(ref System.Numerics.Vector2 instance)
        {
            MemoryHelper.ResetMemory(instance);
        }
    }
}
