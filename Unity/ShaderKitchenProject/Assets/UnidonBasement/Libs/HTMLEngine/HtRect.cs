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

using System.Globalization;

namespace HTMLEngine
{
    public struct HtRect
    {
        public int X, Y, Width, Height;

        public int Left { get { return this.X; } }
        public int Right { get { return this.X + this.Width; } }
        public int Top { get { return this.Y; } }
        public int Bottom { get { return this.Y + this.Height; } }

        public HtRect(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public HtRect Offset(int dx, int dy) { return new HtRect(this.X + dx, this.Y + dy, this.Width, this.Height); }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "(X:{0} Y:{1} Width:{2} Height:{3})", this.X, this.Y,
                                 this.Width, this.Height);
        }
    }
}