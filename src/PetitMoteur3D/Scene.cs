using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using PetitMoteur3D.Camera;
using PetitMoteur3D.Graphics;
using PetitMoteur3D.Graphics.Shaders;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace PetitMoteur3D;

internal sealed class Scene : IDisposable
{
    public bool ShowDepthTest { get; set; }
    public bool ShowShadow { get; set; }
    public bool UseDebugCam { get; set; }
    public ICamera GameCamera => _gameCamera;
    public LightShadersParams Light => _light;

    private readonly List<IUpdatableObjet> _objectsUpdatable;
    private readonly Dictionary<RenderPassType, List<IDrawableObjet>> _objectsDrawablePerRenderPass;
    private ICamera _gameCamera;

    public ComPtr<ID3D11RasterizerState> RasterizerState { get { return _rasterizerState; } set { _rasterizerState = value; } }
    private ComPtr<ID3D11RasterizerState> _rasterizerState;

    private LightShadersParams _light;

    private readonly ShadowMap _shadowMap;
    private readonly ShadowMap _debugDepthMap;
    private readonly FreeCamera _cameraLigth;

    private const int DistanceLight = 50;
    private bool _disposed;

    /// <summary>
    /// Constructeur par défaut
    /// </summary>
    public Scene(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, Size windowSize) : this(graphicDeviceRessourceFactory, new FixedCamera(Vector3.Zero), windowSize)
    { }

    public Scene(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, ICamera camera, Size windowSize)
    {
        _objectsUpdatable = new List<IUpdatableObjet>();
        _objectsDrawablePerRenderPass = new Dictionary<RenderPassType, List<IDrawableObjet>>();
        _gameCamera = camera;

        _light = new LightShadersParams()
        {
            Position = new Vector4(0f, 0f, 0f, 1f),
            Direction = new Vector4(0f, -1f, 1f, 1f),
            AmbiantColor = new Vector4(0.2f, 0.2f, 0.2f, 1.0f),
            DiffuseColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
        };

        _shadowMap = new ShadowMap(graphicDeviceRessourceFactory, windowSize, name: "SceneShadowMap");
        _debugDepthMap = new ShadowMap(graphicDeviceRessourceFactory, windowSize, name: "DebugDepthMap");

        float windowWidth = windowSize.Width;
        float windowHeight = windowSize.Height;
        float aspectRatio = windowWidth / windowHeight;
        float planRapproche = 2.0f;
        float planEloigne = 100.0f;

        _cameraLigth = new FreeCamera((float)(Math.PI / 4d))
        {
            FrustrumView = new FrustrumView((float)(Math.PI / 4d), windowWidth, windowHeight, planRapproche, planEloigne, isOrthographic: true)
        };
        _cameraLigth.LookTo(new Vector3(_light.Direction.X, _light.Direction.Y, _light.Direction.Z));
        _cameraLigth.SetPosition(_gameCamera.Position - (DistanceLight * _cameraLigth.Orientation.Forward));

        _disposed = false;
    }

    public void AddObjet(IObjet3D obj)
    {
        // Render
        foreach (RenderPassType renderPass in obj.SupportedRenderPasses)
        {
            if (_objectsDrawablePerRenderPass.TryGetValue(renderPass, out List<IDrawableObjet>? drawableObjects))
            {
                drawableObjects.Add(obj);
            }
            else
            {
                _objectsDrawablePerRenderPass.Add(renderPass, new List<IDrawableObjet>() { obj });
            }
        }

        // Update
        if (obj is IUpdatableObjet updatableObjet)
        {
            _objectsUpdatable.Add(updatableObjet);
        }
    }

    public void Anime(float elapsedTime)
    {
        foreach (IUpdatableObjet obj in _objectsUpdatable)
        {
            obj.Update(elapsedTime);
        }
    }

    public unsafe void Draw(D3D11GraphicPipeline graphicPipeline, ref readonly System.Numerics.Matrix4x4 matViewProj)
    {
        //// Initialiser et sélectionner les « constantes » des shaders
        //ref Vector4 cameraPosParam = ref _shadersParams.CameraPos;
        //ref readonly System.Numerics.Vector3 cameraPos = ref _gameCamera.Position;
        //cameraPosParam.X = cameraPos.X;
        //cameraPosParam.Y = cameraPos.Y;
        //cameraPosParam.Z = cameraPos.Z;

        //_shadersShadowParams.DrawShadow = ShowShadow ? 1 : 0;
        //_shadersDebugParams.IsDebugCameraUsed = UseDebugCam ? 1 : 0;

        //graphicPipeline.RessourceFactory.UpdateSubresource(_constantBuffer, 0, in Unsafe.NullRef<Box>(), in _shadersParams, 0, 0);
        //graphicPipeline.RessourceFactory.UpdateSubresource(_constantShadowBuffer, 0, in Unsafe.NullRef<Box>(), in _shadersShadowParams, 0, 0);
        //graphicPipeline.RessourceFactory.UpdateSubresource(_constantDebugBuffer, 0, in Unsafe.NullRef<Box>(), in _shadersDebugParams, 0, 0);
        //graphicPipeline.VertexShaderStage.SetConstantBuffers(0, 1, ref _constantBuffer);
        //graphicPipeline.VertexShaderStage.SetConstantBuffers(2, 1, ref _constantShadowBuffer);
        //graphicPipeline.VertexShaderStage.SetConstantBuffers(4, 1, ref _constantDebugBuffer);
        //graphicPipeline.PixelShaderStage.SetConstantBuffers(0, 1, ref _constantBuffer);
        //graphicPipeline.PixelShaderStage.SetConstantBuffers(2, 1, ref _constantShadowBuffer);
        //graphicPipeline.VertexShaderStage.SetConstantBuffers(4, 1, ref _constantDebugBuffer);
        //// Shadow Map : Texture + sampler
        //if (_shadowMap.DepthTexture.TextureView.Handle is not null)
        //{
        //    ComPtr<ID3D11ShaderResourceView> shadowMapTexture = _shadowMap.DepthTexture.TextureView;
        //    graphicPipeline.PixelShaderStage.SetShaderResources(2, 1, ref shadowMapTexture);
        //}
        //// Debug Depth Map : Texture + sampler
        //if (_debugDepthMap.DepthTexture.TextureView.Handle is not null)
        //{
        //    ComPtr<ID3D11ShaderResourceView> debugDepthMapTexture = _debugDepthMap.DepthTexture.TextureView;
        //    graphicPipeline.PixelShaderStage.SetShaderResources(3, 1, ref debugDepthMapTexture);
        //}

        //graphicPipeline.PixelShaderStage.SetSamplers(1, 1, in _shadowMap.SampleState);
        //graphicPipeline.PixelShaderStage.SetSamplers(2, 1, in _debugDepthMap.SampleState);
        graphicPipeline.RasterizerStage.SetState(_rasterizerState);

        _cameraLigth.SetPosition(_gameCamera.Position - (DistanceLight * _cameraLigth.Orientation.Forward));
        _cameraLigth.GetViewMatrix(out Matrix4x4 matViewLight);
        ref readonly Matrix4x4 matProjLight = ref _cameraLigth.FrustrumView.MatProj;
        Matrix4x4 matViewProjLight = matViewLight * matProjLight;

        if (ShowDepthTest)
        {
            foreach (IDrawableObjet obj in _objectsDrawablePerRenderPass[RenderPassType.DepthTest])
            {
                obj.Draw(RenderPassType.DepthTest, this, in matViewProj);
            }
        }

        if (ShowShadow)
        {
            //Utiliser la surface de la texture comme surface de rendu
            graphicPipeline.OutputMergerStage.SetRenderTarget(0, in Unsafe.NullRef<ComPtr<ID3D11RenderTargetView>>(), _shadowMap.DepthTexture.TextureDepthStencilView);
            // Effacer le shadow map
            graphicPipeline.GraphicDevice.DeviceContext.ClearDepthStencilView(_shadowMap.DepthTexture.TextureDepthStencilView, (uint)(ClearFlag.Depth | ClearFlag.Stencil), 1.0f, 0);
            // Rasterizer
            graphicPipeline.RasterizerStage.SetState(graphicPipeline.SolidCullFrontRS);
            foreach (IDrawableObjet obj in _objectsDrawablePerRenderPass[RenderPassType.Shadow])
            {
                obj.Draw(RenderPassType.Shadow, this, in matViewProj);
            }
            graphicPipeline.SetRenderTarget(clear: false);
            graphicPipeline.RasterizerStage.SetState(_rasterizerState);
        }

        foreach (IDrawableObjet obj in _objectsDrawablePerRenderPass[RenderPassType.Standart])
        {
            obj.Draw(RenderPassType.Standart, this, in matViewProj);
        }

        //// s'assuré que le texture n'est plus lié comme SRV.
        //if (_shadowMap.DepthTexture.TextureView.Handle is not null)
        //{
        //    graphicPipeline.PixelShaderStage.ClearShaderResources(2);
        //}
        //if (_debugDepthMap.DepthTexture.TextureView.Handle is not null)
        //{
        //    graphicPipeline.PixelShaderStage.ClearShaderResources(3);
        //}
    }

    //public unsafe void DrawShadow(D3D11GraphicPipeline graphicPipeline)
    //{
    //    _cameraLigth.SetPosition(_gameCamera.Position - (DistanceLight * _cameraLigth.Orientation.Forward));
    //    _cameraLigth.GetViewMatrix(out Matrix4x4 matViewLight);
    //    ref readonly Matrix4x4 matProjLight = ref _cameraLigth.FrustrumView.MatProj;
    //    Matrix4x4 matViewProjLight = matViewLight * matProjLight;

    //    // Utiliser la surface de la texture comme surface de rendu
    //    graphicPipeline.OutputMergerStage.SetRenderTarget(0, in Unsafe.NullRef<ComPtr<ID3D11RenderTargetView>>(), _shadowMap.DepthTexture.TextureDepthStencilView);
    //    // Effacer le shadow map
    //    graphicPipeline.GraphicDevice.DeviceContext.ClearDepthStencilView(_shadowMap.DepthTexture.TextureDepthStencilView, (uint)(ClearFlag.Depth | ClearFlag.Stencil), 1.0f, 0);
    //    // Modifier les dimension du viewport
    //    // Set the rasterizer state with the current viewport.
    //    //const int SHADOW_MAP_DIM = 2048;
    //    //Silk.NET.Direct3D11.Viewport viewport = new(0, 0, SHADOW_MAP_DIM, SHADOW_MAP_DIM, 0, 1);
    //    //graphicPipeline.RasterizerStage.SetViewports(1, in viewport);

    //    // input layout des sommets
    //    graphicPipeline.InputAssemblerStage.SetInputLayout(in _shadowMap.VertexLayout);
    //    // Activer le VS
    //    graphicPipeline.VertexShaderStage.SetShader(in _shadowMap.VertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
    //    // Le sampler state
    //    graphicPipeline.PixelShaderStage.SetSamplers(0, 1, in _shadowMap.SampleState);
    //    // Rasterizer
    //    graphicPipeline.RasterizerStage.SetState(graphicPipeline.SolidCullFrontRS);
    //    foreach (IShadowDrawableObjet obj in _objectsWithShadow)
    //    {
    //        obj.DrawShadow(graphicPipeline, in matViewProjLight);
    //    }

    //    // On remet les render targets originales
    //    graphicPipeline.SetRenderTarget(clear: false);
    //    graphicPipeline.ResetViewport();
    //    graphicPipeline.RasterizerStage.SetState(_rasterizerState);
    //}

    //public unsafe void DrawDebugDepth(D3D11GraphicPipeline graphicPipeline)
    //{
    //    //_cameraLigth.SetPosition(_gameCamera.Position - (DistanceLight * _cameraLigth.Orientation.Forward));
    //    //_cameraLigth.GetViewMatrix(out Matrix4x4 matViewLight);
    //    //ref readonly Matrix4x4 matProjLight = ref _cameraLigth.FrustrumView.MatProj;
    //    //Matrix4x4 matViewProj = matViewLight * matProjLight;

    //    _gameCamera.GetViewMatrix(out Matrix4x4 matView);
    //    ref readonly Matrix4x4 matProj = ref _gameCamera.FrustrumView.MatProj;
    //    Matrix4x4 matViewProj = matView * matProj;

    //    // Utiliser la surface de la texture comme surface de rendu
    //    graphicPipeline.OutputMergerStage.SetRenderTarget(0, in Unsafe.NullRef<ComPtr<ID3D11RenderTargetView>>(), _debugDepthMap.DepthTexture.TextureDepthStencilView);
    //    // Effacer le shadow map
    //    graphicPipeline.GraphicDevice.DeviceContext.ClearDepthStencilView(_debugDepthMap.DepthTexture.TextureDepthStencilView, (uint)(ClearFlag.Depth | ClearFlag.Stencil), 1.0f, 0);
    //    // Modifier les dimension du viewport
    //    // Set the rasterizer state with the current viewport.
    //    //const int SHADOW_MAP_DIM = 2048;
    //    //Silk.NET.Direct3D11.Viewport viewport = new(0, 0, SHADOW_MAP_DIM, SHADOW_MAP_DIM, 0, 1);
    //    //graphicPipeline.RasterizerStage.SetViewports(1, in viewport);

    //    // input layout des sommets
    //    graphicPipeline.InputAssemblerStage.SetInputLayout(in _debugDepthMap.VertexLayout);
    //    // Activer le VS
    //    graphicPipeline.VertexShaderStage.SetShader(in _debugDepthMap.VertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
    //    // Le sampler state
    //    graphicPipeline.PixelShaderStage.SetSamplers(0, 1, in _debugDepthMap.SampleState);
    //    // Rasterizer
    //    graphicPipeline.RasterizerStage.SetState(graphicPipeline.SolidCullFrontRS);
    //    foreach (IShadowDrawableObjet obj in _objectsWithShadow)
    //    {
    //        obj.DrawShadow(graphicPipeline, in matViewProj);
    //    }

    //    // On remet les render targets originales
    //    graphicPipeline.SetRenderTarget(clear: false);
    //    graphicPipeline.ResetViewport();
    //    graphicPipeline.RasterizerStage.SetState(_rasterizerState);
    //}

    public void OnScreenResize(Size newSize)
    {
        _shadowMap.Resize(newSize);
        _debugDepthMap.Resize(newSize);
    }

    ~Scene()
    {
        Dispose(disposing: false);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _objectsUpdatable.Clear();
                _objectsDrawablePerRenderPass.Clear();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null

            _disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
