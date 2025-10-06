using Silk.NET.Maths;

namespace PetitMoteur3D
{
    internal struct BoundingBox
    {
        public Vector3D<float> Min;
        public Vector3D<float> Max;

        public BoundingBox(Vector3D<float> min, Vector3D<float> max)
        {
            Min = min;
            Max = max;
        }

        public BoundingBox()
        : this(new Vector3D<float>(float.MinValue, float.MinValue, float.MinValue), new Vector3D<float>(float.MaxValue, float.MaxValue, float.MaxValue))
        {}
    }
}