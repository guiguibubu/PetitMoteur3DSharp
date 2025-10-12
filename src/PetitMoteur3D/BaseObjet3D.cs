using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PetitMoteur3D
{
    internal abstract class BaseObjet3D : IObjet3D
    {
        /// <inheritdoc/>
        public ref readonly Vector3D<float> Position { get { return ref _position; } }
        /// <summary>
        /// Rotation de l'objet
        /// </summary>
        public ref readonly Vector3D<float> Rotation { get { return ref _rotation; } }

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

        private unsafe readonly uint _vertexStride = (uint)sizeof(Sommet);
        private static readonly uint _vertexOffset = 0;

        private string _name;

        private static IObjectPool<ObjectShadersParams> _objectShadersParamsPool = ObjectPoolFactory.Create<ObjectShadersParams>();
        private static IObjectPool<SamplerDesc> _shaderDescPool = ObjectPoolFactory.Create<SamplerDesc>(new DX11SamplerDescResetter());

        private readonly GraphicBufferFactory _bufferFactory;
        private readonly ShaderManager _shaderManager;
        private readonly TextureManager _textureManager;

        protected BaseObjet3D(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, string name = "")
        {
            _position = Vector3D<float>.Zero;
            _rotation = Vector3D<float>.Zero;
            _matWorld = Matrix4X4<float>.Identity;

            _sommets = Array.Empty<Sommet>();
            _indices = Array.Empty<ushort>();

            if (string.IsNullOrEmpty(name))
            {
                _name = this.GetType().Name + "_" + Guid.NewGuid().ToString();
            }
            else
            {
                _name = name;
            }

            _bufferFactory = graphicDeviceRessourceFactory.BufferFactory;
            _shaderManager = graphicDeviceRessourceFactory.ShaderManager;
            _textureManager = graphicDeviceRessourceFactory.TextureManager;
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
        public ref readonly Vector3D<float> Move(ref readonly Vector3D<float> move)
        {
            _position.X += move.X;
            _position.Y += move.Y;
            _position.Z += move.Z;
            return ref _position;
        }

        /// <inheritdoc/>
        public ref readonly Vector3D<float> Rotate(ref readonly Vector3D<float> rotation)
        {
            _rotation.X += rotation.X;
            _rotation.Y += rotation.Y;
            _rotation.Z += rotation.Z;
            return ref _rotation;
        }

        /// <inheritdoc/>
        public virtual void Anime(float elapsedTime)
        {
            _rotation.Y += (float)((Math.PI * 2.0f) / 24.0f * elapsedTime / 1000f);
            // modifier la matrice de l’objet bloc
            _matWorld = Matrix4X4.CreateRotationY(_rotation.Y) * Matrix4X4.CreateTranslation(_position);
        }

        /// <inheritdoc/>
        public unsafe void Draw(ref readonly ComPtr<ID3D11DeviceContext> deviceContext, ref readonly Matrix4X4<float> matViewProj)
        {
            // Choisir la topologie des primitives
            deviceContext.IASetPrimitiveTopology(D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
            // Source des sommets

            deviceContext.IASetVertexBuffers(0, 1, ref _vertexBuffer, in _vertexStride, in _vertexOffset);
            // Source des index
            deviceContext.IASetIndexBuffer(_indexBuffer, Silk.NET.DXGI.Format.FormatR16Uint, 0);
            // input layout des sommets
            deviceContext.IASetInputLayout(_vertexLayout);
            foreach (SubObjet3D subObjet3D in GetSubObjets())
            {
                // Initialiser et sélectionner les « constantes » des shaders
                _objectShadersParamsPool.Get(out ObjectPoolWrapper<ObjectShadersParams> shadersParamsWrapper);
                ref ObjectShadersParams shadersParams = ref shadersParamsWrapper.Data;
                shadersParams.matWorldViewProj = Matrix4X4.Transpose(subObjet3D.Transformation * _matWorld * matViewProj);
                shadersParams.matWorld = Matrix4X4.Transpose(subObjet3D.Transformation * _matWorld);
                shadersParams.ambiantMaterialValue = subObjet3D.Material.Ambient;
                shadersParams.diffuseMaterialValue = subObjet3D.Material.Diffuse;
                shadersParams.hasTexture = Convert.ToInt32(_textureD3D.Handle is not null);
                shadersParams.hasNormalMap = Convert.ToInt32(_normalMap.Handle is not null);

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

                _objectShadersParamsPool.Return(shadersParamsWrapper);
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
            _shaderDescPool.Get(out ObjectPoolWrapper<SamplerDesc> samplerDescWrapper);
            ref SamplerDesc samplerDesc = ref samplerDescWrapper.Data;
            samplerDesc.Filter = Filter.Anisotropic;
            samplerDesc.AddressU = TextureAddressMode.Wrap;
            samplerDesc.AddressV = TextureAddressMode.Wrap;
            samplerDesc.AddressW = TextureAddressMode.Wrap;
            samplerDesc.MipLODBias = 0f;
            samplerDesc.MaxAnisotropy = 4;
            samplerDesc.ComparisonFunc = ComparisonFunc.Always;
            samplerDesc.MinLOD = 0;
            samplerDesc.MaxLOD = float.MaxValue;
            samplerDesc.BorderColor[0] = 0f;
            samplerDesc.BorderColor[1] = 0f;
            samplerDesc.BorderColor[2] = 0f;
            samplerDesc.BorderColor[3] = 0f;

            // Création de l’état de sampling
            _sampleState = textureManager.Factory.CreateSampler(samplerDesc, $"{_name}_SamplerState");

            _shaderDescPool.Return(samplerDescWrapper);
        }

        private unsafe void InitBuffers<TVertex, TIndice>(GraphicBufferFactory bufferFactory, TVertex[] sommets, TIndice[] indices)
            where TVertex : unmanaged
            where TIndice : unmanaged
        {
            // Create our vertex buffer.
            _vertexBuffer = bufferFactory.CreateVertexBuffer<TVertex>(sommets, Usage.Immutable, CpuAccessFlag.None, $"{_name}_VertexBuffer");

            // Create our index buffer.
            _indexBuffer = bufferFactory.CreateIndexBuffer<TIndice>(indices, Usage.Immutable, CpuAccessFlag.None, $"{_name}_IndexBuffer");

            // Create our constant buffer.
            _constantBuffer = bufferFactory.CreateConstantBuffer<ObjectShadersParams>(Usage.Default, CpuAccessFlag.None, $"{_name}_IndexBuffer");
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
            ShaderCodeFile shaderFile = new
            (
                filePath,
                entryPoint,
                target,
                compilationFlags,
                name: "MiniPhongNormalMap_VertexShader"
            );
            shaderManager.GetOrLoadVertexShaderAndLayout(shaderFile, Sommet.InputLayoutDesc, ref _vertexShader, ref _vertexLayout);
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
            ShaderCodeFile shaderFile = new
            (
                filePath,
                entryPoint,
                target,
                compilationFlags,
                name: "MiniPhongNormalMap_PixelShader"
            );
            _pixelShader = shaderManager.GetOrLoadPixelShader(shaderFile);
        }

        public struct SubObjet3D
        {
            public IReadOnlyList<ushort> Indices;
            public Matrix4X4<float> Transformation;
            public Material Material;
        }
    }
}
