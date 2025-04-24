using System;
using Silk.NET.Maths;

namespace PetitMoteur3D.Camera
{
    /// <summary>
    /// Implemmentation for a camera fixed on a target object
    /// </summary>
    internal struct TrailingCamera : ICamera
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
        private ISceneObjet _target;

        /// <summary>
        /// Constructeur par defaut
        /// </summary>
        /// <param name="target"></param>
        public TrailingCamera(ISceneObjet target) : this(target, (float)(Math.PI / 4))
        {

        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="target"></param>
        /// <param name="champVision"></param>
        public TrailingCamera(ISceneObjet target, float champVision) : this(target, champVision, Vector3D<float>.Zero)
        {

        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="target"></param>
        /// <param name="champVision"></param>
        /// <param name="position"></param>
        public TrailingCamera(ISceneObjet target, float champVision, Vector3D<float> position)
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
            Vector3D<float> cameraTarget = _target.Position;
            Vector3D<float> cameraUpVector = Vector3D<float>.UnitY;
            return CameraHelper.CreateLookAtLH(Position, cameraTarget, cameraUpVector);
        }
    }
}