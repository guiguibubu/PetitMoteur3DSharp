using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace PetitMoteur3D
{
    internal abstract class BaseObjet3D : IObjet3D
    {
        /// <inheritdoc/>
        public Vector3D<float> Position { get { return _position; } }
        /// <inheritdoc/>
        public Vector3D<float> Rotation { get { return _rotation; } }

        private ComPtr<ID3D11Buffer> _vertexBuffer = default;
        private ComPtr<ID3D11Buffer> _indexBuffer = default;
        private ComPtr<ID3D11Buffer> _constantBuffer = default;

        private ComPtr<ID3D11VertexShader> _vertexShader = default;
        private ComPtr<ID3D11InputLayout> _vertexLayout = default;
        private ComPtr<ID3D11PixelShader> _pixelShader = default;

        private ComPtr<ID3D11ShaderResourceView> _textureD3D;
        private ComPtr<ID3D11SamplerState> _sampleState;

        private Matrix4X4<float> _matWorld = default;

        private Vector3D<float> _position;
        private Vector3D<float> _rotation;

        private Sommet[] _sommets;
        private ushort[] _indices;

        private readonly DeviceD3D11 _renderDevice;
        private readonly ShaderManager _shaderManager;

        protected unsafe BaseObjet3D(DeviceD3D11 renderDevice, ShaderManager shaderManager)
        {
            _position = Vector3D<float>.Zero;
            _rotation = Vector3D<float>.Zero;
            _matWorld = Matrix4X4<float>.Identity;

            _sommets = Array.Empty<Sommet>();
            _indices = Array.Empty<ushort>();

            _renderDevice = renderDevice;
            _shaderManager = shaderManager;
        }

        ~BaseObjet3D()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _constantBuffer.Dispose();

            _vertexShader.Dispose();
            _vertexLayout.Dispose();
            _pixelShader.Dispose();
        }

        /// <inheritdoc/>
        public virtual void Anime(float elapsedTime)
        {
            _rotation.Y += (float)((Math.PI * 2.0f) / 3.0f * elapsedTime / 1000f);
            // modifier la matrice de l’objet bloc
            _matWorld = Matrix4X4.CreateRotationY(Rotation.Y);
        }

        /// <inheritdoc/>
        public unsafe void Draw(ComPtr<ID3D11DeviceContext> deviceContext, Matrix4X4<float> matViewProj)
        {
            // Choisir la topologie des primitives
            deviceContext.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
            // Source des sommets
            uint vertexStride = (uint)sizeof(Sommet);
            uint vertextOffset = 0;
            deviceContext.IASetVertexBuffers(0, 1, ref _vertexBuffer, in vertexStride, in vertextOffset);
            // Source des index
            deviceContext.IASetIndexBuffer(_indexBuffer, Silk.NET.DXGI.Format.FormatR16Uint, 0);
            // input layout des sommets
            deviceContext.IASetInputLayout(_vertexLayout);
            foreach (SubObjet3D subObjet3D in GetSubObjets())
            {
                // Initialiser et sélectionner les « constantes » du VS
                ShadersParams shadersParams = new()
                {
                    matWorldViewProj = Matrix4X4.Transpose(subObjet3D.Transformation * _matWorld * matViewProj),
                    matWorld = Matrix4X4.Transpose(subObjet3D.Transformation * _matWorld),
                    lightPos = new Vector4D<float>(-10f, 10f, -10f, 1f),
                    cameraPos = new Vector4D<float>(0.0f, 0.0f, -10.0f, 1.0f),
                    ambiantLightValue = new Vector4D<float>(0.2f, 0.2f, 0.2f, 1.0f),
                    ambiantMaterialValue = subObjet3D.Material.Ambient,
                    diffuseLightValue = new Vector4D<float>(1.0f, 1.0f, 1.0f, 1.0f),
                    diffuseMaterialValue = subObjet3D.Material.Diffuse,
                };
                deviceContext.UpdateSubresource(_constantBuffer, 0, ref Unsafe.NullRef<Box>(), ref shadersParams, 0, 0);

                // Activer le VS
                deviceContext.VSSetShader(_vertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
                deviceContext.VSSetConstantBuffers(0, 1, ref _constantBuffer);
                // Activer le GS
                deviceContext.GSSetShader((ID3D11GeometryShader*)null, (ID3D11ClassInstance**)null, 0);
                // Activer le PS
                deviceContext.PSSetShader(_pixelShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
                deviceContext.PSSetConstantBuffers(0, 1, ref _constantBuffer);
                // Activation de la texture
                deviceContext.PSSetShaderResources(0, 1, ref _textureD3D);
                // Le sampler state
                deviceContext.PSSetSamplers(0, 1, ref _sampleState);
                // **** Rendu de l’objet
                deviceContext.DrawIndexed((uint)_indices.Length, 0, 0);
            }
        }

        public void SetTexture(Texture texture)
        {
            _textureD3D = texture.TextureView;
        }

        protected void Initialisation()
        {
            _sommets = InitVertex().ToArray();
            _indices = InitIndex().ToArray();

            InitBuffers(_renderDevice.Device, _sommets, _indices);
            InitShaders(_shaderManager);
            InitTexture(_renderDevice.Device);
        }

        /// <summary>
        /// Initialise les vertex
        /// </summary>
        /// <returns></returns>
        protected abstract IReadOnlyList<Sommet> InitVertex();

        /// <summary>
        /// Initialise l'index de rendu
        /// </summary>
        /// <returns></returns>
        protected abstract IReadOnlyList<ushort> InitIndex();

        /// <summary>
        /// Renvoie la liste des parties de l'objet pour le rendu
        /// </summary>
        /// <returns></returns>
        protected abstract IReadOnlyList<SubObjet3D> GetSubObjets();

        private unsafe void InitShaders(ShaderManager shaderManager)
        {
            InitVertexShader(shaderManager);
            InitPixelShader(shaderManager);
        }

        private unsafe void InitTexture(ComPtr<ID3D11Device> device)
        {
            // Initialisation des paramètres de sampling de la texture
            SamplerDesc samplerDesc = new()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MipLODBias = 0f,
                MaxAnisotropy = 1,
                ComparisonFunc = ComparisonFunc.Always,
                MinLOD = 0,
                MaxLOD = float.MaxValue,
            };
            samplerDesc.BorderColor[0] = 0f;
            samplerDesc.BorderColor[1] = 0f;
            samplerDesc.BorderColor[2] = 0f;
            samplerDesc.BorderColor[3] = 0f;

            // Création de l’état de sampling
            SilkMarshal.ThrowHResult
            (
                device.CreateSamplerState(ref samplerDesc, ref _sampleState)
            );
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
            CreateConstantBuffer<ShadersParams>(device, ref _constantBuffer);
        }

        /// <summary>
        /// Compilation et chargement du vertex shader
        /// </summary>
        /// <param name="device"></param>
        /// <param name="compiler"></param>
        private unsafe void InitVertexShader(ShaderManager shaderManager)
        {
            // Compilation et chargement du vertex shader
            string filePath = "shaders\\MiniPhong_VS.hlsl";
            string entryPoint = "MiniPhongVS";
            string target = "vs_5_0";
            // #define D3DCOMPILE_ENABLE_STRICTNESS                    (1 << 11)
            uint flagStrictness = ((uint)1 << 11);
            // #define D3DCOMPILE_DEBUG (1 << 0)
            // #define D3DCOMPILE_SKIP_OPTIMIZATION                    (1 << 2)
#if DEBUG
            uint flagDebug = ((uint)1 << 0);
            uint flagSkipOptimization = ((uint)(1 << 2));
#else
            uint flagDebug = 0;
            uint flagSkipOptimization = 0;
#endif
            uint compilationFlags = flagStrictness | flagDebug | flagSkipOptimization;
            ShaderManager.ShaderDesc shaderDesc = new()
            {
                FilePath = filePath,
                EntryPoint = entryPoint,
                Target = target,
                CompilationFlags = compilationFlags
            };
            shaderManager.GetOrLoadVertexShaderAndLayout(shaderDesc, Sommet.InputLayoutDesc, ref _vertexShader, ref _vertexLayout);
        }

        /// <summary>
        /// Compilation et chargement du pixel shader
        /// </summary>
        /// <param name="device"></param>
        /// <param name="compiler"></param>
        private unsafe void InitPixelShader(ShaderManager shaderManager)
        {
            string filePath = "shaders\\MiniPhong_PS.hlsl";
            string entryPoint = "MiniPhongPS";
            string target = "ps_5_0";
            // #define D3DCOMPILE_ENABLE_STRICTNESS                    (1 << 11)
            uint flagStrictness = ((uint)1 << 11);
            // #define D3DCOMPILE_DEBUG (1 << 0)
            // #define D3DCOMPILE_SKIP_OPTIMIZATION                    (1 << 2)
#if DEBUG
            uint flagDebug = ((uint)1 << 0);
            uint flagSkipOptimization = ((uint)(1 << 2));
#else
            uint flagDebug = 0;
            uint flagSkipOptimization = 0;
#endif
            uint compilationFlags = flagStrictness | flagDebug | flagSkipOptimization;
            ShaderManager.ShaderDesc shaderDesc = new()
            {
                FilePath = filePath,
                EntryPoint = entryPoint,
                Target = target,
                CompilationFlags = compilationFlags
            };
            _pixelShader = shaderManager.GetOrLoadPixelShader(shaderDesc);
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

        public struct SubObjet3D
        {
            public IReadOnlyList<ushort> Indices;
            public Matrix4X4<float> Transformation;
            public Material Material;
        }
    }
}
