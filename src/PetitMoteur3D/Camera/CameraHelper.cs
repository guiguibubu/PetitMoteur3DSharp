using System;
using Silk.NET.Maths;

namespace PetitMoteur3D.Camera;

internal static class CameraHelper
{
    public static Matrix4X4<T> CreateLookAtLH<T>(ref readonly Vector3D<T> cameraPosition, ref readonly Vector3D<T> cameraTarget, ref readonly Vector3D<T> cameraUpVector)
       where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        Vector3D<T> zaxis = Vector3D.Normalize(cameraTarget - cameraPosition);
        Vector3D<T> xaxis = Vector3D.Normalize(Vector3D.Cross(cameraUpVector, zaxis));
        Vector3D<T> yaxis = Vector3D.Cross(zaxis, xaxis);

        Matrix4X4<T> result = Matrix4X4<T>.Identity;

        result.M11 = xaxis.X;
        result.M12 = yaxis.X;
        result.M13 = zaxis.X;

        result.M21 = xaxis.Y;
        result.M22 = yaxis.Y;
        result.M23 = zaxis.Y;

        result.M31 = xaxis.Z;
        result.M32 = yaxis.Z;
        result.M33 = zaxis.Z;

        result.M41 = Scalar.Negate(Vector3D.Dot(xaxis, cameraPosition));
        result.M42 = Scalar.Negate(Vector3D.Dot(yaxis, cameraPosition));
        result.M43 = Scalar.Negate(Vector3D.Dot(zaxis, cameraPosition));

        return result;
    }

    public static Matrix4X4<float> CreateLookAtLH(scoped ref readonly System.Numerics.Vector3 cameraPosition, scoped ref readonly System.Numerics.Vector3 cameraTarget, scoped ref readonly System.Numerics.Vector3 cameraUpVector)
    {
        System.Numerics.Vector3 zaxis = System.Numerics.Vector3.Normalize(cameraTarget - cameraPosition);
        System.Numerics.Vector3 xaxis = System.Numerics.Vector3.Normalize(System.Numerics.Vector3.Cross(cameraUpVector, zaxis));
        System.Numerics.Vector3 yaxis = System.Numerics.Vector3.Cross(zaxis, xaxis);

        Matrix4X4<float> result = Matrix4X4<float>.Identity;

        result.M11 = xaxis.X;
        result.M12 = yaxis.X;
        result.M13 = zaxis.X;

        result.M21 = xaxis.Y;
        result.M22 = yaxis.Y;
        result.M23 = zaxis.Y;

        result.M31 = xaxis.Z;
        result.M32 = yaxis.Z;
        result.M33 = zaxis.Z;

        result.M41 = -System.Numerics.Vector3.Dot(xaxis, cameraPosition);
        result.M42 = -System.Numerics.Vector3.Dot(yaxis, cameraPosition);
        result.M43 = -System.Numerics.Vector3.Dot(zaxis, cameraPosition);

        return result;
    }

    public static System.Numerics.Matrix4x4 CreateLookToLH(scoped ref readonly System.Numerics.Vector3 cameraPosition, scoped ref readonly System.Numerics.Vector3 cameraTarget, scoped ref readonly System.Numerics.Vector3 cameraUpVector)
    {
        System.Numerics.Vector3 cameraDirection = cameraTarget - cameraPosition;

        return System.Numerics.Matrix4x4.CreateLookToLeftHanded(cameraPosition, cameraDirection, cameraUpVector);
    }

    public static System.Numerics.Matrix4x4 CreateLookTo(scoped ref readonly System.Numerics.Vector3 cameraPosition, scoped ref readonly System.Numerics.Vector3 cameraTarget, scoped ref readonly System.Numerics.Vector3 cameraUpVector)
    {
        System.Numerics.Vector3 cameraDirection = cameraTarget - cameraPosition;

        return System.Numerics.Matrix4x4.CreateLookTo(cameraPosition, cameraDirection, cameraUpVector);
    }
}