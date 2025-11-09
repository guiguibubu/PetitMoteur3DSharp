using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PetitMoteur3D.Camera;
using PetitMoteur3D.Graphics;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace PetitMoteur3D;

internal sealed class Scene : IDrawableObjet, IDisposable
{
    private readonly List<IObjet3D> _objects3D;
    private ICamera _camera;

    private ComPtr<ID3D11Buffer> _constantBuffer;

    private LightShadersParams _light;
    private SceneShadersParams _shadersParams;

    private bool _disposed;

    /// <summary>
    /// Constructeur par défaut
    /// </summary>
    public Scene(GraphicBufferFactory bufferFactory) : this(bufferFactory, new FixedCamera(System.Numerics.Vector3.Zero))
    { }

    public Scene(GraphicBufferFactory bufferFactory, ICamera camera)
    {
        _objects3D = new List<IObjet3D>();
        _camera = camera;

        _light = new LightShadersParams()
        {
            Position = new Vector4D<float>(0f, 0f, 0f, 1f),
            Direction = new Vector4D<float>(1f, -1f, 1f, 1f),
            AmbiantColor = new Vector4D<float>(0.2f, 0.2f, 0.2f, 1.0f),
            DiffuseColor = new Vector4D<float>(1.0f, 1.0f, 1.0f, 1.0f),
        };

        _shadersParams = new SceneShadersParams()
        {
            LightParams = _light,
            CameraPos = new Vector4D<float>(_camera.Position.ToGeneric(), 1.0f),
        };

        // Create our constant buffer.
        _constantBuffer = bufferFactory.CreateConstantBuffer<SceneShadersParams>(Usage.Default, CpuAccessFlag.None, name: "SceneConstantBuffer");
        _disposed = false;
    }

    public void AddObjet(IObjet3D obj)
    {
        _objects3D.Add(obj);
    }

    public void Anime(float elapsedTime)
    {
        foreach (IObjet3D obj in _objects3D)
        {
            obj.Update(elapsedTime);
        }
        _camera.Update(elapsedTime);
    }

    public void Draw(D3D11GraphicPipeline graphicPipeline, ref readonly System.Numerics.Matrix4x4 matViewProj)
    {
        // Initialiser et sélectionner les « constantes » des shaders
        ref Vector4D<float> cameraPosParam = ref _shadersParams.CameraPos;
        ref readonly System.Numerics.Vector3 cameraPos = ref _camera.Position;
        cameraPosParam.X = cameraPos.X;
        cameraPosParam.Y = cameraPos.Y;
        cameraPosParam.Z = cameraPos.Z;

        graphicPipeline.RessourceFactory.UpdateSubresource(_constantBuffer, 0, in Unsafe.NullRef<Box>(), in _shadersParams, 0, 0);
        graphicPipeline.VertexShaderStage.SetConstantBuffers(0, 1, ref _constantBuffer);
        graphicPipeline.PixelShaderStage.SetConstantBuffers(0, 1, ref _constantBuffer);
        foreach (IObjet3D obj in _objects3D)
        {
            obj.Draw(graphicPipeline, in matViewProj);
        }
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
                _objects3D.Clear();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _constantBuffer.Dispose();

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
