using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PetitMoteur3D
{
    internal interface IObjectPool<T>
    {
        T Get();
        void Return(T item);
    }

    internal static class ObjectPoolFactory
    {
        public static IObjectPool<T> Create<T>() where T : IResetable, new()
        {
            return new ObjectPoolNoParameterConstructorResetable<T>();
        }

        public static IObjectPool<T> Create<T>(Action<T> objectResetFunc) where T : new()
        {
            return new ObjectPoolNoParameterConstructor<T>(objectResetFunc);
        }

        public static IObjectPool<T> Create<T>(IIResetter<T> resetter) where T : new()
        {
            return new ObjectPoolNoParameterConstructor<T>(resetter);
        }

        public static IObjectPool<T> Create<T>(Func<T> objectGenerator) where T : IResetable
        {
            return new ObjectPoolResetable<T>(objectGenerator);
        }

        public static IObjectPool<T> Create<T>(Func<T> objectGenerator, Action<T> objectResetFunc)
        {
            return new BaseObjectPoolImpl<T>(objectGenerator, objectResetFunc);
        }

        public static IObjectPool<T> Create<T>(Func<T> objectGenerator, IIResetter<T> resetter)
        {
            return new BaseObjectPoolImpl<T>(objectGenerator, resetter);
        }

        private class BaseObjectPoolImpl<T> : IObjectPool<T>, IDisposable
        {
            private readonly ConcurrentBag<T> _objects;
            private readonly Func<T> _objectGenerator;
            private readonly Action<T> _objectResetFunc;

            public BaseObjectPoolImpl(Func<T> objectGenerator, Action<T> objectResetFunc)
            {
                ArgumentNullException.ThrowIfNull(objectGenerator);
                ArgumentNullException.ThrowIfNull(objectResetFunc);
                _objectGenerator = objectGenerator;
                _objectResetFunc = objectResetFunc;
                _objects = new ConcurrentBag<T>();
            }

            public BaseObjectPoolImpl(Func<T> objectGenerator, IIResetter<T> resetter)
                : this(objectGenerator, (T item) => resetter.Reset(ref item))
            { }

            public T Get()
            {
                if (_objects.TryTake(out T? item))
                {
                    GC.ReRegisterForFinalize(item!);
                    return item;
                }
                else
                {
                    return _objectGenerator();
                }
            }

            public void Return(T item)
            {
                if (item is null)
                    return;
                //_objectResetFunc.Invoke(item);
                ResetMemory(item);
                GC.SuppressFinalize(item);
                _objects.Add(item);
            }

            private void ResetMemory(T item)
            {
                ref T managedRef = ref item;
                ref byte starAdress = ref Unsafe.As<T, byte>(ref managedRef); // reinterpret as managed pointer to Int16
                Unsafe.InitBlock(ref starAdress, 0, (uint)Unsafe.SizeOf<T>());
            }

            private bool _disposed = false;

            /// <inheritdoc/>
            public void Dispose()
            {
                Dispose(disposing: true);
                // This object will be cleaned up by the Dispose method.
                // Therefore, you should call GC.SuppressFinalize to
                // take this object off the finalization queue
                // and prevent finalization code for this object
                // from executing a second time.
                GC.SuppressFinalize(this);
            }

            // Dispose(bool disposing) executes in two distinct scenarios.
            // If disposing equals true, the method has been called directly
            // or indirectly by a user's code. Managed and unmanaged resources
            // can be disposed.
            // If disposing equals false, the method has been called by the
            // runtime from inside the finalizer and you should not reference
            // other objects. Only unmanaged resources can be disposed.
            private unsafe void Dispose(bool disposing)
            {
                // Check to see if Dispose has already been called.
                if (!_disposed)
                {
                    // If disposing equals true, dispose all managed
                    // and unmanaged resources.
                    if (disposing)
                    {
                        // Dispose managed resources.
                    }

                    // Call the appropriate methods to clean up
                    // unmanaged resources here.
                    // If disposing is false,
                    // only the following code is executed.

                    foreach (T item in _objects.Where(o => o is not null))
                    {
                        GC.ReRegisterForFinalize(item!);
                    }

                    // Note disposing has been done.
                    _disposed = true;
                }
            }
        }

        private class ObjectPoolNoParameterConstructor<T> : BaseObjectPoolImpl<T> where T : new()
        {
            public ObjectPoolNoParameterConstructor(Action<T> objectResetFunc) : base(() => new T(), objectResetFunc)
            {
            }
            public ObjectPoolNoParameterConstructor(IIResetter<T> resetter) : base(() => new T(), (T item) => resetter.Reset(ref item))
            {
            }
        }

        private class ObjectPoolResetable<T> : BaseObjectPoolImpl<T> where T : IResetable
        {
            public ObjectPoolResetable(Func<T> objectGenerator) : base(objectGenerator, (T item) => item.Reset())
            {
            }
        }

        private class ObjectPoolNoParameterConstructorResetable<T> : BaseObjectPoolImpl<T> where T : IResetable, new()
        {
            public ObjectPoolNoParameterConstructorResetable() : base(() => new T(), (T item) => item.Reset())
            {
            }
        }
    }
}
