using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PetitMoteur3D.Core.Memory;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Shaders;

internal sealed class MiniPhongNormalMapRenderPass : BaseRenderPass, IDisposable
{
    public ComPtr<ID3D11Buffer> VertexShaderConstantBuffer => _sceneConstantBuffer;
    public ComPtr<ID3D11Buffer> PixelShaderConstantBuffer => _objectConstantBuffer;
    public ComPtr<ID3D11SamplerState> SampleState => _sampleState;

    private ComPtr<ID3D11Buffer> _sceneConstantBuffer;
    private ComPtr<ID3D11Buffer> _objectConstantBuffer;
    private ComPtr<ID3D11SamplerState> _sampleState;

    private ComPtr<ID3D11ShaderResourceView> _textureD3D;
    private ComPtr<ID3D11ShaderResourceView> _normalMap;

    private bool _disposedValue;

    public MiniPhongNormalMapRenderPass(D3D11GraphicPipeline graphicPipeline, string name = "")
        : base(graphicPipeline, name)
    {
        _disposedValue = false;
    }

    #region Public methods
    #region Update Values
    public void UpdateSceneConstantBuffer(SceneConstantBufferParams value)
    {
        GraphicPipeline.RessourceFactory.UpdateSubresource(_sceneConstantBuffer, 0, in Unsafe.NullRef<Box>(), in value, 0, 0);
    }

    public void UpdateObjectConstantBuffer(ObjectConstantBufferParams value)
    {
        GraphicPipeline.RessourceFactory.UpdateSubresource(_objectConstantBuffer, 0, in Unsafe.NullRef<Box>(), in value, 0, 0);
    }

    public void UpdateTexture(ComPtr<ID3D11ShaderResourceView> texture)
    {
        _textureD3D = texture;
    }

    public void UpdateNormalMap(ComPtr<ID3D11ShaderResourceView> normalMap)
    {
        _normalMap = normalMap;
    }
    #endregion

    #region Vertex Shader
    public override void SetVertexShaderConstantBuffers()
    {
        GraphicPipeline.VertexShaderStage.SetConstantBuffers(0, 1, ref _sceneConstantBuffer);
        GraphicPipeline.VertexShaderStage.SetConstantBuffers(1, 1, ref _objectConstantBuffer);
    }
    #endregion

    #region Pixel Shader
    public override void SetPixelShaderConstantBuffers()
    {
        GraphicPipeline.PixelShaderStage.SetConstantBuffers(0, 1, ref _sceneConstantBuffer);
        GraphicPipeline.PixelShaderStage.SetConstantBuffers(1, 1, ref _objectConstantBuffer);
    }

    public override unsafe void SetPixelShaderRessources()
    {
        // Activation de la texture
        if (_textureD3D.Handle is not null)
        {
            GraphicPipeline.PixelShaderStage.SetShaderResources(0, 1, ref _textureD3D);
        }
        else
        {
            GraphicPipeline.PixelShaderStage.ClearShaderResources(0);
        }
        if (_normalMap.Handle is not null)
        {
            GraphicPipeline.PixelShaderStage.SetShaderResources(1, 1, ref _normalMap);
        }
        else
        {
            GraphicPipeline.PixelShaderStage.ClearShaderResources(1);
        }
    }

    public override void SetSamplers()
    {
        GraphicPipeline.PixelShaderStage.SetSamplers(0, 1, in _sampleState);
    }

    public override void ClearPixelShaderResources()
    {
        GraphicPipeline.PixelShaderStage.ClearShaderResources(0);
        GraphicPipeline.PixelShaderStage.ClearShaderResources(1);
    }
    #endregion
    #endregion

    #region Protected methods
    /// <inheritdoc/>
    protected override void InitBuffers(GraphicBufferFactory bufferFactory)
    {
        // Create our constant buffer.
        _sceneConstantBuffer = bufferFactory.CreateConstantBuffer<SceneConstantBufferParams>(Usage.Default, CpuAccessFlag.None, $"{Name}_SceneConstantBuffer");

        // Create our constant buffer.
        _objectConstantBuffer = bufferFactory.CreateConstantBuffer<ObjectConstantBufferParams>(Usage.Default, CpuAccessFlag.None, $"{Name}_ObjectConstantBuffer");
    }

    /// <inheritdoc/>
    protected override InputElementDesc[] GetInputLayoutDesc()
    {
        return Sommet.InputLayoutDesc;
    }

    /// <inheritdoc/>
    [return: NotNull]
    protected override ShaderCodeFile InitVertexShaderCodeFile()
    {
        // Compilation et chargement du vertex shader
        string filePath = "shaders\\MiniPhongNormalMap_VS.hlsl";
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
        return new ShaderCodeFile
        (
            filePath,
            entryPoint,
            target,
            compilationFlags,
            name: "MiniPhongNormalMap_VertexShader"
        );
    }

    /// <inheritdoc/>
    [return: NotNull]
    protected override ShaderCodeFile? InitPixelShaderCodeFile()
    {
        string filePath = "shaders\\MiniPhongNormalMap_PS.hlsl";
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
        return new ShaderCodeFile
        (
           filePath,
           entryPoint,
           target,
           compilationFlags,
           name: "MiniPhongNormalMap_PixelShader"
        );
    }

    /// <inheritdoc/>
    protected override void InitialisationImpl(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory)
    {
        InitTextureSampler(graphicDeviceRessourceFactory.TextureManager);
    }
    #endregion

    #region Private methods
    private unsafe void InitTextureSampler(TextureManager textureManager)
    {
        // Initialisation des paramètres de sampling de la texture
        SamplerDesc samplerDesc = new()
        {
            Filter = Filter.Anisotropic,
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap,
            MipLODBias = 0f,
            MaxAnisotropy = 4,
            ComparisonFunc = ComparisonFunc.Always,
            MinLOD = 0,
            MaxLOD = float.MaxValue,
        };
        samplerDesc.BorderColor[0] = 0f;
        samplerDesc.BorderColor[1] = 0f;
        samplerDesc.BorderColor[2] = 0f;
        samplerDesc.BorderColor[3] = 0f;

        // Création de l’état de sampling
        _sampleState = textureManager.Factory.CreateSampler(samplerDesc, $"{Name}_SamplerState");
    }
    #endregion

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct LightParams
    {
        /// <summary>
        /// la position de la source d’éclairage (Source Point)
        /// </summary>
        public Vector4 Position;
        /// <summary>
        /// la direction de la source d’éclairage (Source Directionnelle)
        /// </summary>
        public Vector4 Direction;
        /// <summary>
        /// la valeur ambiante de l’éclairage
        /// </summary>
        public Vector4 AmbiantColor;
        /// <summary>
        /// la valeur diffuse de l’éclairage
        /// </summary>
        public Vector4 DiffuseColor;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct SceneConstantBufferParams : IResetable
    {
        /// <summary>
        /// les infos de la lumiere
        /// </summary>
        public LightParams LightParams;
        /// <summary>
        /// la position de la caméra
        /// </summary>
        public Vector4 CameraPos;

        public void Reset()
        {
            MemoryHelper.ResetMemory(this);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct ObjectConstantBufferParams : IResetable
    {
        /// <summary>
        /// la matrice totale
        /// </summary>
        public Matrix4x4 matWorldViewProj;
        /// <summary>
        /// matrice de transformation dans le monde
        /// </summary>
        public Matrix4x4 matWorld;
        /// <summary>
        /// la valeur ambiante du matériau
        /// </summary>
        public Vector4 ambiantMaterialValue;
        /// <summary>
        /// la valeur diffuse du matériau
        /// </summary>
        public Vector4 diffuseMaterialValue;
        /// <summary>
        /// Indique la présence d'une texture
        /// </summary>
        public int hasTexture;
        /// <summary>
        /// Indique la présence d'une texture pour le "normal mapping"
        /// </summary>
        public int hasNormalMap;
        private readonly ulong alignement1_1;

        public void Reset()
        {
            MemoryHelper.ResetMemory(this);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _sceneConstantBuffer.Dispose();
            _objectConstantBuffer.Dispose();
            _sampleState.Dispose();

            base.Dispose(disposing);

            _disposedValue = true;
        }
    }

    ~MiniPhongNormalMapRenderPass()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }
}
