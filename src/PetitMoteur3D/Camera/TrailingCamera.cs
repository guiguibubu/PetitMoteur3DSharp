using System;
using System.Numerics;
using PetitMoteur3D.Core.Math;
using PetitMoteur3D.Input;

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
    public ref readonly Vector3 Position => ref _position;

    private Vector3 _position;

    private static readonly Vector3 CameraUpVector = Vector3.UnitY;

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
    public TrailingCamera(ISceneObjet target, float champVision) : this(target, champVision, Vector3.Zero)
    {
    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="target"></param>
    /// <param name="champVision"></param>
    /// <param name="position"></param>
    public TrailingCamera(ISceneObjet target, float champVision, ref readonly Vector3 position)
    {
        _target = target;
        ChampVision = champVision;
        _position = position;
    }

    /// <inheritdoc/>
    public virtual void Update(float elapsedTime)
    {
        //TODO: Handle input
    }

    /// <inheritdoc/>
    public void InitInput(IInputContext? inputContext)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public ref readonly Vector3 Move(float dx, float dy, float dz)
    {
        _position.X += dx;
        _position.Y += dy;
        _position.Z += dz;
        return ref _position;
    }

    /// <inheritdoc/>
    public ref readonly Vector3 Move(scoped ref readonly Vector3 move)
    {
        return ref Move(move.X, move.Y, move.Z);
    }

    /// <inheritdoc/>
    public void GetViewMatrix(out Matrix4x4 viewMatrix)
    {
        ref readonly Vector3 cameraPosition = ref _position;
        ref readonly Vector3 cameraTarget = ref _target.Position;
        ref readonly Vector3 cameraUpVector = ref CameraUpVector;
        viewMatrix = Matrix4x4Helper.CreateLookAtLeftHanded(in cameraPosition, in cameraTarget, in cameraUpVector);
    }
}