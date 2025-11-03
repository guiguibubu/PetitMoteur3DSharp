using Silk.NET.Direct3D11;

namespace PetitMoteur3D;

public class ViewportResetter : IIResetter<Viewport>
{
    public unsafe void Reset(ref Viewport instance)
    {
        MemoryHelper.ResetMemory(instance);
    }
}
