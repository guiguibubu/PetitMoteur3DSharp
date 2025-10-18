using System;
using Silk.NET.Maths;

namespace PetitMoteur3D.Camera
{
    internal class FreeCamera : ICamera, IRotationObjet
    {
        /// <summary>
        /// Champ vision
        /// </summary>
        public float ChampVision { get; init; }

        /// <inheritdoc/>
        public ref readonly Vector3D<float> Position => ref _position;

        /// <summary>
        /// Rotation de la vue de la caméra par rapport à l'axe +Z
        /// <summary>
        public ref readonly Vector3D<float> Rotation => ref _rotation;

        private Vector3D<float> _position;
        private Vector3D<float> _rotation;

        /// <summary>
        /// Constructeur par defaut
        /// </summary>
        public FreeCamera() : this((float)(Math.PI / 4))
        {

        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="champVision"></param>
        public FreeCamera(float champVision) : this(champVision, Vector3D<float>.Zero, Vector3D<float>.Zero)
        {

        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public FreeCamera(float champVision, ref readonly Vector3D<float> position, ref readonly Vector3D<float> rotation)
        {
            ChampVision = champVision;
            _position = position;
            _rotation = rotation;
        }

        /// <inheritdoc/>
        public ref readonly Vector3D<float> Move(ref readonly Vector3D<float> move)
        {
            _position.X += move.X;
            _position.Y += move.Y;
            _position.Z += move.Z;
            return ref _position;
        }

        /// <inheritdoc/>
        public ref readonly Vector3D<float> Rotate(ref readonly Vector3D<float> rotation)
        {
            _rotation.X += rotation.X;
            _rotation.Y += rotation.Y;
            _rotation.Z += rotation.Z;
            return ref _rotation;
        }

        public void GetViewMatrix(out Matrix4X4<float> viewMatrix)
        {
            Vector3D<float> cameraDirection = _rotation * Vector3D<float>.UnitZ;
            Vector3D<float> cameraTarget = _position + cameraDirection;
            Vector3D<float> cameraUpVector = _rotation * Vector3D<float>.UnitY;
            viewMatrix = CameraHelper.CreateLookAtLH(in _position, in cameraTarget, in cameraUpVector);
        }
    }
}