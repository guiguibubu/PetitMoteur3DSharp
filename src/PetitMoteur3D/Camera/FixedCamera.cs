using System;
using System.Numerics;
using PetitMoteur3D.Core.Math;
using PetitMoteur3D.Input;

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
    public ref readonly Vector3 Position => ref _position;

    private Vector3 _position;

    /// <summary>
    /// The target of the camera.
    /// <summary>
    private Vector3 _target;

    private Orientation3D _orientation;

    private IInputContext? _inputContext;

    /// <summary>
    /// Constructeur par defaut
    /// </summary>
    /// <param name="target"></param>
    public FixedCamera(ref readonly Vector3 target) : this(in target, (float)(Math.PI / 4))
    {

    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="target"></param>
    /// <param name="champVision"></param>
    public FixedCamera(ref readonly Vector3 target, float champVision) : this(in target, champVision, Vector3.Zero)
    {

    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="target"></param>
    /// <param name="champVision"></param>
    /// <param name="position"></param>
    public FixedCamera(ref readonly Vector3 target, float champVision, Vector3 position)
        : this(in target, champVision, in position)
    { }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="target"></param>
    /// <param name="champVision"></param>
    /// <param name="position"></param>
    public FixedCamera(ref readonly Vector3 target, float champVision, ref readonly Vector3 position)
    {
        _target = target;
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

        Vector3 direction = _target - _position;
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


        Vector3 move = Vector3.Zero;

        if (avantArriere != 0)
        {
            move += (avantArriere * _orientation.Forward);
        }

        if (gaucheDroite != 0)
        {
            move += (gaucheDroite * _orientation.Rigth);
        }

        Move(in move);
    }

    /// <inheritdoc/>
    public void InitInput(IInputContext? inputContext)
    {
        _inputContext = inputContext;
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
        ref readonly Vector3 cameraTarget = ref _target;
        ref readonly Vector3 cameraUpVector = ref _orientation.Up;
        Vector3 cameraPosition = _position;
        viewMatrix = Matrix4x4Helper.CreateLookAtLeftHanded(in cameraPosition, in cameraTarget, in cameraUpVector);
    }
}