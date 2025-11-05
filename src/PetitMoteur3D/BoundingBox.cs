using System.Numerics;

namespace PetitMoteur3D;

internal struct BoundingBox
{
    public Vector3 Min;
    public Vector3 Max;

    public BoundingBox(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    public BoundingBox()
    : this(new Vector3(float.MinValue, float.MinValue, float.MinValue), new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
    { }
}