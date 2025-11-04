using System;
using PetitMoteur3D.Core.Math;
using PetitMoteur3D.Input;
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
    public ref readonly Vector3D<float> Move(scoped ref readonly Vector3D<float> move)
    {
        _position.X += move.X;
        _position.Y += move.Y;
        _position.Z += move.Z;
        return ref _position;
    }

    /// <inheritdoc/>
    public void GetViewMatrix(out System.Numerics.Matrix4x4 viewMatrix)
    {
        System.Numerics.Vector3 cameraPosition = _position.ToSystem();
        System.Numerics.Vector3 cameraTarget = _target.Position.ToSystem();
        System.Numerics.Vector3 cameraUpVector = System.Numerics.Vector3.UnitY;
        viewMatrix = Matrix4x4Helper.CreateLookAtLeftHanded(in cameraPosition, in cameraTarget, in cameraUpVector);
    }
}