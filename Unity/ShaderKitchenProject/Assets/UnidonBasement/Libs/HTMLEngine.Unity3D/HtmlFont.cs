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

using UnityEngine;

namespace HTMLEngine.Unity3D
{
    /// <summary>
    /// Provides font for use with HTMLEngine. Implements abstract class.
    /// </summary>
    public class HtmlFont : HtFont
    {
        /// <summary>
        /// style to draw
        /// </summary>
        public readonly GUIStyle style = new GUIStyle();
        /// <summary>
        /// content to draw
        /// </summary>
        public readonly GUIContent content = new GUIContent();
        /// <summary>
        /// Width of whitespace
        /// </summary>
        private readonly int whiteSize;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="face">Font name</param>
        /// <param name="size">Font size</param>
        /// <param name="bold">Bold flag</param>
        /// <param name="italic">Italic flag</param>
        public HtmlFont(string face, int size, bool bold, bool italic) : base(face, size, bold, italic)
        {
            // creating key to load from resources
            string key = string.Format("{0}{1}{2}{3}", face, size, bold ? "b" : "", italic ? "i" : "");
            this.style.font = Resources.Load("fonts/"+key, typeof (Font)) as Font;

            // showing error if font not found
            if (this.style.font==null)
            {
                Debug.LogError("Could not load font: " + key);
            }

            // some tuning
            this.style.wordWrap = false;
            
            // calculating whitesize
            this.content.text = " .";
            this.whiteSize = (int) this.style.CalcSize(this.content).x;
            this.content.text = ".";
            this.whiteSize -= (int)this.style.CalcSize(this.content).x;
        }

        /// <summary>
        /// Space between text lines in pixels
        /// </summary>
        public override int LineSpacing { get { return (int) this.style.lineHeight; } }

        /// <summary>
        /// Space between words
        /// </summary>
        public override int WhiteSize { get { return this.whiteSize; } }

        /// <summary>
        /// Measuring text width and height
        /// </summary>
        /// <param name="text">text to measure</param>
        /// <returns>width and height of measured text</returns>
        public override HtSize Measure(string text)
        {
            this.content.text = text;
            var r = this.style.CalcSize(this.content);
            return new HtSize((int) r.x,(int) r.y);
        }

        /// <summary>
        /// Draw method.
        /// </summary>
        /// <param name="rect">Where to draw</param>
        /// <param name="color">Text color</param>
        /// <param name="text">Text</param>
        public override void Draw(HtRect rect, HtColor color, string text)
        {
            // just common implementation using GUIStyle
            content.text = text;
            style.normal.textColor = new Color32(color.R, color.G, color.B, color.A);
            style.Draw(new Rect(rect.X, rect.Y, rect.Width, rect.Height), content, false, false, false, false);
        }
    }
}