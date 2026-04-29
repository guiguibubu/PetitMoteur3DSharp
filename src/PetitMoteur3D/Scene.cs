using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using PetitMoteur3D.Camera;
using PetitMoteur3D.Graphics;

namespace PetitMoteur3D;

internal sealed class Scene : IVisitable<Scene>, IDisposable
{
    public bool ShowDepthTest { get; set; }
    public bool UseDebugCam { get; set; }
    public ICamera GameCamera => _gameCamera;
    public LightShadersParams Light => _light;
    public SceneNode<IObjet3D> RootNode => _rootNode;

    private readonly SceneNode<IObjet3D> _rootNode;
    private readonly List<IUpdatableObjet> _objectsUpdatable;
    private ICamera _gameCamera;
    private ICamera _debugCamera;

    private LightShadersParams _light;

    private readonly FreeCamera _cameraLigth;

    private const int DistanceLight = 50;
    private bool _disposed;

    /// <summary>
    /// Constructeur par défaut
    /// </summary>
    public Scene(Size windowSize) : this(new FixedCamera(Vector3.Zero), windowSize)
    { }

    public Scene(ICamera camera, Size windowSize)
    {
        _rootNode = new SceneNode<IObjet3D>();

        _objectsUpdatable = new List<IUpdatableObjet>();
        _gameCamera = camera;

        _light = new LightShadersParams()
        {
            Position = new Vector4(0f, 0f, 0f, 1f),
            Direction = new Vector4(1f, -1f, 1f, 1f),
            AmbiantColor = new Vector4(0.2f, 0.2f, 0.2f, 1.0f),
            DiffuseColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
        };

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
        // Update
        if (obj is IUpdatableObjet updatableObjet)
        {
            _objectsUpdatable.Add(updatableObjet);
        }

        _rootNode.AddMesh(obj);
    }

    public void AddChildren(SceneNode<IObjet3D> node)
    {
        // Update
        IUpdatableObjet[] updatableObjet = node.GetObject<IUpdatableObjet>();
        _objectsUpdatable.AddRange(updatableObjet);

        _rootNode.AddChild(node);
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

    public void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
        _rootNode.Accept(visitor);
    }

    public void Accept(IVisitor<Scene> visitor)
    {
        visitor.Visit(this);
        if (visitor is IVisitor<SceneNode<IObjet3D>> visitorNode)
        {
            _rootNode.Accept(visitorNode);
        }
    }

    public SceneViewContext GetSceneViewContext()
    {
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

        return new SceneViewContext()
        {
            MatViewProj = matViewProj,
            MatViewProjLight = matViewProjLight,
            Light = _light,
            GameCameraPos = new Vector4(_gameCamera.Position, 1.0f),
        };
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
