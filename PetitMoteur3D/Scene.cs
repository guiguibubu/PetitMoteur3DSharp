using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public Scene(ComPtr<ID3D11Device> device) : this(device, new FixedCamera(Vector3D<float>.Zero))
        { }

        public Scene(ComPtr<ID3D11Device> device, ICamera camera)
        {
            _objects = new List<ISceneObjet>();
            _objects3D = new List<IObjet3D>();
            _camera = camera;
            _objects.Add(camera);

            // Create our constant buffer.
            CreateConstantBuffer<SceneShadersParams>(device, ref _constantBuffer);
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

        public void Draw(ComPtr<ID3D11DeviceContext> deviceContext, Matrix4X4<float> matViewProj)
        {
            // Initialiser et sélectionner les « constantes » des shaders
            LightShadersParams lightParams = new()
            {
                Position = new Vector4D<float>(0f, 0f, 0f, 1f),
                Direction = new Vector4D<float>(1f, 0f, 1f, 1f),
                AmbiantColor = new Vector4D<float>(0.2f, 0.2f, 0.2f, 1.0f),
                DiffuseColor = new Vector4D<float>(1.0f, 1.0f, 1.0f, 1.0f),
            };

            SceneShadersParams shadersParams = new()
            {
                LightParams = lightParams,
                CameraPos = new Vector4D<float>(_camera.Position, 1.0f),
            };
            deviceContext.UpdateSubresource(_constantBuffer, 0, ref Unsafe.NullRef<Box>(), ref shadersParams, 0, 0);
            deviceContext.VSSetConstantBuffers(0, 1, ref _constantBuffer);
            deviceContext.PSSetConstantBuffers(0, 1, ref _constantBuffer);
            foreach (IObjet3D obj in _objects3D)
            {
                obj.Draw(deviceContext, matViewProj);
            }
        }

        private static unsafe void CreateConstantBuffer<T>(ComPtr<ID3D11Device> device, ref ComPtr<ID3D11Buffer> buffer) where T : unmanaged
        {
            BufferDesc bufferDesc = new()
            {
                ByteWidth = (uint)(Marshal.SizeOf<T>()),
                Usage = Usage.Default,
                BindFlags = (uint)BindFlag.ConstantBuffer,
                CPUAccessFlags = 0
            };

            SilkMarshal.ThrowHResult(device.CreateBuffer(in bufferDesc, in Unsafe.NullRef<SubresourceData>(), ref buffer));
        }
    }
}
