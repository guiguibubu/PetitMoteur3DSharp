using System;
using System.Diagnostics.CodeAnalysis;
using PetitMoteur3D.Graphics.Buffers;
using PetitMoteur3D.Graphics.Shaders;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D.Graphics.RenderTechniques;

internal abstract class BaseRenderPass : IRenderPass, IVisitor<Scene>, IVisitor<SceneNode<IObjet3D>>, IVisitor<IObjet3D>
{
    public bool IsEnabled { get; set; } = true;
    public string Name { get; init; }

    protected D3D11GraphicPipeline GraphicPipeline => _graphicPipeline;
    protected D3D11GraphicPipelineState GraphicPipelineState => _graphicPipelineState;

    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private D3DPrimitiveTopology _topology;

    private readonly D3D11GraphicPipeline _graphicPipeline;
    private readonly D3D11GraphicPipelineState _graphicPipelineState;

    protected RenderArgs RenderArgs => _renderArgs;
    private RenderArgs _renderArgs;

    private bool _disposedValue;

    public BaseRenderPass(D3D11GraphicPipeline graphicPipeline, RenderTarget renderTarget, string name = "")
    {
        if (string.IsNullOrEmpty(name))
        {
            Name = GetType().Name + "_" + Guid.NewGuid().ToString();
        }
        else
        {
            Name = name;
        }

        _graphicPipeline = graphicPipeline;
        _graphicPipelineState = new D3D11GraphicPipelineState(graphicPipeline, renderTarget);
        GraphicDeviceRessourceFactory graphicDeviceRessourceFactory = graphicPipeline.GraphicDevice.RessourceFactory;
        InitPipelineState(_graphicPipelineState, graphicDeviceRessourceFactory.ShaderFactory);
        Initialisation(graphicDeviceRessourceFactory);

        _renderArgs = new RenderArgs();

        _disposedValue = false;
    }

    #region Public methods
    #region Update Values
    public void UpdatePrimitiveTopology(D3DPrimitiveTopology topology)
    {
        _topology = topology;
    }

    public void UpdateVertexBuffer(VertexBuffer vertexBuffer)
    {
        _vertexBuffer = vertexBuffer;
    }

    public void UpdateIndexBuffer(IndexBuffer indexBuffer)
    {
        _indexBuffer = indexBuffer;
    }
    #endregion

    #region Input Assembler
    public void BindPrimitiveTopology()
    {
        _graphicPipeline.InputAssemblerStage.SetPrimitiveTopology(_topology);
    }

    public void BindVertexBuffer()
    {
        _vertexBuffer.Bind(_graphicPipeline, idSlot: 0);
    }

    public void BindIndexBuffer()
    {
        _indexBuffer.Bind(_graphicPipeline);
    }
    #endregion

    #region Vertex Shader
    public abstract void BindVertexShaderConstantBuffers();
    #endregion

    #region Geometry Shader
    #endregion

    #region Pixel Shader
    public abstract void BindPixelShaderConstantBuffers();

    public abstract void BindPixelShaderRessources();

    public abstract void BindSamplers();

    public abstract void ClearPixelShaderResources();
    #endregion

    public void Bind()
    {
        BindPrimitiveTopology();
        BindVertexBuffer();
        BindIndexBuffer();
        BindVertexShaderConstantBuffers();
        BindPixelShaderConstantBuffers();
        BindPixelShaderRessources();
        BindSamplers();

        _graphicPipelineState.RasterizerState = GetRasterizerState();
        _graphicPipelineState.DepthStencilState = GetDepthStencilState();
        _graphicPipelineState.Bind();
    }

    protected void DrawIndexed(uint indexCount, uint startIndexLocation, int baseVertexLocation)
    {
        _graphicPipeline.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
    }

    public virtual void Visit(Scene scene)
    {
        UpdateSceneContext(scene.GetSceneViewContext());
    }

    public virtual void Visit(SceneNode<IObjet3D> node)
    {
        Render(node);
    }

    public virtual void Visit(IObjet3D objet3D)
    {
        if (objet3D is BaseObjet3D baseObjet3D)
        {
            Render(baseObjet3D);
        }
    }

    public void Render(Scene scene)
    {
        RenderCoreImpl(scene);
    }

    public void Render(SceneNode<IObjet3D> node)
    {
        RenderCoreImpl(node);
    }

    public void Render(BaseObjet3D baseObjet3D)
    {
        RenderCoreImpl(baseObjet3D);
    }

    #endregion

    #region Protected methods
    /// <summary>
    /// Init constant buffers
    /// </summary>
    /// <param name="bufferFactory"></param>
    protected abstract void InitBuffers(GraphicBufferFactory bufferFactory);

    /// <summary>
    /// Update Vertex Buffer from Mesh
    /// </summary>
    /// <param name="baseObjet3D"></param>
    protected abstract void UpdateVertexBuffer(BaseObjet3D baseObjet3D);

    /// <summary>
    /// Update constant buffers from Mesh
    /// </summary>
    /// <param name="mesh"></param>
    protected abstract void UpdatePerMeshRessourcesBuffers(Mesh mesh);

    /// <summary>
    /// Get vertex input layout description
    /// </summary>
    /// <returns></returns>
    protected abstract InputLayoutDesc GetInputLayoutDesc();

    /// <summary>
    /// Get rasterizer state
    /// </summary>
    /// <returns></returns>
    protected abstract ComPtr<ID3D11RasterizerState> GetRasterizerState();

    /// <summary>
    /// Get depth stencil state
    /// </summary>
    /// <returns></returns>
    protected abstract ComPtr<ID3D11DepthStencilState> GetDepthStencilState();

    /// <summary>
    /// VertexShader file
    /// </summary>
    [return: NotNull]
    protected abstract ShaderCodeFile InitVertexShaderCodeFile();

    /// <summary>
    /// PixelShader file
    /// </summary>
    protected abstract ShaderCodeFile? InitPixelShaderCodeFile();

    /// <summary>
    /// Init Impl
    /// </summary>
    protected abstract void InitialisationImpl(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory);

    /// <summary>
    /// Update scene view context
    /// </summary>
    /// <param name="sceneContext"></param>
    protected virtual void UpdateSceneContext(SceneViewContext sceneContext)
    {
        _renderArgs.SceneContext = sceneContext;
    }

    /// <summary>
    /// Update object view context
    /// </summary>
    /// <param name="objectContext"></param>
    protected virtual void UpdateObjectContext(ObjectViewContext objectContext)
    {
        _renderArgs.ObjectContext.MatWorld = objectContext.MatWorld;
    }

    /// <summary>
    /// Render scene core impl
    /// </summary>
    /// <param name="scene"></param>
    protected virtual void RenderCoreImpl(Scene scene)
    {
        scene.Accept(this);
    }

    protected virtual void RenderCoreImpl(SceneNode<IObjet3D> node)
    {
        _renderArgs.ObjectContext.AdditionalTransformation = node.Transformation;
    }

    protected virtual void RenderCoreImpl(BaseObjet3D baseObjet3D)
    {
        UpdatePrimitiveTopology(baseObjet3D.Topology);
        UpdateVertexBuffer(baseObjet3D);
        UpdateIndexBuffer(baseObjet3D.IndexBuffer);
        UpdateObjectContext(baseObjet3D.GetViewContext());
        UpdatePerMeshRessourcesBuffers(baseObjet3D.Mesh);

        Bind();
        DrawIndexed(_indexBuffer.Buffer.NbElements, 0, 0);

        ClearPixelShaderResources();
        _graphicPipeline.OutputMergerStage.UnbindRenderTargets();
    }
    #endregion

    #region Private methods

    private void Initialisation(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory)
    {
        InitBuffers(graphicDeviceRessourceFactory.BufferFactory);
        InitialisationImpl(graphicDeviceRessourceFactory);
    }

    private unsafe void InitPipelineState([DisallowNull] D3D11GraphicPipelineState graphicPipeline, ShaderFactory shaderFactory)
    {
        _graphicPipelineState.VertexShader = InitVertexShader(shaderFactory);
        _graphicPipelineState.GeometryShader = InitGeometryShader(shaderFactory);
        _graphicPipelineState.PixelShader = InitPixelShader(shaderFactory);
    }

    /// <summary>
    /// Compilation et chargement du vertex shader
    /// </summary>
    /// <param name="shaderFactory"></param>
    private VertexShader InitVertexShader(ShaderFactory shaderFactory)
    {
        ShaderCodeFile shaderFile = InitVertexShaderCodeFile();
        return shaderFactory.CreateVertexShader(shaderFile, GetInputLayoutDesc());
    }

    /// <summary>
    /// Compilation et chargement du geometry shader
    /// </summary>
    /// <param name="shaderFactory"></param>
    private ComPtr<ID3D11GeometryShader> InitGeometryShader(ShaderFactory shaderFactory)
    {
        return (ComPtr<ID3D11GeometryShader>)null;
    }

    /// <summary>
    /// Compilation et chargement du pixel shader
    /// </summary>
    /// <param name="device"></param>
    /// <param name="compiler"></param>
    private PixelShader? InitPixelShader(ShaderFactory shaderFactory)
    {
        ShaderCodeFile? shaderFile = InitPixelShaderCodeFile();
        if (shaderFile is null)
        {
            return null;
        }
        else
        {
            return shaderFactory.CreatePixelShader(shaderFile);
        }
    }

    #endregion

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null

            _disposedValue = true;
        }
    }

    ~BaseRenderPass()
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
