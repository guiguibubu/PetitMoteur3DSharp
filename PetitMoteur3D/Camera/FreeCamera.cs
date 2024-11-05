using System;
using Silk.NET.Maths;

namespace PetitMoteur3D.Camera
{
    internal struct FreeCamera : ICamera, IRotationObjet
    {
        /// <summary>
        /// Champ vision
        /// </summary>
        public float ChampVision { get; init; }

        /// <inheritdoc/>
        public Vector3D<float> Position { get; private set; }

        /// <summary>
        /// Rotation de la vue de la caméra par rapport à l'axe +Z
        /// <summary>
        public Vector3D<float> Rotation { get; private set; }

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
        public FreeCamera(float champVision, Vector3D<float> position, Vector3D<float> rotation)
        {
            ChampVision = champVision;
            Position = position;
            Rotation = rotation;
        }

        /// <inheritdoc/>
        public Vector3D<float> Move(Vector3D<float> move)
        {
            Position += move;
            return Position;
        }

        /// <inheritdoc/>
        public Vector3D<float> Rotate(Vector3D<float> rotation)
        {
            Rotation += rotation;
            return Rotation;
        }

        public Matrix4X4<float> GetViewMatrix()
        {
            Vector3D<float> cameraDirection = Rotation * Vector3D<float>.UnitZ;
            Vector3D<float> cameraTarget = Position + cameraDirection;
            Vector3D<float> cameraUpVector = Rotation * Vector3D<float>.UnitY;
            return CameraHelper.CreateLookAtLH(Position, cameraTarget, cameraUpVector);
        }
    }
}