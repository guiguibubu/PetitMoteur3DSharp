using System;
using Silk.NET.Maths;

namespace PetitMoteur3D.Camera
{
    internal struct FixedCamera : ICamera
    {
        /// <summary>
        /// Champ vision
        /// </summary>
        public float ChampVision { get; init; }

        /// <inheritdoc/>
        public Vector3D<float> Position { get; private set; }

        /// <summary>
        /// The target of the camera.
        /// <summary>
        private Vector3D<float> _target;

        /// <summary>
        /// Constructeur par defaut
        /// </summary>
        /// <param name="target"></param>
        public FixedCamera(Vector3D<float> target) : this(target, (float)(Math.PI / 4))
        {

        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="target"></param>
        /// <param name="champVision"></param>
        public FixedCamera(Vector3D<float> target, float champVision) : this(target, champVision, Vector3D<float>.Zero)
        {

        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="target"></param>
        /// <param name="champVision"></param>
        /// <param name="position"></param>
        public FixedCamera(Vector3D<float> target, float champVision, Vector3D<float> position)
        {
            _target = target;
            ChampVision = champVision;
            Position = position;
        }

        /// <inheritdoc/>
        public Vector3D<float> Move(Vector3D<float> move)
        {
            Position += move;
            return Position;
        }

        public Matrix4X4<float> GetViewMatrix()
        {
            Vector3D<float> cameraTarget = _target;
            Vector3D<float> cameraUpVector = Vector3D<float>.UnitY;
            return CameraHelper.CreateLookAtLH(Position, cameraTarget, cameraUpVector);
        }
    }
}