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
    public SceneRenderingType RenderingType { get; set; }
    public ICamera GameCamera => _gameCamera;
    public LightShadersParams Light => _light;
    public ComPtr<ID3D11RasterizerState> RasterizerState { get { return _rasterizerState; } set { _rasterizerState = value; } }

    private readonly List<IUpdatableObjet> _objectsUpdatable;
    private readonly Dictionary<RenderPassType, List<IDrawableObjet>> _objectsDrawablePerRenderPass;
    private ICamera _gameCamera;
    private ICamera _debugCamera;

    private ComPtr<ID3D11RasterizerState> _rasterizerState;

    private LightShadersParams _light;

    private readonly ShadowMap _shadowMap;
    private readonly ShadowMap _debugDepthMap;
    private readonly FreeCamera _cameraLigth;

    private readonly ScreenQuad _fullScreenQuad;

    private const int DistanceLight = 50;
    private bool _disposed;

    /// <summary>
    /// Constructeur par défaut
    /// </summary>
    public Scene(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, RenderPassFactory renderPassFactory, Size windowSize) : this(graphicDeviceRessourceFactory, renderPassFactory, new FixedCamera(Vector3.Zero), windowSize)
    { }

    public Scene(GraphicDeviceRessourceFactory graphicDeviceRessourceFactory, RenderPassFactory renderPassFactory, ICamera camera, Size windowSize)
    {
        _objectsUpdatable = new List<IUpdatableObjet>();
        _objectsDrawablePerRenderPass = new Dictionary<RenderPassType, List<IDrawableObjet>>();
        _gameCamera = camera;

        _light = new LightShadersParams()
        {
            Position = new Vector4(0f, 0f, 0f, 1f),
            Direction = new Vector4(1f, -1f, 1f, 1f),
            AmbiantColor = new Vector4(0.2f, 0.2f, 0.2f, 1.0f),
            DiffuseColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
        };

        _shadowMap = new ShadowMap(graphicDeviceRessourceFactory, windowSize, name: "SceneShadowMap");
        _debugDepthMap = new ShadowMap(graphicDeviceRessourceFactory, windowSize, name: "DebugDepthMap");

        _fullScreenQuad = new ScreenQuad(-1, 1, -1, 1, 1, graphicDeviceRessourceFactory, renderPassFactory);
        _fullScreenQuad.SupportedRenderPasses = [RenderPassType.DeferredShadingLightning];
        AddObjet(_fullScreenQuad);

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

    public void SetDebugCamera(ICamera camera)
    {
        _debugCamera = camera;
    }

    public unsafe void Draw(D3D11GraphicPipeline graphicPipeline)
    {
        graphicPipeline.RasterizerStage.SetState(_rasterizerState);

        Matrix4x4 matViewProj;
        if (UseDebugCam)
        {
            ref readonly Matrix4x4 matProj = ref _debugCamera.FrustrumView.MatProj;
            _debugCamera.GetViewMatrix(out Matrix4x4 matView);
            matViewProj = matView * matProj;
        }
        else
        {
            ref readonly Matrix4x4 matProj = ref _gameCamera.FrustrumView.MatProj;
            _gameCamera.GetViewMatrix(out Matrix4x4 matView);
            matViewProj = matView * matProj;
        }

        _cameraLigth.SetPosition(_gameCamera.Position - (DistanceLight * _cameraLigth.Orientation.Forward));
        _cameraLigth.GetViewMatrix(out Matrix4x4 matViewLight);
        ref readonly Matrix4x4 matProjLight = ref _cameraLigth.FrustrumView.MatProj;
        Matrix4x4 matViewProjLight = matViewLight * matProjLight;

        SceneViewContext sceneContext = new()
        {
            MatViewProj = matViewProj,
            MatViewProjLight = matViewProjLight,
            Light = _light,
            GameCameraPos = new Vector4(_gameCamera.Position, 1.0f),
            ShowShadow = ShowShadow,
            ShadowMap = _shadowMap,
        };

        if (ShowDepthTest)
        {
            Draw(RenderPassType.DepthTest, sceneContext);
        }

        if (RenderingType == SceneRenderingType.Forward)
        {
            if (ShowShadow)
            {
                //Utiliser la surface de la texture comme surface de rendu
                graphicPipeline.SetRenderTarget(RenderTargetType.NoRenderTarget);
                graphicPipeline.OutputMergerStage.SetRenderTarget(0, in Unsafe.NullRef<ComPtr<ID3D11RenderTargetView>>(), _shadowMap.DepthTexture.TextureDepthStencilView);
                // Effacer le shadow map
                graphicPipeline.GraphicDevice.DeviceContext.ClearDepthStencilView(_shadowMap.DepthTexture.TextureDepthStencilView, (uint)(ClearFlag.Depth | ClearFlag.Stencil), 1.0f, 0);
                // Rasterizer
                graphicPipeline.RasterizerStage.SetState(graphicPipeline.SolidCullFrontRS);

                Draw(RenderPassType.ShadowMap, sceneContext);

                graphicPipeline.RasterizerStage.SetState(_rasterizerState);
            }
            graphicPipeline.SetRenderTarget(RenderTargetType.BackBuffer, clear: true);
            Draw(RenderPassType.ForwardOpac, sceneContext);
        }
        else if (RenderingType == SceneRenderingType.DeferredShading)
        {
            graphicPipeline.SetRenderTarget(RenderTargetType.GeometryBuffers, clear: false);
            Draw(RenderPassType.DeferredShadingGeometry, sceneContext);
            graphicPipeline.SetRenderTarget(RenderTargetType.BackBuffer, clear: false);
            graphicPipeline.OutputMergerStage.SetDepthStencilState(graphicPipeline.ReadonlyGreaterDSS);
            sceneContext.MatViewProj = Matrix4x4.CreateOrthographicOffCenterLeftHanded(-1, 1, -1, 1, 0, 1);
            Draw(RenderPassType.DeferredShadingLightning, sceneContext);
            graphicPipeline.OutputMergerStage.SetDefaultDepthStencilState();
        }
    }

    private void Draw(RenderPassType renderPassType, SceneViewContext sceneContext)
    {
        foreach (IDrawableObjet obj in _objectsDrawablePerRenderPass[renderPassType])
        {
            obj.Draw(renderPassType, sceneContext);
        }
    }

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
