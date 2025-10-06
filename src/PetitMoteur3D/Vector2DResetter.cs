using System;
using Silk.NET.Maths;

namespace PetitMoteur3D
{
    public class Vector2DResetter<T> : IIResetter<Vector2D<T>> where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        public unsafe void Reset(ref Vector2D<T> instance)
        {
            MemoryHelper.ResetMemory(instance);
        }
    }
}
