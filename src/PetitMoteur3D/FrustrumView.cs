using System.Drawing;
using System.Numerics;

namespace PetitMoteur3D;

internal sealed class FrustrumView
{
    public ref readonly Matrix4x4 MatProj { get { return ref _matProj; } }
    public bool IsOrthographique
    {
        get => _isOrthographique;
        set
        {
            if (_isOrthographique != value)
            {
                _isOrthographique = value; UpdateMatProj();
            }
        }
    }
    public float FieldOfView
    {
        get => _fieldOfView;
        set
        {
            if (_fieldOfView != value)
            {
                _fieldOfView = value; UpdateMatProj();
            }
        }
    }
    public float Width
    {
        get => _width;
        set
        {
            if (_width != value)
            {
                _width = value; UpdateMatProj();
            }
        }
    }
    public float Height
    {
        get => _height;
        set
        {
            if (_height != value)
            {
                _height = value; UpdateMatProj();
            }
        }
    }
    public float NearPlaneDistance
    {
        get => _nearPlaneDistance;
        set
        {
            if (_nearPlaneDistance != value)
            {
                _nearPlaneDistance = value; UpdateMatProj();
            }
        }
    }
    public float FarPlaneDistance
    {
        get => _farPlaneDistance;
        set
        {
            if (_farPlaneDistance != value)
            {
                _farPlaneDistance = value; UpdateMatProj();
            }
        }
    }

    private Matrix4x4 _matProj;
    private bool _isOrthographique;
    private float _fieldOfView;
    private float _width;
    private float _height;
    private float _nearPlaneDistance;
    private float _farPlaneDistance;

    public FrustrumView(float fieldOfView, float width, float height, float nearPlaneDistance, float farPlaneDistance, bool isOrthographic = false)
    {
        _isOrthographique = isOrthographic;
        _fieldOfView = fieldOfView;
        _width = width;
        _height = height;
        _nearPlaneDistance = nearPlaneDistance;
        _farPlaneDistance = farPlaneDistance;
        UpdateMatProj();
    }

    public void Update(float fieldOfView, float width, float height, float nearPlaneDistance, float farPlaneDistance)
    {
        _fieldOfView = fieldOfView;
        _width = width;
        _height = height;
        _nearPlaneDistance = nearPlaneDistance;
        _farPlaneDistance = farPlaneDistance;
        UpdateMatProj();
    }

    private void UpdateMatProj()
    {
        if (!_isOrthographique)
        {
            _matProj = CreatePerspectiveFieldOfViewLH(
                _fieldOfView,
                _width/_height,
                _nearPlaneDistance,
                _farPlaneDistance
            );
        }
        else
        {
            SizeF farPlaneSize = GetOrthographicPlaneSize(_fieldOfView, _width, _height, _nearPlaneDistance, _farPlaneDistance);
            _matProj = CreateOrthographicLH(
                farPlaneSize.Width,
                farPlaneSize.Height,
                _nearPlaneDistance,
                _farPlaneDistance
            );
        }
    }

    private static Matrix4x4 CreatePerspectiveFieldOfViewLH(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
    {
        Matrix4x4 result = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance);
        return result;
    }

    private static Matrix4x4 CreateOrthographicLH(float width, float height, float nearPlaneDistance, float farPlaneDistance)
    {
        Matrix4x4 result = Matrix4x4.CreateOrthographicLeftHanded(width, height, nearPlaneDistance, farPlaneDistance);
        return result;
    }

    private static SizeF GetOrthographicPlaneSize(float fieldOfView, float width, float height, float nearPlaneDistance, float farPlaneDistance)
    {
        const int nbPortionMax = 2;
        const int nbPortion = 1;
        float planMedian = (farPlaneDistance - nearPlaneDistance) * nbPortion / (float)nbPortionMax + nearPlaneDistance;
        float heightMedianPlane = 2 * planMedian * (float)System.Math.Tan(fieldOfView / 2.0f);
        float widthMedianPlane = (width/height) * heightMedianPlane;
        return new SizeF(widthMedianPlane, heightMedianPlane);
    }
}
