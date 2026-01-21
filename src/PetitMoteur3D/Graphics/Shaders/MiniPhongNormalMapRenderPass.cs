using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PetitMoteur3D.Core.Memory;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.Shaders;

internal sealed class MiniPhongNormalMapRenderPass : IDisposable
{
    public string Name { get; init; }

    public ComPtr<ID3D11Buffer> VertexShaderConstantBuffer => _sceneConstantBuffer;
    public ComPtr<ID3D11Buffer> PixelShaderConstantBuffer => _objectConstantBuffer;
    public ComPtr<ID3D11VertexShader> VertexShader => _vertexShader;
    public ComPtr<ID3D11InputLayout> VertexLayout => _vertexLayout;
    public ComPtr<ID3D11PixelShader> PixelShader => _pixelShader;
    public ComPtr<ID3D11SamplerState> SampleState => _sampleState;

    private ComPtr<ID3D11Buffer> _sceneConstantBuffer;
    private ComPtr<ID3D11Buffer> _objectConstantBuffer;
    private ComPtr<ID3D11VertexShader> _vertexShader;
    private ComPtr<ID3D11InputLayout> _vertexLayout;
    private ComPtr<ID3D11PixelShader> _pixelShader;
    private ComPtr<ID3D11SamplerState> _sampleState;

    private ComPtr<ID3D11Buffer> _vertexBuffer;
    private uint _vertexStride;
    private ComPtr<ID3D11Buffer> _indexBuffer;
    private Silk.NET.DXGI.Format _format;
    private D3DPrimitiveTopology _topology;

    private ComPtr<ID3D11ShaderResourceView> _textureD3D;
    private ComPtr<ID3D11ShaderResourceView> _normalMap;

    private readonly D3D11GraphicPipeline _graphicPipeline;

    private bool _disposedValue;

    public MiniPhongNormalMapRenderPass(D3D11GraphicPipeline graphicPipeline, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            Name = this.GetType().Name + "_" + Guid.NewGuid().ToString();
        }
        else
        {
            Name = name;
        }

        _graphicPipeline = graphicPipeline;
        Initialisation(graphicPipeline.GraphicDevice.RessourceFactory);

        _disposedValue = false;
    }

    #region Public methods

    public void UpdatePrimitiveTopology(D3DPrimitiveTopology topology)
    {
        _topology = topology;
    }

    public void SetPrimitiveTopology()
    {
        _graphicPipeline.InputAssemblerStage.SetPrimitiveTopology(_topology);
    }

    public void UpdateVertexBuffer(ComPtr<ID3D11Buffer> vertexBuffer, uint vertexStride)
    {
        _vertexBuffer = vertexBuffer;
        _vertexStride = vertexStride;
    }

    public void UpdateIndexBuffer(ComPtr<ID3D11Buffer> indexBuffer, Silk.NET.DXGI.Format format)
    {
        _indexBuffer = indexBuffer;
        _format = format;
    }

    public void SetVertexBuffer(uint offset = 0)
    {
        _graphicPipeline.InputAssemblerStage.SetVertexBuffers(0, 1, ref _vertexBuffer, in _vertexStride, in offset);
    }

    public void SetIndexBuffer(uint offset = 0)
    {
        _graphicPipeline.InputAssemblerStage.SetIndexBuffer(_indexBuffer, _format, offset);
    }

    public void SetInputLayout()
    {
        _graphicPipeline.InputAssemblerStage.SetInputLayout(_vertexLayout);
    }

    public void UpdateSceneConstantBuffer(SceneConstantBufferParams value)
    {
        _graphicPipeline.RessourceFactory.UpdateSubresource(_sceneConstantBuffer, 0, in Unsafe.NullRef<Box>(), in value, 0, 0);
    }

    public void UpdateObjectConstantBuffer(ObjectConstantBufferParams value)
    {
        _graphicPipeline.RessourceFactory.UpdateSubresource(_objectConstantBuffer, 0, in Unsafe.NullRef<Box>(), in value, 0, 0);
    }

    public void UpdateTexture(ComPtr<ID3D11ShaderResourceView> texture)
    {
        _textureD3D = texture;
    }

    public void UpdateNormalMap(ComPtr<ID3D11ShaderResourceView> normalMap)
    {
        _normalMap = normalMap;
    }

    public void SetVertexShader()
    {
        _graphicPipeline.VertexShaderStage.SetShader(_vertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
    }

    public void SetVertexShaderConstantBuffers()
    {
        _graphicPipeline.VertexShaderStage.SetConstantBuffers(0, 1, ref _sceneConstantBuffer);
        _graphicPipeline.VertexShaderStage.SetConstantBuffers(1, 1, ref _objectConstantBuffer);
    }

    public void SetGeometryShader()
    {
        _graphicPipeline.GeometryShaderStage.SetShader((ComPtr<ID3D11GeometryShader>)null, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
    }

    public void SetPixelShader()
    {
        _graphicPipeline.PixelShaderStage.SetShader(_pixelShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
    }

    public void SetPixelShaderConstantBuffers()
    {
        _graphicPipeline.PixelShaderStage.SetConstantBuffers(0, 1, ref _sceneConstantBuffer);
        _graphicPipeline.PixelShaderStage.SetConstantBuffers(1, 1, ref _objectConstantBuffer);
    }

    public unsafe void SetPixelShaderRessources()
    {
        // Activation de la texture
        if (_textureD3D.Handle is not null)
        {
            _graphicPipeline.PixelShaderStage.SetShaderResources(0, 1, ref _textureD3D);
        }
        else
        {
            _graphicPipeline.PixelShaderStage.ClearShaderResources(0);
        }
        if (_normalMap.Handle is not null)
        {
            _graphicPipeline.PixelShaderStage.SetShaderResources(1, 1, ref _normalMap);
        }
        else
        {
            _graphicPipeline.PixelShaderStage.ClearShaderResources(1);
        }
    }

    public void ClearPixelShaderResources()
    {
        _graphicPipeline.PixelShaderStage.ClearShaderResources(0);
        _graphicPipeline.PixelShaderStage.ClearShaderResources(1);
    }

    public void SetSamplers()
    {
        _graphicPipeline.PixelShaderStage.SetSamplers(0, 1, in _sampleState);
    }

    public void DrawIndexed(uint indexCount, uint startIndexLocation, int baseVertexLocation)
    {
        _graphicPipeline.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
    }
    #endregion

    #region Private methods

    private void Initialisation(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory)
    {
        InitBuffers(graphicDeviceRessourceFactory.BufferFactory);
        InitShaders(graphicDeviceRessourceFactory.ShaderManager);
        InitTextureSampler(graphicDeviceRessourceFactory.TextureManager);
    }

    private unsafe void InitBuffers(GraphicBufferFactory bufferFactory)
    {
        // Create our constant buffer.
        _sceneConstantBuffer = bufferFactory.CreateConstantBuffer<SceneConstantBufferParams>(Usage.Default, CpuAccessFlag.None, $"{Name}_SceneConstantBuffer");

        // Create our constant buffer.
        _objectConstantBuffer = bufferFactory.CreateConstantBuffer<ObjectConstantBufferParams>(Usage.Default, CpuAccessFlag.None, $"{Name}_ObjectConstantBuffer");
    }

    private unsafe void InitShaders(ShaderManager shaderManager)
    {
        InitVertexShader(shaderManager);
        InitPixelShader(shaderManager);
    }

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

    /// <summary>
    /// Compilation et chargement du vertex shader
    /// </summary>
    /// <param name="device"></param>
    /// <param name="compiler"></param>
    private unsafe void InitVertexShader(ShaderManager shaderManager)
    {
        ShaderCodeFile shaderFile = InitVertexShaderCodeFile();
        shaderManager.GetOrLoadVertexShaderAndLayout(shaderFile, Sommet.InputLayoutDesc, ref _vertexShader, ref _vertexLayout);
    }

    /// <summary>
    /// Compilation et chargement du pixel shader
    /// </summary>
    /// <param name="device"></param>
    /// <param name="compiler"></param>
    private unsafe void InitPixelShader(ShaderManager shaderManager)
    {
        ShaderCodeFile shaderFile = InitPixelShaderCodeFile();
        _pixelShader = shaderManager.GetOrLoadPixelShader(shaderFile);
    }

    /// <summary>
    /// VertexShader file
    /// </summary>
    [return: NotNull]
    private static ShaderCodeFile InitVertexShaderCodeFile()
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

    /// <summary>
    /// PixelShader file
    /// </summary>
    [return: NotNull]
    private static ShaderCodeFile InitPixelShaderCodeFile()
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

    private void Dispose(bool disposing)
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
            _vertexShader.Dispose();
            _vertexLayout.Dispose();
            _pixelShader.Dispose();
            _sampleState.Dispose();

            _disposedValue = true;
        }
    }

    ~MiniPhongNormalMapRenderPass()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
