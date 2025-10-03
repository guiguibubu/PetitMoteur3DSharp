using Silk.NET.Direct3D11;

namespace PetitMoteur3D
{
    public class DX11SamplerDescResetter : IIResetter<SamplerDesc>
    {
        public unsafe void Reset(ref SamplerDesc instance)
        {
            instance.Filter = 0;
            instance.AddressU = 0;
            instance.AddressV = 0;
            instance.AddressW = 0;
            instance.MipLODBias = 0;
            instance.MaxAnisotropy = 0;
            instance.ComparisonFunc = 0;
            for(int i = 0; i < 4; i++)
            {
                instance.BorderColor[i] = 0;
            }
            instance.MinLOD = 0;
            instance.MaxLOD = 0;
        }
    }
}
