using System.Runtime.CompilerServices;

namespace PetitMoteur3D
{
    internal static class MemoryHelper
    {
        public static void ResetMemory<T>(T item)
        {
            ref T managedRef = ref item;
            ref byte starAdress = ref Unsafe.As<T, byte>(ref managedRef); // reinterpret as managed pointer to Int16
            Unsafe.InitBlock(ref starAdress, 0, (uint)Unsafe.SizeOf<T>());
        }

        public static unsafe void ResetMemory<T>(T* item) where T : unmanaged
        {
            Unsafe.InitBlock(item, 0, (uint)Unsafe.SizeOf<T>());
        }
    }
}
