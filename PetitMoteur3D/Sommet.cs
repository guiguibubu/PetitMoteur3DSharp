using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using System.Collections.Generic;

namespace PetitMoteur3D
{
    internal struct Sommet
    {
        public Vector3D<float> Position { get; private set; }
        public Vector3D<float> Normale { get; private set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normale"></param>
        public unsafe Sommet(Vector3D<float> position, Vector3D<float> normale)
        {
            Position = position;
            Normale = normale;
        }

        /// <summary>
        /// Defini l’organisation de notre sommet
        /// </summary>
        public static unsafe void CreateInputLayout(ComPtr<ID3D11Device> device, ComPtr<ID3D10Blob> vertexCode, ref ComPtr<ID3D11InputLayout> inputLayout)
        {
            // Describe the layout of the input data for the shader.
            fixed (byte* semanticNamePosition = SilkMarshal.StringToMemory("POSITION"))
            fixed (byte* semanticNameNormal = SilkMarshal.StringToMemory("NORMAL"))
            {
                InputElementDesc[] inputElements = new[]
                {
                    new InputElementDesc(
                        semanticNamePosition, 0, Silk.NET.DXGI.Format.FormatR32G32B32Float, 0, 0, InputClassification.PerVertexData, 0
                    ),
                    new InputElementDesc(
                        semanticNameNormal, 0, Silk.NET.DXGI.Format.FormatR32G32B32Float, 0, 12, InputClassification.PerVertexData, 0
                    ),
                };

                SilkMarshal.ThrowHResult
                (
                    device.CreateInputLayout
                    (
                        in inputElements[0],
                        (uint)inputElements.Length,
                        vertexCode.GetBufferPointer(),
                        vertexCode.GetBufferSize(),
                        ref inputLayout
                    )
                );
            }
        }
    }
}
