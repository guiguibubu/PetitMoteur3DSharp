using System;
using Silk.NET.Maths;

namespace PetitMoteur3D;

public class Box2DResetter<T> : IIResetter<Box2D<T>> where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
{
    public unsafe void Reset(ref Box2D<T> instance)
    {
        MemoryHelper.ResetMemory(instance);
    }
}
