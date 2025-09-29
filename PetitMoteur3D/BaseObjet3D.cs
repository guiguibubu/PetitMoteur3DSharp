using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PetitMoteur3D
{
    internal abstract class BaseObjet3D : IObjet3D
    {
        /// <inheritdoc/>
        public Vector3D<float> Position { get { return _position; } }
        /// <summary>
        /// Rotation de l'objet
        /// </summary>
        public Vector3D<float> Rotation { get { return _rotation; } }

        private ComPtr<ID3D11Buffer> _vertexBuffer = default;
        private ComPtr<ID3D11Buffer> _indexBuffer = default;
        private ComPtr<ID3D11Buffer> _constantBuffer = default;

        private ComPtr<ID3D11VertexShader> _vertexShader = default;
        private ComPtr<ID3D11InputLayout> _vertexLayout = default;
        private ComPtr<ID3D11PixelShader> _pixelShader = default;

        private ComPtr<ID3D11ShaderResourceView> _textureD3D;
        private ComPtr<ID3D11ShaderResourceView> _normalMap;
        private ComPtr<ID3D11SamplerState> _sampleState;

        private Matrix4X4<float> _matWorld = default;

        private Vector3D<float> _position;
        private Vector3D<float> _rotation;

        private Sommet[] _sommets;
        private ushort[] _indices;

        private readonly DeviceD3D11 _renderDevice;
        private readonly GraphicBufferFactory _bufferFactory;
        private readonly ShaderManager _shaderManager;
        private readonly TextureManager _textureManager;

        protected unsafe BaseObjet3D(DeviceD3D11 renderDevice, GraphicBufferFactory bufferFactory, ShaderManager shaderManager, TextureManager textureManager)
        {
            _position = Vector3D<float>.Zero;
            _rotation = Vector3D<float>.Zero;
            _matWorld = Matrix4X4<float>.Identity;

            _sommets = Array.Empty<Sommet>();
            _indices = Array.Empty<ushort>();

            _renderDevice = renderDevice;
            _bufferFactory = bufferFactory;
            _shaderManager = shaderManager;
            _textureManager = textureManager;
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
        public Vector3D<float> Move(Vector3D<float> move)
        {
            _position += move;
            return _position;
        }

        /// <inheritdoc/>
        public Vector3D<float> Rotate(Vector3D<float> rotation)
        {
            _rotation += rotation;
            return rotation;
        }

        /// <inheritdoc/>
        public virtual void Anime(float elapsedTime)
        {
            _rotation.Y += (float)((Math.PI * 2.0f) / 24.0f * elapsedTime / 1000f);
            // modifier la matrice de l’objet bloc
            _matWorld = Matrix4X4.CreateRotationY(_rotation.Y) * Matrix4X4.CreateTranslation(_position);
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
                // Initialiser et sélectionner les « constantes » des shaders
                ObjectShadersParams shadersParams = new()
                {
                    matWorldViewProj = Matrix4X4.Transpose(subObjet3D.Transformation * _matWorld * matViewProj),
                    matWorld = Matrix4X4.Transpose(subObjet3D.Transformation * _matWorld),
                    ambiantMaterialValue = subObjet3D.Material.Ambient,
                    diffuseMaterialValue = subObjet3D.Material.Diffuse,
                    hasTexture = Convert.ToInt32(_textureD3D.Handle is not null),
                    hasNormalMap = Convert.ToInt32(_normalMap.Handle is not null),
                };

                deviceContext.UpdateSubresource(_constantBuffer, 0, ref Unsafe.NullRef<Box>(), ref shadersParams, 0, 0);

                // Activer le VS
                deviceContext.VSSetShader(_vertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
                deviceContext.VSSetConstantBuffers(1, 1, ref _constantBuffer);
                // Activer le GS
                deviceContext.GSSetShader((ID3D11GeometryShader*)null, (ID3D11ClassInstance**)null, 0);
                // Activer le PS
                deviceContext.PSSetShader(_pixelShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
                deviceContext.PSSetConstantBuffers(1, 1, ref _constantBuffer);
                // Activation de la texture
                if (_textureD3D.Handle is not null)
                {
                    deviceContext.PSSetShaderResources(0, 1, ref _textureD3D);
                }
                if (_normalMap.Handle is not null)
                {
                    deviceContext.PSSetShaderResources(1, 1, ref _normalMap);
                }
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

        public void SetNormalMapTexture(Texture texture)
        {
            _normalMap = texture.TextureView;
        }

        protected void Initialisation()
        {
            _sommets = InitVertex().ToArray();
            _indices = InitIndex().ToArray();

            InitBuffers(_bufferFactory, _sommets, _indices);
            InitShaders(_shaderManager);
            InitTexture(_textureManager);
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

        private unsafe void InitTexture(TextureManager textureManager)
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
            _sampleState = textureManager.Factory.CreateSampler(samplerDesc);
        }

        private unsafe void InitBuffers<TVertex, TIndice>(GraphicBufferFactory bufferFactory, TVertex[] sommets, TIndice[] indices)
            where TVertex : unmanaged
            where TIndice : unmanaged
        {
            // Create our vertex buffer.
            _vertexBuffer = bufferFactory.CreateVertexBuffer<TVertex>(sommets, Usage.Immutable, CpuAccessFlag.None);

            // Create our index buffer.
            _indexBuffer = bufferFactory.CreateIndexBuffer<TIndice>(indices, Usage.Immutable, CpuAccessFlag.None);

            // Create our constant buffer.
            _constantBuffer = bufferFactory.CreateConstantBuffer<ObjectShadersParams>(Usage.Default, CpuAccessFlag.None);
        }

        /// <summary>
        /// Compilation et chargement du vertex shader
        /// </summary>
        /// <param name="device"></param>
        /// <param name="compiler"></param>
        private unsafe void InitVertexShader(ShaderManager shaderManager)
        {
            // Compilation et chargement du vertex shader
            string filePath = "shaders\\MiniPhongNormalMap.hlsl";
            string entryPoint = "MiniPhongNormalMapVS";
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
            string filePath = "shaders\\MiniPhongNormalMap.hlsl";
            string entryPoint = "MiniPhongNormalMapPS";
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

        public struct SubObjet3D
        {
            public IReadOnlyList<ushort> Indices;
            public Matrix4X4<float> Transformation;
            public Material Material;
        }
    }
}
