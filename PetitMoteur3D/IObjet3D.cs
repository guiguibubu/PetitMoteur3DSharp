using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace PetitMoteur3D
{
    internal interface IObjet3D
    {
        void Anime(float elapsedTime);
        void Draw(ComPtr<ID3D11DeviceContext> deviceContext, Matrix4X4<float> matViewProj);
    }
}
