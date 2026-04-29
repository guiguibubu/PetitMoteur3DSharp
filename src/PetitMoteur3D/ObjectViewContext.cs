using System.Numerics;

namespace PetitMoteur3D;

internal struct ObjectViewContext
{
    public Matrix4x4 MatWorld { get; set; }
    public Matrix4x4 AdditionalTransformation { get; set; }

    public ObjectViewContext()
    {
        MatWorld = Matrix4x4.Identity;
        AdditionalTransformation = Matrix4x4.Identity;
    }
}
