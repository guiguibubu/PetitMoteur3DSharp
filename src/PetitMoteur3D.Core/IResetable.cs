namespace PetitMoteur3D.Core;

public interface IResetable
{
    void Reset();
}

public interface IResetter<T>
{
    void Reset(ref T instance);
}
