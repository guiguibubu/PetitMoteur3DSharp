namespace PetitMoteur3D.Core.Memory;

public class DefaultStructResetter<TStruct> : IResetter<TStruct> where TStruct : struct
{
    public unsafe void Reset(ref TStruct instance)
    {
        MemoryHelper.ResetMemory(instance);
    }
}
