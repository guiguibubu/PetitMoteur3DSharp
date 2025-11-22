using System.Numerics;

namespace PetitMoteur3D;

internal class FrustrumView
{
    public ref readonly Matrix4x4 MatProj { get { return ref _matProj; } }
    
    private Matrix4x4 _matProj;

    public FrustrumView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
    {
        _matProj = CreatePerspectiveFieldOfViewLH(
            fieldOfView,
            aspectRatio,
            nearPlaneDistance,
            farPlaneDistance
        );
    }

    public void Update(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
    {
        _matProj = CreatePerspectiveFieldOfViewLH(
            fieldOfView,
            aspectRatio,
            nearPlaneDistance,
            farPlaneDistance
        );
    }

    private static Matrix4x4 CreatePerspectiveFieldOfViewLH(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
    {
        Matrix4x4 result = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance);
        result.M31 = -result.M31;
        result.M32 = -result.M32;
        result.M33 = -result.M33;
        result.M34 = -result.M34;
        return result;
    }
}
