﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PetitMoteur3D.Camera;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace PetitMoteur3D
{
    internal class Scene
    {
        private readonly List<ISceneObjet> _objects;
        private readonly List<IObjet3D> _objects3D;
        private ICamera _camera;

        private ComPtr<ID3D11Buffer> _constantBuffer = default;

        private LightShadersParams _light;
        private SceneShadersParams _shadersParams;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public Scene(GraphicBufferFactory bufferFactory) : this(bufferFactory, new FixedCamera(Vector3D<float>.Zero))
        { }

        public Scene(GraphicBufferFactory bufferFactory, ICamera camera)
        {
            _objects = new List<ISceneObjet>();
            _objects3D = new List<IObjet3D>();
            _camera = camera;
            _objects.Add(camera);

            _light = new LightShadersParams()
            {
                Position = new Vector4D<float>(0f, 0f, 0f, 1f),
                Direction = new Vector4D<float>(1f, 0f, 1f, 1f),
                AmbiantColor = new Vector4D<float>(0.2f, 0.2f, 0.2f, 1.0f),
                DiffuseColor = new Vector4D<float>(1.0f, 1.0f, 1.0f, 1.0f),
            };

            _shadersParams = new SceneShadersParams()
            {
                LightParams = _light,
                CameraPos = new Vector4D<float>(_camera.Position, 1.0f),
            };

            // Create our constant buffer.
            _constantBuffer = bufferFactory.CreateConstantBuffer<SceneShadersParams>(Usage.Default, CpuAccessFlag.None, name: "SceneConstantBuffer");
        }

        ~Scene()
        {
            _constantBuffer.Dispose();
        }

        public void AddObjet(IObjet3D obj)
        {
            _objects.Add(obj);
            _objects3D.Add(obj);
        }

        public void Anime(float elapsedTime)
        {
            foreach (IObjet3D obj in _objects3D)
            {
                obj.Anime(elapsedTime);
            }
        }

        public void Draw(ref readonly ComPtr<ID3D11DeviceContext> deviceContext, ref readonly Matrix4X4<float> matViewProj)
        {
            // Initialiser et sélectionner les « constantes » des shaders
            ref Vector4D<float> cameraPosParam = ref _shadersParams.CameraPos;
            ref readonly Vector3D<float> cameraPos = ref _camera.Position;
            cameraPosParam.X = cameraPos.X;
            cameraPosParam.Y = cameraPos.Y;
            cameraPosParam.Z = cameraPos.Z;

            deviceContext.UpdateSubresource(_constantBuffer, 0, ref Unsafe.NullRef<Box>(), in _shadersParams, 0, 0);
            deviceContext.VSSetConstantBuffers(0, 1, ref _constantBuffer);
            deviceContext.PSSetConstantBuffers(0, 1, ref _constantBuffer);
            foreach (IObjet3D obj in _objects3D)
            {
                obj.Draw(in deviceContext, in matViewProj);
            }
        }
    }
}
