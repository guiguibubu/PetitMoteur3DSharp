namespace PetitMoteur3D;

internal interface IResetable
{
    void Reset();
}

internal interface IIResetter<T>
{
    void Reset(ref T instance);
}
