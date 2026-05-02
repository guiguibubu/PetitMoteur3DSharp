using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PetitMoteur3D.Core.Memory;
using PetitMoteur3D.Graphics.Buffers;
using PetitMoteur3D.Graphics.Shaders;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using ShaderType = PetitMoteur3D.Graphics.Shaders.ShaderType;

namespace PetitMoteur3D.Graphics.RenderTechniques;

internal sealed class DeferredLightningRenderPass : BaseRenderPass, IDisposable
{
    private ConstantBuffer _sceneConstantBuffer;
    private ConstantBuffer _vertexObjectConstantBuffer;

    private Texture _geometryBufferLightAccumulation;
    private Texture _geometryBufferDiffuse;
    private Texture _geometryBufferSpecular;
    private Texture _geometryBufferNormal;
    private Texture _geometryBufferShadow;

    private ObjetMesh _fullScreenQuad;

    private bool _disposedValue;

    public DeferredLightningRenderPass(D3D11GraphicPipeline graphicPipeline, RenderTarget renderTarget, Texture geometryBufferLightAccumulation, Texture geometryBufferDiffuse, Texture geometryBufferSpecular, Texture geometryBufferNormal, Texture geometryBufferShadow, MeshFactory meshFactory, string name = "")
        : base(graphicPipeline, renderTarget, name)
    {
        _geometryBufferLightAccumulation = geometryBufferLightAccumulation;
        _geometryBufferDiffuse = geometryBufferDiffuse;
        _geometryBufferSpecular = geometryBufferSpecular;
        _geometryBufferNormal = geometryBufferNormal;
        _geometryBufferShadow = geometryBufferShadow;

        /// <summary>
        /// Create a screen-space quad that can be used to render full-screen post-process effects to the screen.
        /// By default, the quad will have clip-space coordinates and can be used with a pass-through vertex shader
        /// to render full-screen post-process effects. If you want more control over the area of the screen the quad covers, 
        /// you can specify your own screen coordinates and supply an appropriate orthographic projection matrix to align the 
        /// screen quad appropriately.
        /// </summary>
        Mesh meshScreenQuad = meshFactory.CreateQuad(-1, 1, -1, 1, 1);

        _fullScreenQuad = new ObjetMesh(meshScreenQuad, graphicPipeline.GraphicDevice.RessourceFactory, "DeferredLightning_ScreenQuad");

        _disposedValue = false;
    }

    #region Public methods
    #region Update Values
    public void UpdateSceneConstantBuffer(SceneConstantBufferParams value)
    {
        _sceneConstantBuffer.Buffer.UpdateSubresource(GraphicPipeline.DeviceContext, 0, in Unsafe.NullRef<Box>(), in value, 0, 0);
    }

    public void UpdateVertexObjectConstantBuffer(VertexObjectConstantBufferParams value)
    {
        _vertexObjectConstantBuffer.Buffer.UpdateSubresource(GraphicPipeline.DeviceContext, 0, in Unsafe.NullRef<Box>(), in value, 0, 0);
    }
    #endregion

    #region Vertex Shader
    public override void BindVertexShaderConstantBuffers()
    {
        _vertexObjectConstantBuffer.Bind(GraphicPipeline, ShaderType.VertexShader, idSlot: 0);
    }
    #endregion

    #region Pixel Shader
    public override void BindPixelShaderConstantBuffers()
    {
        _sceneConstantBuffer.Bind(GraphicPipeline, ShaderType.PixelShader, idSlot: 0);
    }

    public override unsafe void BindPixelShaderRessources()
    {
        ComPtr<ID3D11ShaderResourceView> textureLightAccumulation = _geometryBufferLightAccumulation.ShaderRessourceView;
        ComPtr<ID3D11ShaderResourceView> textureDiffuse = _geometryBufferDiffuse.ShaderRessourceView;
        ComPtr<ID3D11ShaderResourceView> textureSpecular = _geometryBufferSpecular.ShaderRessourceView;
        ComPtr<ID3D11ShaderResourceView> textureNormal = _geometryBufferNormal.ShaderRessourceView;
        ComPtr<ID3D11ShaderResourceView> textureShadow = _geometryBufferShadow.ShaderRessourceView;

        // Activation de la texture
        if (textureLightAccumulation.Handle is not null)
        {
            GraphicPipeline.PixelShaderStage.SetShaderResources(0, 1, ref textureLightAccumulation);
        }
        else
        {
            GraphicPipeline.PixelShaderStage.ClearShaderResources(0);
        }

        if (textureDiffuse.Handle is not null)
        {
            GraphicPipeline.PixelShaderStage.SetShaderResources(1, 1, ref textureDiffuse);
        }
        else
        {
            GraphicPipeline.PixelShaderStage.ClearShaderResources(1);
        }

        if (textureSpecular.Handle is not null)
        {
            GraphicPipeline.PixelShaderStage.SetShaderResources(2, 1, ref textureSpecular);
        }
        else
        {
            GraphicPipeline.PixelShaderStage.ClearShaderResources(2);
        }

        if (textureNormal.Handle is not null)
        {
            GraphicPipeline.PixelShaderStage.SetShaderResources(3, 1, ref textureNormal);
        }
        else
        {
            GraphicPipeline.PixelShaderStage.ClearShaderResources(3);
        }
        
        if (textureShadow.Handle is not null)
        {
            GraphicPipeline.PixelShaderStage.SetShaderResources(4, 1, ref textureShadow);
        }
        else
        {
            GraphicPipeline.PixelShaderStage.ClearShaderResources(4);
        }
    }

    public override void BindSamplers()
    {
    }

    public override void ClearPixelShaderResources()
    {
        GraphicPipeline.PixelShaderStage.ClearShaderResources(0);
        GraphicPipeline.PixelShaderStage.ClearShaderResources(1);
        GraphicPipeline.PixelShaderStage.ClearShaderResources(2);
        GraphicPipeline.PixelShaderStage.ClearShaderResources(3);
        GraphicPipeline.PixelShaderStage.ClearShaderResources(4);
    }
    #endregion

    public override void Visit(Scene scene)
    {
        base.Visit(scene);
        Render(_fullScreenQuad);
    }

    public override void Visit(SceneNode<IObjet3D> node)
    {
    }

    public override void Visit(IObjet3D objet3D)
    {
    }
    #endregion

    #region Protected methods
    /// <inheritdoc/>
    protected sealed override void InitBuffers(GraphicBufferFactory bufferFactory)
    {
        // Create our constant buffer.
        _sceneConstantBuffer = bufferFactory.CreateConstantBuffer<SceneConstantBufferParams>(Usage.Default, CpuAccessFlag.None, $"{Name}_SceneConstantBuffer");

        // Create our constant buffer.
        _vertexObjectConstantBuffer = bufferFactory.CreateConstantBuffer<VertexObjectConstantBufferParams>(Usage.Default, CpuAccessFlag.None, $"{Name}_VertexObjectConstantBuffer");
    }

    protected override void UpdateVertexBuffer(BaseObjet3D baseObjet3D)
    {
        UpdateVertexBuffer(baseObjet3D.VertexBuffer);
    }

    protected override void UpdatePerMeshRessourcesBuffers(Mesh mesh)
    {
        SceneViewContext sceneContext = RenderArgs.SceneContext;
        Matrix4x4 matViewProj = sceneContext.MatViewProj;
        Matrix4x4 matWorld = RenderArgs.ObjectContext.MatWorld;
        // Initialiser et sélectionner les « constantes » des shaders
        UpdateSceneConstantBuffer(new DeferredLightningRenderPass.SceneConstantBufferParams()
        {
            LightParams = new DeferredLightningRenderPass.LightParams()
            {
                Position = sceneContext.Light.Position,
                Direction = sceneContext.Light.Direction,
                AmbiantColor = sceneContext.Light.AmbiantColor,
                DiffuseColor = sceneContext.Light.DiffuseColor,
                Enable = Convert.ToInt32(true),
            },
            CameraPos = sceneContext.GameCameraPos
        });

        Matrix4x4 matrixWorld = RenderArgs.ObjectContext.AdditionalTransformation * matWorld;
        UpdateVertexObjectConstantBuffer(new DeferredLightningRenderPass.VertexObjectConstantBufferParams()
        {
            matWorldViewProj = Matrix4x4.Transpose(matrixWorld * matViewProj),
            matWorld = Matrix4x4.Transpose(matrixWorld),
        });
    }

    /// <inheritdoc/>
    protected sealed override InputLayoutDesc GetInputLayoutDesc()
    {
        return Sommet.InputLayoutDesc;
    }

    /// <inheritdoc/>
    protected override ComPtr<ID3D11RasterizerState> GetRasterizerState()
    {
        return GraphicPipeline.SolidCullBackRS;
    }

    /// <inheritdoc/>
    protected override ComPtr<ID3D11DepthStencilState> GetDepthStencilState()
    {
        return GraphicPipeline.ReadonlyGreaterDSS;
    }

    /// <inheritdoc/>
    [return: NotNull]
    protected override ShaderCodeFile InitVertexShaderCodeFile()
    {
        // Compilation et chargement du vertex shader
        string filePath = "shaders\\DeferredShadingLightningPass_VS.hlsl";
        string entryPoint = "DeferredShadingLightningPassVS";
        string target = "vs_5_0";
        // #define D3DCOMPILE_ENABLE_STRICTNESS                    (1 << 11)
        uint flagStrictness = (uint)1 << 11;
        // #define D3DCOMPILE_DEBUG (1 << 0)
        // #define D3DCOMPILE_SKIP_OPTIMIZATION                    (1 << 2)
#if DEBUG
        uint flagDebug = (uint)1 << 0;
        uint flagSkipOptimization = 1 << 2;
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
            name: "DeferredShadingLightningPass_VertexShader"
        );
    }

    /// <inheritdoc/>
    [return: NotNull]
    protected override ShaderCodeFile? InitPixelShaderCodeFile()
    {
        string filePath = "shaders\\DeferredShadingLightningPass_PS.hlsl";
        string entryPoint = "DeferredShadingLightningPassPS";
        string target = "ps_5_0";
        // #define D3DCOMPILE_ENABLE_STRICTNESS                    (1 << 11)
        uint flagStrictness = (uint)1 << 11;
        // #define D3DCOMPILE_DEBUG (1 << 0)
        // #define D3DCOMPILE_SKIP_OPTIMIZATION                    (1 << 2)
#if DEBUG
        uint flagDebug = (uint)1 << 0;
        uint flagSkipOptimization = 1 << 2;
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
           name: "DeferredShadingLightningPass_PixelShader"
        );
    }

    /// <inheritdoc/>
    protected sealed override void InitialisationImpl(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory)
    {
    }

    /// <inheritdoc/>
    protected sealed override void UpdateSceneContext(SceneViewContext sceneContext)
    {
        sceneContext.MatViewProj = sceneContext.MatViewProj = Matrix4x4.CreateOrthographicOffCenterLeftHanded(-1, 1, -1, 1, 0, 1);
        base.UpdateSceneContext(sceneContext);
    }

    #endregion

    #region Private methods
    #endregion

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct LightParams
    {
        /// <summary>
        /// la position de la source d’éclairage (Source Point)
        /// </summary>
        public Vector4 Position;
        // ---- 16 bytes ----
        /// <summary>
        /// la direction de la source d’éclairage (Source Directionnelle)
        /// </summary>
        public Vector4 Direction;
        // ---- 16 bytes ----
        /// <summary>
        /// la valeur ambiante de l’éclairage
        /// </summary>
        public Vector4 AmbiantColor;
        // ---- 16 bytes ----
        /// <summary>
        /// la valeur diffuse de l’éclairage
        /// </summary>
        public Vector4 DiffuseColor;
        // ---- 16 bytes ----
        /// <summary>
        /// Indique la lumiere est active
        /// </summary>
        public int Enable;
        /// <summary>
        /// Indique la lumiere fait une ombre (ShadowMapping)
        /// </summary>
        public int EnableShadow;
        private readonly uint alignement1_1;
        private readonly uint alignement1_2;
        // ---- 16 bytes ----
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
    internal struct VertexObjectConstantBufferParams : IResetable
    {
        /// <summary>
        /// la matrice totale
        /// </summary>
        public Matrix4x4 matWorldViewProj;

        /// <summary>
        /// matrice de transformation dans le monde
        /// </summary>
        public Matrix4x4 matWorld;

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
            _vertexObjectConstantBuffer.Dispose();

            base.Dispose(disposing);

            _disposedValue = true;
        }
    }

    ~DeferredLightningRenderPass()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }
}
