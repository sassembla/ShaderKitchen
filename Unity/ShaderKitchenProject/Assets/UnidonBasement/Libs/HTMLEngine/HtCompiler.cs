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
using HTMLEngine.Core;

namespace HTMLEngine
{
    public class HtCompiler : PoolableObject
    {
        internal override void OnAcquire() { this.d = OP<DeviceChunkCollection>.Acquire(); }

        internal override void OnRelease()
        {
            this.d.Dispose();
            this.d = null;
        }

        private readonly Reader reader = new Reader();

        private DeviceChunkCollection d;

        public int CompiledWidth { get; private set; }
        public int CompiledHeight { get; private set; }

        public string GetLink(int x, int y)
        {
            if (this.d != null)
            {
                for (int link = 0; link < this.d.Links.Count; link++)
                {
                    KeyValuePair<DeviceChunk, string> data = this.d.Links[link];
                    if (data.Key.Contains(x, y)) return data.Value;
                }
            }
            return null;
        }

        public void Compile(string source, int width)
        {
            this.reader.SetSource(source);
            {
                using (HtmlChunkCollection h = OP<HtmlChunkCollection>.Acquire())
                {
                    h.Read(this.reader);
                    this.Compile(h.GetEnumerator(), width);
                }
            }
        }

        internal void Compile(IEnumerator<HtmlChunk> source, int width, TextAlign align = TextAlign.Left, VertAlign valign = VertAlign.Bottom)
        {
            this.d.Clear();
            this.CompiledWidth = width;
            this.d.Parse(source, width, align, valign);
            this.UpdateHeight();
        }

        private void UpdateHeight()
        {
            if (this.d.Lines.Count > 0)
            {
                DeviceChunkLine line = this.d.Lines[this.d.Lines.Count - 1];
                this.CompiledHeight = line.Y + line.Height;
            }
            else
            {
                this.CompiledHeight = 0;
            }
        }


        public void Draw(float deltaTime)
        {
            if (this.d != null)
            {
                for (int lineIndex = 0; lineIndex < this.d.Lines.Count; lineIndex++)
                {
                    DeviceChunkLine line = this.d.Lines[lineIndex];
                    for (int chunkIndex = 0; chunkIndex < line.Chunks.Count; chunkIndex++)
                    {
                        DeviceChunk chunk = line.Chunks[chunkIndex];
                        chunk.Draw(deltaTime);
                    }
                }
            }
        }

        public void Offset(int dx, int dy)
        {
            if (this.d != null)
            {
                for (int lineIndex = 0; lineIndex < this.d.Lines.Count; lineIndex++)
                {
                    DeviceChunkLine line = this.d.Lines[lineIndex];
                    for (int chunkIndex = 0; chunkIndex < line.Chunks.Count; chunkIndex++)
                    {
                        DeviceChunk chunk = line.Chunks[chunkIndex];
                        chunk.Rect.X += dx;
                        chunk.Rect.Y += dy;
                    }
                }
            }
        }

    }
}