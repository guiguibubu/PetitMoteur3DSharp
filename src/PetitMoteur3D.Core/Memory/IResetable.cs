namespace PetitMoteur3D.Core.Memory;

public interface IResetable
{
    void Reset();
}

public interface IResetter<T>
{
    void Reset(ref T instance);
}
