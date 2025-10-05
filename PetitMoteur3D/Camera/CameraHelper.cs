using System;
using Silk.NET.Maths;

namespace PetitMoteur3D.Camera
{
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
    }
}