using Silk.NET.Direct3D11;

namespace PetitMoteur3D
{
    public class DX11SamplerDescResetter : IIResetter<SamplerDesc>
    {
        public unsafe void Reset(ref SamplerDesc instance)
        {
            MemoryHelper.ResetMemory(instance);
        }
    }
}
