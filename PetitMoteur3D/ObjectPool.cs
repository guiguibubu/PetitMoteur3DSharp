using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace PetitMoteur3D
{
    public delegate void ObjectReseterDelegate<T>(ref T instance) where T : struct;

    internal interface IObjectPool<T> where T : struct
    {
        void Get([NotNull] out ObjectPoolWrapper<T> item);
        void Return(ObjectPoolWrapper<T> item);
    }

    internal static class ObjectPoolFactory
    {
        public static IObjectPool<T> Create<T>() where T : struct, IResetable
        {
            return new BaseObjectPoolImpl<T>((ref T item) => item.Reset());
        }

        public static IObjectPool<T> Create<T>(ObjectReseterDelegate<T> objectResetFunc) where T : struct
        {
            return new BaseObjectPoolImpl<T>(objectResetFunc);
        }

        public static IObjectPool<T> Create<T>(IIResetter<T> resetter) where T : struct
        {
            return new BaseObjectPoolImpl<T>(resetter);
        }

        private class BaseObjectPoolImpl<T> : IObjectPool<T>, IDisposable where T : struct
        {
            private readonly ConcurrentDictionary<Guid, ObjectPoolWrapper<T>> _objects;
            private readonly ConcurrentBag<Guid> _objectsAvailableKeys;
            private readonly ObjectReseterDelegate<T> _objectResetFunc;

            public BaseObjectPoolImpl(ObjectReseterDelegate<T> objectResetFunc)
            {
                ArgumentNullException.ThrowIfNull(objectResetFunc);
                _objectResetFunc = objectResetFunc;
                _objects = new ConcurrentDictionary<Guid, ObjectPoolWrapper<T>>();
                _objectsAvailableKeys = new ConcurrentBag<Guid>();
            }

            public BaseObjectPoolImpl(IIResetter<T> resetter)
                : this((ref T item) => resetter.Reset(ref item))
            { }

            public void Get(out ObjectPoolWrapper<T> item)
            {
                if (_objectsAvailableKeys.TryTake(out Guid id))
                {
                    item = _objects[id];
                    _objectResetFunc.Invoke(ref item.Data);
                    //GC.ReRegisterForFinalize(item!);
                }
                else
                {
                    item = new();
                    _objects.TryAdd(item.Id, item);
                    GC.SuppressFinalize(item);
                }
            }

            public void Return(ObjectPoolWrapper<T> item)
            {
                _objectsAvailableKeys.Add(item.Id);
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

                    foreach (ObjectPoolWrapper<T> item in _objects.Values)
                    {
                        GC.ReRegisterForFinalize(item!);
                    }

                    // Note disposing has been done.
                    _disposed = true;
                }
            }
        }
    }

    internal class ObjectPoolWrapper<T> where T : struct
    {
        public readonly Guid Id;
        public T Data;

        public ObjectPoolWrapper()
        {
            Id = Guid.NewGuid();
            Data = new T();
        }
    }
}
