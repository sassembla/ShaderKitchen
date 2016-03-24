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

namespace HTMLEngine.Core
{
    internal partial class DeviceChunkCollection : PoolableObject
    {
        private readonly List<DeviceChunkLine> list = new List<DeviceChunkLine>();
        internal readonly Stack<HtFont> fontStack = new Stack<HtFont>();
        internal readonly Stack<HtColor> colorStack = new Stack<HtColor>();
        private readonly Stack<TextAlign> alignStack = new Stack<TextAlign>();
        private readonly Stack<VertAlign> valignStack = new Stack<VertAlign>();

        internal override void OnAcquire() { }

        internal override void OnRelease() { this.Clear(); }

        public List<DeviceChunkLine> Lines { get { return list; } }

        public List<KeyValuePair<DeviceChunk, string>> Links = new List<KeyValuePair<DeviceChunk, string>>();

        public void Clear(bool releaseItems = true)
        {
            if (releaseItems)
                foreach (DeviceChunkLine line in this.list)
                    line.Dispose();
            this.Links.Clear();
            this.list.Clear();
            this.fontStack.Clear();
            this.colorStack.Clear();
            this.alignStack.Clear();
            this.valignStack.Clear();
        }
    }
}