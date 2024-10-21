using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace PetitMoteur3D
{
    internal struct Sommet
    {
        public Vector3D<float> Position { get; private set; }
        public Vector3D<float> Normale { get; private set; }
        public Vector2D<float> CoordTex { get; private set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normale"></param>
        /// <param name="coordTex"></param>
        public unsafe Sommet(Vector3D<float> position, Vector3D<float> normale, Vector2D<float> coordTex)
        {
            Position = position;
            Normale = normale;
            CoordTex = coordTex;
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normale"></param>
        /// <param name="coordTex"></param>
        public unsafe Sommet(Vector3D<float> position, Vector3D<float> normale)
        : this(position, normale, Vector2D<float>.Zero)
        { }

        /// <summary>
        /// Defini l’organisation de notre sommet
        /// </summary>
        public static unsafe void CreateInputLayout(ComPtr<ID3D11Device> device, ComPtr<ID3D10Blob> vertexCode, ref ComPtr<ID3D11InputLayout> inputLayout)
        {
            // Describe the layout of the input data for the shader.
            fixed (byte* semanticNamePosition = SilkMarshal.StringToMemory("POSITION"))
            fixed (byte* semanticNameNormal = SilkMarshal.StringToMemory("NORMAL"))
            fixed (byte* semanticNameTexCoord = SilkMarshal.StringToMemory("TEXCOORD"))
            {
                InputElementDesc[] inputElements = new[]
                {
                    new InputElementDesc(
                        semanticNamePosition, 0, Silk.NET.DXGI.Format.FormatR32G32B32Float, 0, 0, InputClassification.PerVertexData, 0
                    ),
                    new InputElementDesc(
                        semanticNameNormal, 0, Silk.NET.DXGI.Format.FormatR32G32B32Float, 0, (uint)sizeof(Vector3D<float>), InputClassification.PerVertexData, 0
                    ),
                    new InputElementDesc(
                        semanticNameTexCoord, 0, Silk.NET.DXGI.Format.FormatR32G32Float, 0, 2 * (uint)sizeof(Vector3D<float>), InputClassification.PerVertexData, 0
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
