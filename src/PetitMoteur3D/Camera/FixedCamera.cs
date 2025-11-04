using System;
using PetitMoteur3D.Core.Math;
using PetitMoteur3D.Input;
using Silk.NET.Maths;

namespace PetitMoteur3D.Camera;

/// <summary>
/// Implemmentation for a camera fixed on a target position and keep same distance to it
/// </summary>
internal class FixedCamera : ICamera
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
    private System.Numerics.Vector3 _target;

    private Orientation3D _orientation;

    private IInputContext? _inputContext;

    /// <summary>
    /// Constructeur par defaut
    /// </summary>
    /// <param name="target"></param>
    public FixedCamera(ref readonly Vector3D<float> target) : this(in target, (float)(Math.PI / 4))
    {

    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="target"></param>
    /// <param name="champVision"></param>
    public FixedCamera(ref readonly Vector3D<float> target, float champVision) : this(in target, champVision, Vector3D<float>.Zero)
    {

    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="target"></param>
    /// <param name="champVision"></param>
    /// <param name="position"></param>
    public FixedCamera(ref readonly Vector3D<float> target, float champVision, Vector3D<float> position)
        : this(in target, champVision, in position)
    { }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="target"></param>
    /// <param name="champVision"></param>
    /// <param name="position"></param>
    public FixedCamera(ref readonly Vector3D<float> target, float champVision, ref readonly Vector3D<float> position)
    {
        _target = target.ToSystem();
        ChampVision = champVision;
        _position = position;
        _orientation = new Orientation3D();
        _inputContext = default;
    }

    /// <inheritdoc/>
    public void Update(float elapsedTime)
    {
        if (_inputContext is null)
        {
            return;
        }

        System.Numerics.Vector3 direction = _target - _position.ToSystem();
        _orientation.LookTo(in direction);

        IKeyboard keyboard = _inputContext.Keyboards[0];
        // WASD (W is up, A is left, S is down, and D is right)
        float avantArriere = 0f;
        float gaucheDroite = 0f;
        if (keyboard.IsKeyPressed(Key.W) && direction.LengthSquared() > Math.Pow(float.Epsilon, 2))
        {
            avantArriere += 1f;
        }
        if (keyboard.IsKeyPressed(Key.S))
        {
            avantArriere -= 1f;
        }
        if (keyboard.IsKeyPressed(Key.D))
        {
            gaucheDroite += 1f;
        }
        if (keyboard.IsKeyPressed(Key.A))
        {
            gaucheDroite -= 1f;
        }


        Vector3D<float> move = Vector3D<float>.Zero;

        if (avantArriere != 0)
        {
            move += (avantArriere * _orientation.Forward).ToGeneric();
        }

        if (gaucheDroite != 0)
        {
            move += (gaucheDroite * _orientation.Rigth).ToGeneric();
        }

        Move(in move);
    }

    /// <inheritdoc/>
    public void InitInput(IInputContext? inputContext)
    {
        _inputContext = inputContext;
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
        ref readonly System.Numerics.Vector3 cameraTarget = ref _target;
        ref readonly System.Numerics.Vector3 cameraUpVector = ref _orientation.Up;
        System.Numerics.Vector3 cameraPosition = _position.ToSystem();
        viewMatrix = Matrix4x4Helper.CreateLookAtLeftHanded(in cameraPosition, in cameraTarget, in cameraUpVector);
    }
}