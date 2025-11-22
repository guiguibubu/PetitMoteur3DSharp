using System;
using System.Drawing;
using System.Numerics;
using PetitMoteur3D.Core.Math;
using PetitMoteur3D.Input;
using PetitMoteur3D.Window;

namespace PetitMoteur3D.Camera;

internal sealed class FreeCamera : ICamera, IRotationObjet
{
    /// <summary>
    /// Champ vision
    /// </summary>
    public float ChampVision { get; init; }

    /// <inheritdoc/>
    public ref readonly Vector3 Position => ref _position;

    public Orientation3D Orientation => _orientation;

    private Vector3 _position;

    private readonly IWindow? _window;
    private readonly Orientation3D _orientation;

    private IInputContext? _inputContext;

    private const float WindowCenterOffset = 1f / 8;

    private static readonly Vector3 UpRotationAxis = Vector3.UnitY;
    private static readonly Vector3 ZeroRotation = Vector3.Zero;

    /// <summary>
    /// Constructeur par defaut
    /// </summary>
    public FreeCamera() : this(null, (float)(Math.PI / 4))
    {

    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="champVision"></param>
    public FreeCamera(float champVision) : this(null, champVision)
    {


    }
    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="window"></param>
    public FreeCamera(IWindow? window) : this(window, (float)(Math.PI / 4))
    {

    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="champVision"></param>
    public FreeCamera(IWindow? window, float champVision) : this(window, champVision, Vector3.Zero)
    {

    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="position"></param>
    /// <param name="champVision"></param>
    /// <param name="position"></param>
    public FreeCamera(IWindow? window, float champVision, Vector3 position)
    {
        _window = window;
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

        IKeyboard keyboard = _inputContext.Keyboards[0];
        // WASD (W is up, A is left, S is down, and D is right)
        float avantArriere = 0f;
        float gaucheDroite = 0f;
        if (keyboard.IsKeyPressed(Key.W))
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

        IMouse mouse = _inputContext.Mice[0];
        Size? windowsSize = _window?.Size;

        Vector2 positionMouse = mouse.Position;

        bool intoWindow = windowsSize is null
            || (positionMouse.X >= 0 
            && positionMouse.X <= windowsSize.Value.Width
            && positionMouse.Y >= 0
            && positionMouse.Y <= windowsSize.Value.Height
            );

        if (!intoWindow)
        {
            return;
        }

        // Normalize mouse position into [-1, 1] in window space
        Vector2 positionNormalized = ((mouse.Position / new Vector2(windowsSize?.Width ?? 1, windowsSize?.Height ?? 1)) - new Vector2(0.5f)) * 2;

        bool isInCenter = Math.Abs(positionNormalized.X) <= WindowCenterOffset && Math.Abs(positionNormalized.Y) <= WindowCenterOffset;
        if (!isInCenter)
        {
            float rotGaucheDroite = positionNormalized.X;
            float rotHautBas = positionNormalized.Y;

            _orientation.Rotate(in UpRotationAxis, (float)(rotGaucheDroite * (Math.PI / 128f)));
            _orientation.Rotate(in _orientation.Rigth, (float)(rotHautBas * (Math.PI / 128f)));
        }
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
    public ref readonly Vector3 Move(Vector3 move)
    {
        return ref Move(in move);
    }

    /// <inheritdoc/>
    public ref readonly Vector3 Move(scoped ref readonly Vector3 move)
    {
        return ref Move(move.X, move.Y, move.Z);
    }

    /// <inheritdoc/>
    public ref readonly Vector3 SetPosition(float x, float y, float z)
    {
        _position.X = x;
        _position.Y = y;
        _position.Z = z;
        return ref _position;
    }

    /// <inheritdoc/>
    public ref readonly Vector3 SetPosition(Vector3 move)
    {
        return ref SetPosition(in move);
    }

    /// <inheritdoc/>
    public ref readonly Vector3 SetPosition(scoped ref readonly Vector3 move)
    {
        return ref SetPosition(move.X, move.Y, move.Z);
    }

    /// <inheritdoc/>
    /// <remarks>Currently only return zero vector</remarks>
    public ref readonly Vector3 RotateEuler(ref readonly Vector3 rotation)
    {
        Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        _orientation.Rotate(in quaternion);
        return ref ZeroRotation;
    }


    /// <inheritdoc/>
    /// <remarks>Currently only return zero vector</remarks>
    public ref readonly Vector3 Rotate(ref readonly Vector3 axis, float angle)
    {
        _orientation.Rotate(in axis, angle);
        return ref ZeroRotation;
    }

    /// <summary>
    /// Set rotation
    /// </summary>
    /// <remarks>Currently only return zero vector</remarks>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public ref readonly Vector3 SetRotationEuler(ref readonly Vector3 rotation)
    {
        Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        _orientation.SetRotation(in quaternion);
        return ref ZeroRotation;
    }


    /// <summary>
    /// Set rotation
    /// </summary>
    /// <remarks>Currently only return zero vector</remarks>
    /// <param name="axis"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    public ref readonly Vector3 SetRotation(ref readonly Vector3 axis, float angle)
    {
        _orientation.SetRotation(in axis, angle);
        return ref ZeroRotation;
    }

    /// <inheritdoc/>
    public void GetViewMatrix(out Matrix4x4 viewMatrix)
    {
        Vector3 cameraPosition = _position;
        Vector3 cameraDirection = _orientation.Forward;
        Vector3 cameraTarget = cameraPosition + cameraDirection;
        Vector3 cameraUpVector = _orientation.Up;
        viewMatrix = Matrix4x4Helper.CreateLookAtLeftHanded(in cameraPosition, in cameraTarget, in cameraUpVector);
    }

    /// <summary>
    /// Set direction
    /// </summary>
    /// <param name="direction"></param>
    public void LookTo(scoped ref readonly Vector3 direction)
    {
        _orientation.LookTo(in direction);
    }

    /// <summary>
    /// Set direction
    /// </summary>
    /// <param name="direction"></param>
    public void LookTo(Vector3 direction)
    {
        LookTo(in direction);
    }
}