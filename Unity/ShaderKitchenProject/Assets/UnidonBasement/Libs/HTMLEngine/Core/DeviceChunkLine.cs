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
    internal class DeviceChunkLine : PoolableObject
    {
        /// <summary>
        /// Indicates that line is full and justify can be applied
        /// </summary>
        public bool IsFull;

        /// <summary>
        /// Y of line
        /// </summary>
        public int Y;

        /// <summary>
        /// Viewport width
        /// </summary>
        public int MaxWidth;

        /// <summary>
        /// Total line width
        /// </summary>
        public int Width;

        /// <summary>
        /// Total line height
        /// </summary>
        public int Height;

        private readonly List<DeviceChunk> list = new List<DeviceChunk>();

        internal override void OnAcquire()
        {
            Y = 0;
            MaxWidth = 0;
            Width = 0;
            Height = 0;
        }

        internal override void OnRelease() { this.Clear(); }

        public void Clear(bool releaseItems = true)
        {
            if (releaseItems)
                foreach (DeviceChunk line in this.list)
                    line.Dispose();
            this.list.Clear();
        }

        public List<DeviceChunk> Chunks { get { return list; } }

        public int AvailWidth
        {
            get
            {
                DeviceChunk prev = list.Count > 0 ? list[list.Count - 1] : null;
                var freeX = prev == null ? 0 : prev.Rect.Right + prev.Font.WhiteSize;
                return MaxWidth - freeX;
            }
        }

        /// <summary>
        /// Add chunk to line
        /// </summary>
        /// <param name="chunk">DeviceChunk size (Rect.Width and Rect.Height) must be measured.</param>
        /// <returns>True, if chunk added. False, if no horz space here.</returns>
        public bool AddChunk(DeviceChunk chunk)
        {
            // we need prev chunk to calc whitespace and new position
            DeviceChunk prev = list.Count > 0 ? list[list.Count - 1] : null;
            
            // check if we can put new chunk into line
            var freeX = prev == null ? 0 : prev.Rect.Right + prev.Font.WhiteSize;
            if (freeX + chunk.Rect.Width > MaxWidth) return false;

            // we can, so add extra space into prev chunk
            if (prev != null)
            {
                prev.ExtraSpace = prev.Font.WhiteSize;
                Width += prev.ExtraSpace;
            }
            // set position to new chunk
            chunk.Rect.X = freeX;
            chunk.Rect.Y = this.Y;
            chunk.ExtraSpace = 0;
            // update line width and height
            Width += chunk.Rect.Width;
            if (chunk.Rect.Height > Height) Height = chunk.Rect.Height;

            list.Add(chunk);
            return true;
        }

        /// <summary>
        /// Align chunks horz
        /// </summary>
        /// <param name="align">Align type</param>
        public void HorzAlign(TextAlign align)
        {
            // we can not justify lines with just one chunk, also if its already fit
            if (align == TextAlign.Justify)
            {
                if (!IsFull || list.Count < 2 || MaxWidth - Width <= 0)
                    align = TextAlign.Left;
            }

            int currX;
            switch (align)
            {
                case TextAlign.Left:
                    currX = 0;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var chunk = list[i];
                        chunk.Rect.X = currX;
                        currX += chunk.TotalWidth;
                    }
                    break;
                case TextAlign.Right:
                    currX = MaxWidth - Width;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var chunk = list[i];
                        chunk.Rect.X = currX;
                        currX += chunk.TotalWidth;
                    }
                    break;
                case TextAlign.Center:
                    currX = (MaxWidth - Width)/2;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var chunk = list[i];
                        chunk.Rect.X = currX;
                        currX += chunk.TotalWidth;
                    }
                    break;
                case TextAlign.Justify:
                    float f = (MaxWidth - Width)/(float) (list.Count - 1);
                    float fx = 0;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var chunk = list[i];
                        chunk.Rect.X = (int)fx;
                        fx += chunk.TotalWidth;
                        fx += f;
                    }
                    break;
            }
        }

        /// <summary>
        /// Align chunks verticaly
        /// </summary>
        /// <param name="align"></param>
        public void VertAlign(VertAlign align)
        {
            switch (align)
            {
                case Core.VertAlign.Top:
                    for (int i = 0; i < list.Count; i++)
                    {
                        var chunk = list[i];
                        chunk.Rect.Y = Y;
                    }
                    break;
                case Core.VertAlign.Middle:
                    for (int i = 0; i < list.Count; i++)
                    {
                        var chunk = list[i];
                        chunk.Rect.Y = Y + Height / 2 - chunk.Rect.Height / 2;
                    }
                    break;
                case Core.VertAlign.Bottom:
                    for (int i = 0; i < list.Count; i++)
                    {
                        var chunk = list[i];
                        chunk.Rect.Y = Y + Height - chunk.Rect.Height;
                    }
                    break;
            }
        }

        public override string ToString() { return string.Format("Chunks:{0}", list.Count); }
    }
}