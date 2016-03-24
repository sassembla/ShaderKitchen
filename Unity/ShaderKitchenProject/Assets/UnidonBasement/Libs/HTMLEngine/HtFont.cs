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

namespace HTMLEngine
{
    /// <summary>
    /// Abstract font
    /// </summary>
    public abstract class HtFont
    {
        /// <summary>
        /// Face of font
        /// </summary>
        public string Face { get; private set; }

        /// <summary>
        /// Size of font
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Bold flag
        /// </summary>
        public bool Bold { get; private set; }

        /// <summary>
        /// Italic flag
        /// </summary>
        public bool Italic { get; private set; }

        /// <summary>
        /// Line height
        /// </summary>
        public abstract int LineSpacing { get; }

        /// <summary>
        /// Space width
        /// </summary>
        public abstract int WhiteSize { get; }

        /// <summary>
        /// Calcs text width and height
        /// </summary>
        /// <param name="text">text</param>
        /// <returns>Calculated width and height</returns>
        public abstract HtSize Measure(string text);

        protected HtFont(string face, int size, bool bold, bool italic)
        {
            this.Face = face;
            this.Size = size;
            this.Bold = bold;
            this.Italic = italic;
        }

        public abstract void Draw(HtRect rect, HtColor color, string text);

        public override string ToString() { return string.Format("{0}{1}{2}{3}", this.Face, this.Size, this.Bold ? "b" : "", this.Italic ? "i" : ""); }
    }
}