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

namespace HTMLEngine.Core
{
    internal abstract class DeviceChunk : PoolableObject
    {
        /// <summary>
        /// Bounds
        /// </summary>
        public HtRect Rect;

        /// <summary>
        /// Font to measure and draw
        /// </summary>
        public HtFont Font;

        /// <summary>
        /// Extraspace to add as whitespace
        /// </summary>
        public int ExtraSpace;

        /// <summary>
        /// Total width of chunk (including whitespace
        /// </summary>
        public int TotalWidth { get { return Rect.Width + ExtraSpace; } }

        /// <summary>
        /// Total height of chunk
        /// </summary>
        public int TotalHeight { get { return Rect.Height; } }

        /// <summary>
        /// Draws chunk
        /// </summary>
        /// <param name="deltaTime">delta time</param>
        public abstract void Draw(float deltaTime);

        /// <summary>
        /// Raised when object is acquired from pool
        /// </summary>
        internal override void OnAcquire() { }

        /// <summary>
        /// Raised when object is returned to pool
        /// </summary>
        internal override void OnRelease() { }

        /// <summary>
        /// Measuring chunk size
        /// </summary>
        public abstract void MeasureSize();

        /// <summary>
        /// Is coords inside chunk?
        /// </summary>
        public bool Contains(int x, int y)
        {
            var left = Rect.Left;
            var right = Rect.Right + ExtraSpace;
            var top = Rect.Top;
            var bottom = Rect.Bottom;
            return ((x >= left) && (x < right) && (y >= top) && (y < bottom)); 
        }
    }
}