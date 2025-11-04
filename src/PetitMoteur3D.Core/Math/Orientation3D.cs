using System.Numerics;

namespace PetitMoteur3D.Core.Math;

public class Orientation3D
{
    private Vector3 _up;
    public ref readonly Vector3 Up { get => ref _up; }

    private Vector3 _forward;
    public ref readonly Vector3 Forward { get => ref _forward; }

    private Vector3 _rigth;
    public ref readonly Vector3 Rigth { get => ref _rigth; }

    private Quaternion _quaternion;
    public ref readonly Quaternion Quaternion { get => ref _quaternion; }

    private static readonly Vector3 BaseRight = Vector3.UnitX;
    private static readonly Vector3 BaseUp = Vector3.UnitY;
    private static readonly Vector3 BaseForward = Vector3.UnitZ;

    /// <summary>
    /// Constructeur par défaut
    /// </summary>
    public Orientation3D()
        : this(in BaseUp, in BaseForward)
    { }

    /// <summary>
    /// Constructeur paramètrique (par copie)
    /// </summary>
    /// <param name="up"></param>
    /// <param name="forward"></param>
    public Orientation3D(Vector3 up, Vector3 forward)
        : this(in up, in forward)
    { }

    /// <summary>
    /// Constructeur paramètrique (par référece)
    /// </summary>
    /// <param name="up"></param>
    /// <param name="forward"></param>
    public Orientation3D(scoped ref readonly Vector3 up, scoped ref readonly Vector3 forward)
    {
        _up = up;
        _forward = forward;
        _rigth = BaseRight;
        _quaternion = Quaternion.Identity;
    }

    public void Rotate(scoped ref readonly Vector3 axis, float angle)
    {
        if ((System.Math.Abs(angle) - 0f) < float.Epsilon)
        {
            return;
        }
        Quaternion quaternionTemp = Quaternion.CreateFromAxisAngle(axis, angle);
        Rotate(in quaternionTemp);
    }

    public void Rotate(scoped ref readonly Quaternion quaternion)
    {
        _quaternion = Quaternion.Normalize(quaternion * _quaternion);
        UpdateOrientation();
    }

    public void SetRotation(scoped ref readonly Vector3 axis, float angle)
    {
        if ((System.Math.Abs(angle) - 0f) < float.Epsilon)
        {
            SetRotation(Quaternion.Identity);
        }
        else
        {
            SetRotation(Quaternion.CreateFromAxisAngle(axis, angle));
        }
    }

    public void SetRotation(Quaternion quaternion)
    {
        SetRotation(in quaternion);
    }

    public void SetRotation(scoped ref readonly Quaternion quaternion)
    {
        _quaternion = quaternion;
        UpdateOrientation();
    }

    public void LookTo(scoped ref readonly Vector3 direction)
    {
        if ((System.Math.Abs(Vector3.DistanceSquared(direction, Vector3.Zero) - 0f) < float.Epsilon))
        {
            return;
        }
        Matrix4x4 viewMatrix = Matrix4x4.CreateLookToLeftHanded(System.Numerics.Vector3.Zero, direction, Vector3.UnitY);
        Quaternion quaternionTemp = Quaternion.CreateFromRotationMatrix(Matrix4x4.Transpose(viewMatrix));
        _quaternion = Quaternion.Normalize(quaternionTemp);
        UpdateOrientation();
    }

    private void UpdateOrientation()
    {
        _up = Vector3.Transform(BaseUp, _quaternion);
        _forward = Vector3.Transform(BaseForward, _quaternion);
        _rigth = Vector3.Normalize(Vector3.Cross(_up, _forward));
    }
}
