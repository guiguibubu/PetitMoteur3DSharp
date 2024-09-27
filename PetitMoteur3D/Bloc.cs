using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace PetitMoteur3D
{
    internal class Bloc : IObjet3D
    {
        private ComPtr<ID3D11Buffer> _vertexBuffer = default;
        private ComPtr<ID3D11Buffer> _indexBuffer = default;
        private ComPtr<ID3D11Buffer> _constantBuffer = default;

        private ComPtr<ID3D11VertexShader> _vertexShader = default;
        private ComPtr<ID3D11InputLayout> _vertexLayout = default;
        private ComPtr<ID3D11PixelShader> _pixelShader = default;

        private Matrix4X4<float> _matWorld = default;
        private float _rotation = default;

        private Sommet[] _sommets;
        private ushort[] _indices;

        public unsafe Bloc(float dx, float dy, float dz, DeviceD3D11 renderDevice)
        {
            _matWorld = Matrix4X4<float>.Identity;
            _rotation = 0f;

            Vector3D<float>[] vertices = new[]
            {
                new Vector3D<float>(-dx / 2, dy / 2, -dz / 2),
                new Vector3D<float>(dx / 2, dy / 2, -dz / 2),
                new Vector3D<float>(dx / 2, -dy / 2, -dz / 2),
                new Vector3D<float>(-dx / 2, -dy / 2, -dz / 2),
                new Vector3D<float>(-dx / 2, dy / 2, dz / 2),
                new Vector3D<float>(-dx / 2, -dy / 2, dz / 2),
                new Vector3D<float>(dx / 2, -dy / 2, dz / 2),
                new Vector3D<float>(dx / 2, dy / 2, dz / 2)
            };

            Vector3D<float> n0 = new(0.0f, 0.0f, -1.0f); // devant
            Vector3D<float> n1 = new(0.0f, 0.0f, 1.0f); // arrière
            Vector3D<float> n2 = new(0.0f, -1.0f, 0.0f); // dessous
            Vector3D<float> n3 = new(0.0f, 1.0f, 0.0f); // dessus
            Vector3D<float> n4 = new(-1.0f, 0.0f, 0.0f); // face gauche
            Vector3D<float> n5 = new(1.0f, 0.0f, 0.0f); // face droite

            _sommets = new[]
            {
                new Sommet(vertices[0], n0),
                new Sommet(vertices[1], n0),
                new Sommet(vertices[2], n0),
                new Sommet(vertices[3], n0),
                new Sommet(vertices[4], n1),
                new Sommet(vertices[5], n1),
                new Sommet(vertices[6], n1),
                new Sommet(vertices[7], n1),
            };

            _indices = new ushort[]
            {
                0,1,2, // devant
                0,2,3, // devant
                5,6,7, // arrière
                5,7,4, // arrière
                2,6,5, // dessous
                2,5,3, // dessous
                0,7,1, // dessus
                0,4,7, // dessus
                0,3,5, // gauche
                0,5,4, // gauche
                1,6,2, // droite
                1,7,6 // droite
            };
            // WIP A conserver pour quand on utilisera les normales
            /*
            Sommet[] sommets = new[]
            {
                // Le devant du bloc
                new Sommet(vertices[0], n0),
                new Sommet(vertices[1], n0),
                new Sommet(vertices[2], n0),
                new Sommet(vertices[3], n0),
                // L’arrière du bloc
                new Sommet(vertices[4], n1),
                new Sommet(vertices[5], n1),
                new Sommet(vertices[6], n1),
                new Sommet(vertices[7], n1),
                // Le dessous du bloc
                new Sommet(vertices[3], n2),
                new Sommet(vertices[2], n2),
                new Sommet(vertices[6], n2),
                new Sommet(vertices[5], n2),
                // Le dessus du bloc
                new Sommet(vertices[0], n3),
                new Sommet(vertices[4], n3),
                new Sommet(vertices[7], n3),
                new Sommet(vertices[1], n3),
                // La face gauche
                new Sommet(vertices[0], n4),
                new Sommet(vertices[3], n4),
                new Sommet(vertices[5], n4),
                new Sommet(vertices[4], n4),
                // La face droite
                new Sommet(vertices[1], n5),
                new Sommet(vertices[7], n5),
                new Sommet(vertices[6], n5),
                new Sommet(vertices[2], n5)
            };
            

            _indices = new[]
            {
                0,1,2, // devant
                0,2,3, // devant
                5,6,7, // arrière
                5,7,4, // arrière
                8,9,10, // dessous
                8,10,11, // dessous
                13,14,15, // dessus
                13,15,12, // dessus
                19,16,17, // gauche
                19,17,18, // gauche
                20,21,22, // droite
                20,22,23 // droite
            };
            */

            InitBuffers(renderDevice.Device, _sommets, _indices);
            InitShaders(renderDevice.Device, renderDevice.ShaderCompiler);
        }

        ~Bloc()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _constantBuffer.Dispose();

            _vertexShader.Dispose();
            _vertexLayout.Dispose();
            _pixelShader.Dispose();
        }

        public void Anime(float elapsedTime)
        {
            _rotation += (float)((Math.PI * 2.0f) / 3.0f * elapsedTime / 1000f);
            // modifier la matrice de l’objet bloc
            _matWorld = Matrix4X4.CreateRotationY(_rotation);
        }

        public unsafe void Draw(ComPtr<ID3D11DeviceContext> _deviceContext, Matrix4X4<float> matViewProj)
        {
            // Choisir la topologie des primitives
            _deviceContext.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
            // Source des sommets
            uint vertexStride = (uint)sizeof(Sommet);
            uint vertextOffset = 0;
            _deviceContext.IASetVertexBuffers(0, 1, ref _vertexBuffer, in vertexStride, in vertextOffset);
            // Source des index
            _deviceContext.IASetIndexBuffer(_indexBuffer, Silk.NET.DXGI.Format.FormatR16Uint, 0);
            // input layout des sommets
            _deviceContext.IASetInputLayout(_vertexLayout);
            // Activer le VS
            _deviceContext.VSSetShader(_vertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            // Initialiser et sélectionner les « constantes » du VS
            Matrix4X4<float> matWorldViewProj = Matrix4X4.Transpose(_matWorld * matViewProj);
            _deviceContext.UpdateSubresource(_constantBuffer, 0, ref Unsafe.NullRef<Box>(), ref matWorldViewProj, 0, 0);
            _deviceContext.VSSetConstantBuffers(0, 1, ref _constantBuffer);
            // Activer le PS
            _deviceContext.PSSetShader(_pixelShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            // **** Rendu de l’objet
            _deviceContext.DrawIndexed((uint)_indices.Length, 0, 0);
        }

        private unsafe void InitShaders(ComPtr<ID3D11Device> device, D3DCompiler compiler)
        {
            InitVertexShader(device, compiler);
            InitPixelShader(device, compiler);
        }

        private unsafe void InitBuffers<TVertex, TIndice>(ComPtr<ID3D11Device> device, TVertex[] sommets, TIndice[] indices)
            where TVertex : unmanaged
            where TIndice : unmanaged
        {
            // Create our vertex buffer.
            CreateVertexBuffer(device, sommets, ref _vertexBuffer);

            // Create our index buffer.
            CreateIndexBuffer(device, indices, ref _indexBuffer);

            // Create our constant buffer.
            CreateConstantBuffer<Matrix4X4<float>>(device, ref _constantBuffer);
        }

        /// <summary>
        /// Compilation et chargement du vertex shader
        /// </summary>
        /// <param name="device"></param>
        /// <param name="compiler"></param>
        private unsafe void InitVertexShader(ComPtr<ID3D11Device> device, D3DCompiler compiler)
        {
            // Compilation et chargement du vertex shader
            ComPtr<ID3D10Blob> compilationBlob = default;
            ComPtr<ID3D10Blob> compilationErrors = default;
            string filePath = "shaders\\VS1.hlsl";
            byte[] shaderCode = File.ReadAllBytes(filePath);
            string entryPoint = "VS1";
            string target = "vs_5_0";
            // #define D3DCOMPILE_ENABLE_STRICTNESS                    (1 << 11)
            uint flagStrictness = ((uint)0 << 11);
            // #define D3DCOMPILE_DEBUG (1 << 0)
            // #define D3DCOMPILE_SKIP_OPTIMIZATION                    (1 << 2)
#if DEBUG
            uint flagDebug = ((uint)1 << 0);
            uint flagOptimization = ((uint)(1 << 2));
#else
            uint flagDebug = 0;
            uint flagOptimization = 0;
#endif
            HResult hr = compiler.Compile
            (
                in shaderCode[0],
                (nuint)shaderCode.Length,
                filePath,
                ref Unsafe.NullRef<D3DShaderMacro>(),
                ref Unsafe.NullRef<ID3DInclude>(),
                entryPoint,
                target,
                flagStrictness | flagDebug | flagOptimization,
                0,
                ref compilationBlob,
                ref compilationErrors
            );

            // Check for compilation errors.
            if (hr.IsFailure)
            {
                if (compilationErrors.Handle is not null)
                {
                    Console.WriteLine(SilkMarshal.PtrToString((nint)compilationErrors.GetBufferPointer()));
                }

                hr.Throw();
            }

            // Create vertex shader.
            SilkMarshal.ThrowHResult
            (
                device.CreateVertexShader
                (
                    compilationBlob.GetBufferPointer(),
                    compilationBlob.GetBufferSize(),
                    ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                    ref _vertexShader
                )
            );

            // Créer l’organisation des sommets
            Sommet.CreateInputLayout(device, compilationBlob, ref _vertexLayout);

            compilationBlob.Dispose();
            compilationErrors.Dispose();
        }

        /// <summary>
        /// Compilation et chargement du pixel shader
        /// </summary>
        /// <param name="device"></param>
        /// <param name="compiler"></param>
        private unsafe void InitPixelShader(ComPtr<ID3D11Device> device, D3DCompiler compiler)
        {
            ComPtr<ID3D10Blob> compilationBlob = default;
            ComPtr<ID3D10Blob> compilationErrors = default;
            string filePath = "shaders\\PS1.hlsl";
            byte[] shaderCode = File.ReadAllBytes(filePath);
            string entryPoint = "PS1";
            string target = "ps_5_0";
            // #define D3DCOMPILE_ENABLE_STRICTNESS                    (1 << 11)
            uint flagStrictness = ((uint)0 << 11);
            // #define D3DCOMPILE_DEBUG (1 << 0)
            // #define D3DCOMPILE_SKIP_OPTIMIZATION                    (1 << 2)
#if DEBUG
            uint flagDebug = ((uint)1 << 0);
            uint flagOptimization = ((uint)(1 << 2));
#else
            uint flagDebug = 0;
            uint flagOptimization = 0;
#endif
            HResult hr = compiler.Compile
            (
                in shaderCode[0],
                (nuint)shaderCode.Length,
                filePath,
                ref Unsafe.NullRef<D3DShaderMacro>(),
                ref Unsafe.NullRef<ID3DInclude>(),
                entryPoint,
                target,
                flagStrictness | flagDebug | flagOptimization,
                0,
                ref compilationBlob,
                ref compilationErrors
            );

            // Check for compilation errors.
            if (hr.IsFailure)
            {
                if (compilationErrors.Handle is not null)
                {
                    Console.WriteLine(SilkMarshal.PtrToString((nint)compilationErrors.GetBufferPointer()));
                }

                hr.Throw();
            }

            // Create pixel shader.
            SilkMarshal.ThrowHResult
            (
                device.CreatePixelShader
                (
                    compilationBlob.GetBufferPointer(),
                    compilationBlob.GetBufferSize(),
                    ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                    ref _pixelShader
                )
            );

            compilationBlob.Dispose();
            compilationErrors.Dispose();

        }

        private static unsafe void CreateVertexBuffer<T>(ComPtr<ID3D11Device> device, T[] sommets, ref ComPtr<ID3D11Buffer> buffer) where T : unmanaged
        {
            BufferDesc bufferDesc = new()
            {
                ByteWidth = (uint)(sommets.Length * sizeof(T)),
                Usage = Usage.Immutable,
                BindFlags = (uint)BindFlag.VertexBuffer,
                CPUAccessFlags = 0
            };

            fixed (T* vertexData = sommets)
            {
                SubresourceData subresourceData = new()
                {
                    PSysMem = vertexData
                };

                SilkMarshal.ThrowHResult(device.CreateBuffer(in bufferDesc, in subresourceData, ref buffer));
            }
        }

        private static unsafe void CreateIndexBuffer<T>(ComPtr<ID3D11Device> device, T[] indices, ref ComPtr<ID3D11Buffer> buffer) where T : unmanaged
        {
            BufferDesc bufferDesc = new()
            {
                ByteWidth = (uint)(indices.Length * sizeof(T)),
                Usage = Usage.Immutable,
                BindFlags = (uint)BindFlag.IndexBuffer,
                CPUAccessFlags = 0,
                StructureByteStride = (uint)sizeof(T)
            };

            fixed (T* indexData = indices)
            {
                SubresourceData subresourceData = new()
                {
                    PSysMem = indexData
                };

                SilkMarshal.ThrowHResult(device.CreateBuffer(in bufferDesc, in subresourceData, ref buffer));
            }
        }

        private static unsafe void CreateConstantBuffer<T>(ComPtr<ID3D11Device> device, ref ComPtr<ID3D11Buffer> buffer) where T : unmanaged
        {
            BufferDesc bufferDesc = new()
            {
                ByteWidth = (uint)(sizeof(T)),
                Usage = Usage.Default,
                BindFlags = (uint)BindFlag.ConstantBuffer,
                CPUAccessFlags = 0
            };

            SilkMarshal.ThrowHResult(device.CreateBuffer(in bufferDesc, in Unsafe.NullRef<SubresourceData>(), ref buffer));
        }
    }
}
