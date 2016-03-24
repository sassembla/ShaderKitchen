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
    internal class PList<T> : PoolableObject
    {
        protected readonly List<T> list = new List<T>();

        internal override void OnAcquire() { }

        internal override void OnRelease() { this.list.Clear(); }

        public int Count{get { return this.list.Count; }}

        public void Add(T value) { this.list.Add(value); }

        public IEnumerable<T> Items { get { return list; } }

        public IEnumerator<T> GetEnumerator() { return list.GetEnumerator(); }

        public T this[int index] { get { return this.list[index]; } set { this.list[index] = value; } }

        public override string ToString()
        {
            using (var sb = OP<PStringBuilder>.Acquire())
            {
                for (int index = 0; index < this.list.Count; index++)
                {
                    var item = this.list[index];
                    sb.Append('[');
                    sb.Append(Equals(item, default(T)) ? "null" : item.ToString());
                    sb.Append(']');
                    if (index<this.list.Count-1)
                        sb.Append(',');
                }
                return sb.ToString();
            }
        }
    }
}