using System.Numerics;

namespace PetitMoteur3D.Core.Math;

public static class Matrix4x4Helper
{
    public static Matrix4x4 CreateLookTo(Vector3 cameraPosition, Vector3 cameraDirection, Vector3 cameraUpVector)
    {
        return CreateLookTo(in cameraPosition, in cameraDirection, in cameraUpVector);
    }

    public static Matrix4x4 CreateLookTo(scoped ref readonly Vector3 cameraPosition, scoped ref readonly Vector3 cameraDirection, scoped ref readonly Vector3 cameraUpVector)
    {
        Vector3 zaxis = Vector3.Normalize(-cameraDirection);
        Vector3 xaxis = Vector3.Normalize(Vector3.Cross(cameraUpVector, zaxis));
        Vector3 yaxis = Vector3.Cross(zaxis, xaxis);

        Matrix4x4 result = Matrix4x4.Identity;

        result.M11 = xaxis.X;
        result.M12 = yaxis.X;
        result.M13 = zaxis.X;

        result.M21 = xaxis.Y;
        result.M22 = yaxis.Y;
        result.M23 = zaxis.Y;

        result.M31 = xaxis.Z;
        result.M32 = yaxis.Z;
        result.M33 = zaxis.Z;

        result.M41 = -Vector3.Dot(xaxis, cameraPosition);
        result.M42 = -Vector3.Dot(yaxis, cameraPosition);
        result.M43 = -Vector3.Dot(zaxis, cameraPosition);

        return result;
    }

    public static Matrix4x4 CreateLookToLeftHanded(Vector3 cameraPosition, Vector3 cameraDirection, Vector3 cameraUpVector)
    {
        return CreateLookToLeftHanded(in cameraPosition, in cameraDirection, in cameraUpVector);
    }

    public static Matrix4x4 CreateLookToLeftHanded(scoped ref readonly Vector3 cameraPosition, scoped ref readonly Vector3 cameraDirection, scoped ref readonly Vector3 cameraUpVector)
    {
        Vector3 zaxis = Vector3.Normalize(cameraDirection);
        Vector3 xaxis = Vector3.Normalize(Vector3.Cross(cameraUpVector, zaxis));
        Vector3 yaxis = Vector3.Cross(zaxis, xaxis);

        Matrix4x4 result = Matrix4x4.Identity;

        result.M11 = xaxis.X;
        result.M12 = yaxis.X;
        result.M13 = zaxis.X;

        result.M21 = xaxis.Y;
        result.M22 = yaxis.Y;
        result.M23 = zaxis.Y;

        result.M31 = xaxis.Z;
        result.M32 = yaxis.Z;
        result.M33 = zaxis.Z;

        result.M41 = -Vector3.Dot(xaxis, cameraPosition);
        result.M42 = -Vector3.Dot(yaxis, cameraPosition);
        result.M43 = -Vector3.Dot(zaxis, cameraPosition);

        return result;
    }

    public static Matrix4x4 CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
    {
        return CreateLookAt(in cameraPosition, in cameraTarget, in cameraUpVector);
    }

    public static Matrix4x4 CreateLookAt(scoped ref readonly Vector3 cameraPosition, scoped ref readonly Vector3 cameraTarget, scoped ref readonly Vector3 cameraUpVector)
    {
        Vector3 cameraDirection = cameraTarget - cameraPosition;

        return CreateLookTo(in cameraPosition, in cameraDirection, in cameraUpVector);
    }

    public static Matrix4x4 CreateLookAtLeftHanded(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
    {
        return CreateLookAtLeftHanded(in cameraPosition, in cameraTarget, in cameraUpVector);
    }

    public static Matrix4x4 CreateLookAtLeftHanded(scoped ref readonly Vector3 cameraPosition, scoped ref readonly Vector3 cameraTarget, scoped ref readonly Vector3 cameraUpVector)
    {
        Vector3 cameraDirection = cameraTarget - cameraPosition;

        return CreateLookToLeftHanded(in cameraPosition, in cameraDirection, in cameraUpVector);
    }

}