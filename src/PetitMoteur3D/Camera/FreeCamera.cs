using System;
using System.Drawing;
using System.Numerics;
using PetitMoteur3D.Core.Math;
using PetitMoteur3D.Input;
using PetitMoteur3D.Logging;
using PetitMoteur3D.Window;

namespace PetitMoteur3D.Camera;

internal class FreeCamera : ICamera, IRotationObjet
{
    /// <summary>
    /// Champ vision
    /// </summary>
    public float ChampVision { get; init; }

    /// <inheritdoc/>
    public ref readonly Vector3 Position => ref _position;

    private Vector3 _position;

    private IWindow _window;
    private Orientation3D _orientation;

    private IInputContext? _inputContext;

    private const float WindowCenterOffset = 1f / 8;

    private static readonly Vector3 UpRotationAxis = Vector3.UnitY;
    private static readonly Vector3 ZeroRotation = Vector3.Zero;

    /// <summary>
    /// Constructeur par defaut
    /// </summary>
    public FreeCamera(IWindow window) : this(window, (float)(Math.PI / 4))
    {

    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="champVision"></param>
    public FreeCamera(IWindow window, float champVision) : this(window, champVision, Vector3.Zero)
    {

    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public FreeCamera(IWindow window, float champVision, ref readonly Vector3 position)
    {
        _window = window;
        ChampVision = champVision;
        _position = position;
        _orientation = new Orientation3D();
        _inputContext = default;
    }

    /// <inheritdoc/>
    public virtual void Update(float elapsedTime)
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
        Size windowsSize = _window.Size;

        Vector2 positionMouse = mouse.Position;

        bool intoWindow = positionMouse.X >= 0 
            && positionMouse.X <= windowsSize.Width
            && positionMouse.Y >= 0
            && positionMouse.Y <= windowsSize.Height;

        if (!intoWindow)
        {
            return;
        }

        // Normalize mouse position into [-1, 1] in window space
        Vector2 positionNormalized = ((mouse.Position / new Vector2(windowsSize.Width, windowsSize.Height)) - new Vector2(0.5f)) * 2;

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
    public ref readonly Vector3 Move(scoped ref readonly Vector3 move)
    {
        return ref Move(move.X, move.Y, move.Z);
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

    /// <inheritdoc/>
    public void GetViewMatrix(out Matrix4x4 viewMatrix)
    {
        Vector3 cameraPosition = _position;
        Vector3 cameraDirection = _orientation.Forward;
        Vector3 cameraTarget = cameraPosition + cameraDirection;
        Vector3 cameraUpVector = _orientation.Up;
        viewMatrix = Matrix4x4Helper.CreateLookAtLeftHanded(in cameraPosition, in cameraTarget, in cameraUpVector);
    }
}