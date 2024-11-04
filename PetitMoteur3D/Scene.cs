using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.Maths;

namespace PetitMoteur3D
{
    internal class Scene
    {
        private readonly List<IObjet3D> _objects;

        private ComPtr<ID3D11Buffer> _constantBuffer = default;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public Scene(ComPtr<ID3D11Device> device) : this(device, Array.Empty<IObjet3D>()) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public Scene(ComPtr<ID3D11Device> device, params IObjet3D[] obj) : this(device, obj.AsEnumerable()) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objets"></param>
        public Scene(ComPtr<ID3D11Device> device, IEnumerable<IObjet3D> objets)
        {
            _objects = new List<IObjet3D>(objets);
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
        }

        public void Anime(float elapsedTime)
        {
            foreach (IObjet3D obj in _objects)
            {
                obj.Anime(elapsedTime);
            }
        }

        public void Draw(ComPtr<ID3D11DeviceContext> deviceContext, Matrix4X4<float> matViewProj)
        {
            // Initialiser et sélectionner les « constantes » des shaders
            SceneShadersParams shadersParams = new()
            {
                lightPos = new Vector4D<float>(0f, 0f, 0f, 1f),
                cameraPos = new Vector4D<float>(0.0f, 0.0f, -10.0f, 1.0f),
                ambiantLightValue = new Vector4D<float>(0.2f, 0.2f, 0.2f, 1.0f),
                diffuseLightValue = new Vector4D<float>(1.0f, 1.0f, 1.0f, 1.0f),
            };
            deviceContext.UpdateSubresource(_constantBuffer, 0, ref Unsafe.NullRef<Box>(), ref shadersParams, 0, 0);
            deviceContext.VSSetConstantBuffers(0, 1, ref _constantBuffer);
            deviceContext.PSSetConstantBuffers(0, 1, ref _constantBuffer);
            foreach (IObjet3D obj in _objects)
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
