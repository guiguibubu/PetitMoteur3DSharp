using System;
using System.Drawing;
using PetitMoteur3D.Core.Math;
using PetitMoteur3D.Input;
using PetitMoteur3D.Logging;
using PetitMoteur3D.Window;
using Silk.NET.Maths;

namespace PetitMoteur3D.Camera;

internal class FreeCamera : ICamera, IRotationObjet
{
    /// <summary>
    /// Champ vision
    /// </summary>
    public float ChampVision { get; init; }

    /// <inheritdoc/>
    public ref readonly Vector3D<float> Position => ref _position;

    private Vector3D<float> _position;

    private IWindow _window;
    private Orientation3D _orientation;

    private IInputContext? _inputContext;

    private const float WindowCenterOffset = 1f / 8;

    private static readonly System.Numerics.Vector3 UpRotationAxis = System.Numerics.Vector3.UnitY;
    private static readonly Vector3D<float> ZeroRotation = Vector3D<float>.Zero;

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
    public FreeCamera(IWindow window, float champVision) : this(window, champVision, Vector3D<float>.Zero)
    {

    }

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public FreeCamera(IWindow window, float champVision, ref readonly Vector3D<float> position)
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

        IMouse mouse = _inputContext.Mice[0];
        Size windowsSize = _window.Size;

        System.Numerics.Vector2 positionMouse = mouse.Position;

        bool intoWindow = positionMouse.X >= 0 
            && positionMouse.X <= windowsSize.Width
            && positionMouse.Y >= 0
            && positionMouse.Y <= windowsSize.Height;

        if (!intoWindow)
        {
            return;
        }

        // Normalize mouse position into [-1, 1] in window space
        System.Numerics.Vector2 positionNormalized = ((mouse.Position / new System.Numerics.Vector2(windowsSize.Width, windowsSize.Height)) - new System.Numerics.Vector2(0.5f)) * 2;

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
    public ref readonly Vector3D<float> Move(scoped ref readonly Vector3D<float> move)
    {
        _position.X += move.X;
        _position.Y += move.Y;
        _position.Z += move.Z;
        return ref _position;
    }

    /// <inheritdoc/>
    /// <remarks>Currently only return zero vector</remarks>
    public ref readonly Vector3D<float> RotateEuler(ref readonly Vector3D<float> rotation)
    {
        System.Numerics.Quaternion quaternion = System.Numerics.Quaternion.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        _orientation.Rotate(in quaternion);
        return ref ZeroRotation;
    }


    /// <inheritdoc/>
    /// <remarks>Currently only return zero vector</remarks>
    public ref readonly Vector3D<float> Rotate(ref readonly Vector3D<float> axis, float angle)
    {
        _orientation.Rotate(axis.ToSystem(), angle);
        return ref ZeroRotation;
    }

    /// <inheritdoc/>
    public void GetViewMatrix(out Matrix4X4<float> viewMatrix)
    {
        Vector3D<float> cameraDirection = _orientation.Forward.ToGeneric();
        Vector3D<float> cameraTarget = _position + cameraDirection;
        Vector3D<float> cameraUpVector = _orientation.Up.ToGeneric();
        viewMatrix = CameraHelper.CreateLookAtLH(in _position, in cameraTarget, in cameraUpVector);
    }
}