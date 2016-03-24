/* Copyright (C) 2012 Ruslan A. Abdrashitov

Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
and associated documentation files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions 
of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE. */

using System;
using System.Collections.Generic;

namespace HTMLEngine
{
    internal delegate void ObjectPoolHandler(PoolableObject obj);

    public abstract class PoolableObject : IDisposable
    {
        private ObjectPoolHandler _handler;

        internal void SetPoolHandler(ObjectPoolHandler handler) { this._handler = handler; }

        internal abstract void OnAcquire();
        internal abstract void OnRelease();

        public void Dispose()
        {
            if (this._handler != null)
                this._handler(this);
            else
            {
                // do nothing dor now
            }
        }

        ~PoolableObject()
        {
            if (this._handler != null)
                HtEngine.Log(HtLogLevel.Warning, "Poolable object is not disposed: " + this.GetType());
        }
    }

    public sealed class OP<T> where T : PoolableObject, new()
    {
        public static readonly OP<T> Instance = new OP<T>(16);

        private readonly Queue<T> _pool;
        private int _capacity;
        private readonly ObjectPoolHandler _returnHandler;

        private OP(int capacity)
        {
            if (capacity < 1)
                capacity = 1;

            this._returnHandler = this.ReturnObject;

            this._pool = new Queue<T>();

            for (int i = 0; i < capacity; i++)
            {
                T obj = CreateInstance();
                this._pool.Enqueue(obj);
            }

            this._capacity = capacity;
        }

        private static T CreateInstance() { return new T(); }

        public static T Acquire() { return Instance.AcquireInternal(); }

        private T AcquireInternal()
        {
            if (this._pool.Count == 0)
            {
                for (int i = 0; i < this._capacity; i++)
                {
                    T obj = CreateInstance();
                    this._pool.Enqueue(obj);
                }
                this._capacity *= 2;
            }

            T newObject = this._pool.Dequeue();
            newObject.SetPoolHandler(this._returnHandler);
            newObject.OnAcquire();
            return newObject;
        }

        private void ReturnObject(PoolableObject obj)
        {
            obj.SetPoolHandler(null);
            obj.OnRelease();
            this._pool.Enqueue((T) obj);
        }

        public int Count { get { return this._pool.Count; } }
    }
}