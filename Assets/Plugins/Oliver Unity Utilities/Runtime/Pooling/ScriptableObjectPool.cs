using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace OliverBeebe.UnityUtilities.Runtime.Pooling
{
    public abstract class ScriptableObjectPool<T> : ScriptableObject
    {
        private ObjectPool<T> pool;
        private ObjectPool<T> Pool => pool ??= new();

        public T[] ActiveObjects => Pool.ActiveObjects;

        protected abstract T Create();

        protected virtual void Destroy(T obj) { }

        public void Generate(int count, Func<T> create) => Pool.Generate(count, create);
        
        protected T Retrieve(Func<T> create) => Pool.Retrieve(create);

        public virtual T[] RetrieveAll() => Pool.RetrieveAll();

        public virtual void Return(T obj) => Pool.Return(obj);

        public virtual void ReturnAll() => Pool.ReturnAll();
        
        public void Clear() => Pool.Clear(Destroy);
    }
}
