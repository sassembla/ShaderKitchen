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
using UnityEngine;

namespace HTMLEngine.Unity3D
{
    /// <summary>
    /// Provides gate between HTMLEngine and Unity3D. Implements abstract class.
    /// </summary>
    public class HtmlDevice : HtDevice
    {
        /// <summary>
        /// Fonts cache (to do not load every time from resouces)
        /// </summary>
        private readonly Dictionary<string, HtmlFont> fonts = new Dictionary<string, HtmlFont>();
        /// <summary>
        /// Image cache (same thing)
        /// </summary>
        private readonly Dictionary<string, HtmlImage> images = new Dictionary<string, HtmlImage>();

        /// <summary>
        /// White texture (for FillRect method)
        /// </summary>
        private static Texture2D whiteTex;

        /// <summary>
        /// Load font
        /// </summary>
        /// <param name="face">Font name</param>
        /// <param name="size">Font size</param>
        /// <param name="bold">Bold flag</param>
        /// <param name="italic">Italic flag</param>
        /// <returns>Loaded font</returns>
        public override HtFont LoadFont(string face, int size, bool bold, bool italic)
        {
            // try get from cache
            string key = string.Format("{0}{1}{2}{3}", face, size, bold ? "b" : "", italic ? "i" : "");
            HtmlFont ret;
            if (fonts.TryGetValue(key, out ret)) return ret;
            // fail with cache, so create new and store into cache
            ret = new HtmlFont(face, size, bold, italic);
            fonts[key] = ret;
            return ret;
        }

        /// <summary>
        /// Load image
        /// </summary>
        /// <param name="src">src attribute from img tag</param>
        /// <returns>Loaded image</returns>
        public override HtImage LoadImage(string src)
        {
            // try get from cache
            HtmlImage ret;
            if (images.TryGetValue(src, out ret)) return ret;
            // fail with cache, so create new and store into cache
            ret = new HtmlImage(src);
            images[src] = ret;
            return ret;
        }

        /// <summary>
        /// FillRect implementation
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        public override void FillRect(HtRect rect, HtColor color)
        {
            // create white texture if need
            if (whiteTex==null)
            {
                whiteTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                whiteTex.SetPixel(0, 0, Color.white);
                whiteTex.Apply(false, true);
            }
            // store current gui color
            var oldColor = GUI.color;
            // fill rect with given color
            GUI.color = new Color32(color.R,color.G,color.B,color.A);
            GUI.DrawTexture(new Rect(rect.X, rect.Y, rect.Width, rect.Height), whiteTex);
            // restore gui color
            GUI.color = oldColor;
        }
    }
}
