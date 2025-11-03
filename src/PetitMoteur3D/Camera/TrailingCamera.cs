using System;
using Silk.NET.Maths;

namespace PetitMoteur3D.Camera;

/// <summary>
/// Implemmentation for a camera fixed on a target object
/// </summary>
internal class TrailingCamera : ICamera
{
    /// <summary>
    /// Champ vision
    /// </summary>
    public float ChampVision { get; init; }

    /// <inheritdoc/>
    public ref readonly Vector3D<float> Position => ref _position;

    private Vector3D<float> _position;

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
    public TrailingCamera(ISceneObjet target, float champVision, ref readonly Vector3D<float> position)
    {
        _target = target;
        ChampVision = champVision;
        _position = position;
    }

    /// <inheritdoc/>
    public ref readonly Vector3D<float> Move(ref readonly Vector3D<float> move)
    {
        _position.X += move.X;
        _position.Y += move.Y;
        _position.Z += move.Z;
        return ref _position;
    }

    public void GetViewMatrix(out Matrix4X4<float> viewMatrix)
    {
        Vector3D<float> cameraTarget = _target.Position;
        Vector3D<float> cameraUpVector = Vector3D<float>.UnitY;
        viewMatrix = CameraHelper.CreateLookAtLH(in _position, in cameraTarget, in cameraUpVector);
    }
}