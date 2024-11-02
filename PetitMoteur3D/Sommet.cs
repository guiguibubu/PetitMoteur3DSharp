using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace PetitMoteur3D
{
    internal struct Sommet
    {
        public Vector3D<float> Position { get; private set; }
        public Vector3D<float> Normale { get; private set; }
        public Vector3D<float> Tangente { get; private set; }
        public Vector2D<float> CoordTex { get; private set; }
        
        /// <summary>
        /// Defini l’organisation de notre sommet
        /// </summary>
        public static InputElementDesc[] InputLayoutDesc => s_inputElements;

        private static readonly GlobalMemory s_semanticNamePosition;
        private static readonly GlobalMemory s_semanticNameNormal;
        private static readonly GlobalMemory s_semanticNameTexCoord;
        private static readonly GlobalMemory s_semanticNameTangent;
        private static readonly InputElementDesc[] s_inputElements;


        static unsafe Sommet()
        {
            s_semanticNamePosition = SilkMarshal.StringToMemory("POSITION", NativeStringEncoding.LPStr);
            s_semanticNameNormal = SilkMarshal.StringToMemory("NORMAL", NativeStringEncoding.LPStr);
            s_semanticNameTangent = SilkMarshal.StringToMemory("TANGENT", NativeStringEncoding.LPStr);
            s_semanticNameTexCoord = SilkMarshal.StringToMemory("TEXCOORD", NativeStringEncoding.LPStr);

            s_inputElements = new[]
            {
                new InputElementDesc(
                    s_semanticNamePosition.AsPtr<byte>(), 0, Silk.NET.DXGI.Format.FormatR32G32B32Float, 0, 0, InputClassification.PerVertexData, 0
                ),
                new InputElementDesc(
                    s_semanticNameNormal.AsPtr<byte>(), 0, Silk.NET.DXGI.Format.FormatR32G32B32Float, 0, (uint)sizeof(Vector3D<float>), InputClassification.PerVertexData, 0
                ),
                new InputElementDesc(
                    s_semanticNameTangent.AsPtr<byte>(), 0, Silk.NET.DXGI.Format.FormatR32G32Float, 0, 2 * (uint)sizeof(Vector3D<float>), InputClassification.PerVertexData, 0
                ),
                new InputElementDesc(
                    s_semanticNameTexCoord.AsPtr<byte>(), 0, Silk.NET.DXGI.Format.FormatR32G32Float, 0, 3 * (uint)sizeof(Vector3D<float>), InputClassification.PerVertexData, 0
                ),
            };
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normale"></param>
        /// <param name="coordTex"></param>
        public unsafe Sommet(Vector3D<float> position, Vector3D<float> normale, Vector3D<float> tangente, Vector2D<float> coordTex)
        {
            Position = position;
            Normale = normale;
            Tangente = tangente;
            CoordTex = coordTex;
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normale"></param>
        /// <param name="coordTex"></param>
        public unsafe Sommet(Vector3D<float> position, Vector3D<float> normale, Vector3D<float> tangente)
        : this(position, normale, tangente, Vector2D<float>.Zero)
        { }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normale"></param>
        /// <param name="coordTex"></param>
        public unsafe Sommet(Vector3D<float> position, Vector3D<float> normale)
        : this(position, normale, Vector3D<float>.Zero, Vector2D<float>.Zero)
        { }

        public Sommet Clone()
        {
            Sommet copy =  (Sommet)this.MemberwiseClone();
            copy.Position = new Vector3D<float>(Position.X, Position.Y, Position.Z);
            copy.Normale = new Vector3D<float>(Normale.X, Normale.Y, Normale.Z);
            copy.CoordTex = new Vector2D<float>(CoordTex.X, CoordTex.Y);
            return copy;
        }
    }
}
