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

using System.Collections.Generic;

namespace HTMLEngine
{
    internal class PDictionary<K, V> : PoolableObject
    {
        private readonly Dictionary<K, V> dict = new Dictionary<K, V>();

        internal override void OnAcquire() { }

        internal override void OnRelease() { this.dict.Clear(); }

        public void Clear() { this.dict.Clear(); }
        
        public int Count { get { return this.dict.Count; } }
        
        public IEnumerable<K> Keys { get { return this.dict.Keys; } }
        
        public IEnumerable<V> Values { get { return this.dict.Values; } }

        public V this[K key]
        {
            get
            {
                V ret;
                return this.dict.TryGetValue(key, out ret) ? ret : default(V);
            }

            set { this.dict[key] = value; }
        }
    }

    internal class PIgnoreCaseDictionary<V> : PoolableObject
    {
        private readonly Dictionary<string, V> dict = new Dictionary<string, V>();

        internal override void OnAcquire() { }

        internal override void OnRelease() { dict.Clear(); }

        public void Clear() { this.dict.Clear(); }

        public int Count { get { return this.dict.Count; } }

        public IEnumerable<string> Keys { get { return this.dict.Keys; } }

        public IEnumerable<V> Values { get { return this.dict.Values; } }

        public V this[string key]
        {
            get
            {
                V ret;
                return this.dict.TryGetValue(key, out ret) ? ret : default(V);
            }

            set { this.dict[key] = value; }
        }

    }
}