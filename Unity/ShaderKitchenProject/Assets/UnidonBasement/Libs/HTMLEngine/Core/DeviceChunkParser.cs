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
    partial class DeviceChunkCollection
    {
        public void Parse(IEnumerator<HtmlChunk> htmlChunks, int viewportWidth, TextAlign align=TextAlign.Left, VertAlign valign=VertAlign.Bottom)
        {
            this.Clear();
            var defaultFont = HtEngine.Device.LoadFont(HtEngine.DefaultFontFace, HtEngine.DefaultFontSize, false, false);
            var font = defaultFont;
            var color = HtEngine.DefaultColor;
            //var align = TextAlign.Left;
            //var valign = VertAlign.Bottom;
            DrawTextDeco deco = DrawTextDeco.None;
            DrawTextEffect effect = DrawTextEffect.None;
            HtColor effectColor = HtEngine.DefaultColor;
            int effectAmount = 1;
            string currentLink = null;

            
            DeviceChunkLine currLine = null;
            DeviceChunkDrawText lastTextChunk=null;

            //for (int i = 0; i < htmlChunks.Count; i++)
            while(htmlChunks.MoveNext())
            {
                HtmlChunk htmlChunk = htmlChunks.Current;

                var word = htmlChunk as HtmlChunkWord;
                if (word != null)
                {
                    if (currLine==null)
                    {
                        currLine = this.NewLine(null, viewportWidth, align, valign);
                    }

                    if (effect == DrawTextEffect.None)
                    {
                        var textChunk = OP<DeviceChunkDrawText>.Acquire();
                        textChunk.Text = word.Text;
                        textChunk.Font = font;
                        textChunk.Color = color;
                        textChunk.Deco = deco;
                        if (lastTextChunk!=null && lastTextChunk.Deco!=deco)
                        {
                            lastTextChunk.DecoStop = true;
                        }
                        textChunk.MeasureSize();
                        lastTextChunk = textChunk;
                    }
                    else
                    {
                        var textChunk = OP<DeviceChunkDrawTextEffect>.Acquire();
                        textChunk.Text = word.Text;
                        textChunk.Font = font;
                        textChunk.Color = color;
                        textChunk.Deco = deco;
                        if (lastTextChunk != null && lastTextChunk.Deco != deco)
                        {
                            lastTextChunk.DecoStop = true;
                        }
                        textChunk.Effect = effect;
                        textChunk.EffectAmount = effectAmount;
                        textChunk.EffectColor = effectColor;
                        textChunk.MeasureSize();
                        lastTextChunk = textChunk;
                    }

                    if (currentLink!=null)
                        this.Links.Add(new KeyValuePair<DeviceChunk, string>(lastTextChunk, currentLink));
                    if (!currLine.AddChunk(lastTextChunk))
                    {
                        currLine.IsFull = true;
                        currLine = this.NewLine(currLine, viewportWidth, align, valign);
                        if (!currLine.AddChunk(lastTextChunk))
                        {
                            HtEngine.Log(HtLogLevel.Error, "Could not fit word into line. Word is too big: {0}", lastTextChunk);
                            lastTextChunk.Dispose();
                            lastTextChunk = null;
                        }
                    }
                }

                var tag = htmlChunk as HtmlChunkTag;
                if (tag != null)
                {
                    switch (tag.Tag)
                    {
                        case "div":
                            if (tag.IsSingle)
                            {
                                // do nothing
                            }
                            else if (tag.IsClosing)
                            {
                                this.FinishLine(currLine,align,valign);
                                return; // return control to parent
                            }
                            else
                            {
                                ExctractAligns(tag, ref align, ref valign);
                                var compiled = OP<DeviceChunkDrawCompiled>.Acquire();
                                compiled.Font = font;
                                var scompiledWidth = tag.GetAttr("width") ?? "0";
                                var compiledWidth = 0;
                                if (!int.TryParse(scompiledWidth, out compiledWidth)) compiledWidth = 0;
                                if (compiledWidth==0)
                                {
                                    compiledWidth = currLine == null ? viewportWidth : currLine.AvailWidth-font.WhiteSize;
                                }
                                if (compiledWidth>0)
                                {
                                    compiled.Parse(htmlChunks, compiledWidth, align, valign);
                                    compiled.MeasureSize();
                                    if (currLine == null)
                                    {
                                        currLine = this.NewLine(null, viewportWidth, align, valign);
                                    }

                                    if (!currLine.AddChunk(compiled))
                                    {
                                        currLine.IsFull = true;
                                        currLine = this.NewLine(currLine, viewportWidth, align, valign);
                                        if (!currLine.AddChunk(compiled))
                                        {
                                            HtEngine.Log(HtLogLevel.Error, "Could not fit div into line. Word is too big: {0}", lastTextChunk);
                                            compiled.Dispose();
                                            compiled = null;
                                        }
                                    }
                                }
                                else
                                {
                                    HtEngine.Log(HtLogLevel.Warning, "div width is not given");
                                }
                            }
                            break;
                        case "effect":
                            if (tag.IsSingle)
                            {
                                // do nothing
                            }
                            else if (tag.IsClosing)
                            {
                                effect = DrawTextEffect.None;
                            }
                            else
                            {
                                var name = tag.GetAttr("name") ?? "outline";
                                switch (name)
                                {
                                    case "shadow":
                                        effect = DrawTextEffect.Shadow;
                                        effectAmount = 1;
                                        effectColor = HtColor.RGBA(0, 0, 0, 80);
                                        break;
                                    case "outline":
                                        effect = DrawTextEffect.Outline;
                                        effectAmount = 1;
                                        effectColor = HtColor.RGBA(0xFF, 0xFF, 0xFF, 80);
                                        break;
                                }
                                var amount = tag.GetAttr("amount");
                                if (amount!=null)
                                {
                                    if (!int.TryParse(amount,out effectAmount))
                                    {
                                        HtEngine.Log(HtLogLevel.Error, "Invalid numeric value: " + amount);
                                    }
                                }
                                var colors = tag.GetAttr("color");
                                if (colors != null) effectColor = HtColor.Parse(colors);
                            }
                            break;
                        case "u":
                            if (tag.IsSingle)
                            {
                                // do nothing
                            }
                            else if (tag.IsClosing)
                            {
                                deco &= ~DrawTextDeco.Underline;
                            }
                            else
                            {
                                deco |= DrawTextDeco.Underline;
                            }
                            break;
                        case "s":
                        case "strike":
                            if (tag.IsSingle)
                            {
                                // do nothing
                            }
                            else if (tag.IsClosing)
                            {
                                deco &= ~DrawTextDeco.Strike;
                            }
                            else
                            {
                                deco |= DrawTextDeco.Strike;
                            }
                            break;
                        case "code":
                            if (tag.IsSingle)
                            {
                                // do nothing
                            }
                            else if (tag.IsClosing)
                            {
                                font = this.fontStack.Count > 0 ? this.fontStack.Pop() : defaultFont;
                            }
                            else
                            {
                                this.fontStack.Push(font);

                                const string fontName = "code";
                                int fontSize = font.Size;
                                bool fontBold = font.Bold;
                                bool fontItal = font.Italic;

                                font = HtEngine.Device.LoadFont(fontName, fontSize, fontBold, fontItal);
                            }
                            break;

                        case "b":
                            if (tag.IsSingle)
                            {
                                // do nothing
                            }
                            else if (tag.IsClosing)
                            {
                                font = this.fontStack.Count > 0 ? this.fontStack.Pop() : defaultFont;
                            }
                            else
                            {
                                this.fontStack.Push(font);

                                string fontName = font.Face;
                                int fontSize = font.Size;
                                const bool fontBold = true;
                                bool fontItal = font.Italic;

                                font = HtEngine.Device.LoadFont(fontName, fontSize, fontBold, fontItal);
                            }
                            break;
                        case "i":
                            if (tag.IsSingle)
                            {
                                // do nothing
                            }
                            else if (tag.IsClosing)
                            {
                                font = this.fontStack.Count > 0 ? this.fontStack.Pop() : defaultFont;
                            }
                            else
                            {
                                this.fontStack.Push(font);

                                string fontName = font.Face;
                                int fontSize = font.Size;
                                bool fontBold = font.Bold;
                                const bool fontItal = true;

                                font = HtEngine.Device.LoadFont(fontName, fontSize, fontBold, fontItal);
                            }
                            break;
                        case "a":
                            if (tag.IsSingle)
                            {
                                // do nothing
                            }
                            else if (tag.IsClosing)
                            {
                                if (this.colorStack.Count>0)
                                    color = this.colorStack.Pop();
                                currentLink = null;
                            }
                            else
                            {
                                currentLink = tag.GetAttr("href");
                                this.colorStack.Push(color);
                                color = HtEngine.DefaultLinkColor;
                            }
                            break;

                        case "font":
                            if (tag.IsSingle)
                            {
                                // do nothing
                            }
                            else if (tag.IsClosing)
                            {
                                font = this.fontStack.Count > 0 ? this.fontStack.Pop() : defaultFont;
                                color = this.colorStack.Count > 0 ? this.colorStack.Pop() : HtEngine.DefaultColor;
                            }
                            else
                            {
                                this.fontStack.Push(font);
                                this.colorStack.Push(color);

                                string fontName = tag.GetAttr("face") ?? font.Face;
                                string fontSizeS = tag.GetAttr("size");
                                int fontSize;
                                if (fontSizeS == null || !int.TryParse(fontSizeS, out fontSize)) fontSize = font.Size;
                                bool fontBold = font.Bold;
                                bool fontItal = font.Italic;

                                font = HtEngine.Device.LoadFont(fontName, fontSize, fontBold, fontItal);

                                color = HtColor.Parse(tag.GetAttr("color"), color);
                            }
                            break;
                        case "br":
                            currLine = this.NewLine(currLine, viewportWidth, align, valign);
                            currLine.Height = font.LineSpacing;
                            break;
                        case "img":
                            if (tag.IsClosing)
                            {
                                // ignore closing tags
                            }
                            else
                            {
                                var src = tag.GetAttr("src");
                                var widthS = tag.GetAttr("width");
                                var heightS = tag.GetAttr("height");
                                int w,h;
                                if (widthS == null || !int.TryParse(widthS, out w)) w = -1;
                                if (heightS == null || !int.TryParse(heightS, out h)) h = -1;
                                var img = HtEngine.Device.LoadImage(src);
                                if (w < 0) w = img.Width;
                                if (h < 0) h = img.Height;
                                var dChunk = OP<DeviceChunkDrawImage>.Acquire();
                                if (currLine == null)
                                    currLine = this.NewLine(null, viewportWidth, align, valign);
                                dChunk.Image = img;
                                dChunk.Rect.Width = w;
                                dChunk.Rect.Height = h;
                                dChunk.Font = font; // for whitespace measure
                                //HtEngine.Log(HtLogLevel.Debug, "Adding image w={0} h={1}",dChunk.Width,dChunk.Height);
                                if (currentLink != null)
                                    this.Links.Add(new KeyValuePair<DeviceChunk, string>(dChunk, currentLink));
                                if (!currLine.AddChunk(dChunk))
                                {
                                    currLine.IsFull = true;
                                    currLine = this.NewLine(currLine, viewportWidth, align, valign);
                                    if (!currLine.AddChunk(dChunk))
                                    {
                                        HtEngine.Log(HtLogLevel.Error, "Could not fit image into line. Image is too big: {0}", dChunk);
                                        dChunk.Dispose();
                                    }
                                }
                            }
                            break;
                        case "p":
                            if (tag.IsClosing)
                            {
                                // ignore closing tags
                            }
                            else
                            {
                                currLine = this.NewLine(currLine, viewportWidth, align, valign);

                                ExctractAligns(tag, ref align, ref valign);
                            }
                            
                            break;
                        default:
                            HtEngine.Log(HtLogLevel.Error, "Unsupported html tag {0}", tag);
                            break;
                    }
                }
            }

            // align last line
            this.FinishLine(currLine, align, valign);
        }

        static void ExctractAligns(HtmlChunkTag tag, ref TextAlign align, ref VertAlign valign)
        {
            var tmp = tag.GetAttr("ALIGN");
            if (tmp != null)
            {
                switch (tmp.ToUpperInvariant())
                {
                    case "CENTER":
                        align = TextAlign.Center;
                        break;
                    case "JUSTIFY":
                        align = TextAlign.Justify;
                        break;
                    case "RIGHT":
                        align = TextAlign.Right;
                        break;
                    case "LEFT":
                        align = TextAlign.Left;
                        break;
                    default:
                        HtEngine.Log(HtLogLevel.Warning, "Invalid attribute align: '{0}'", tmp);
                        align = TextAlign.Left;
                        break;
                }
            }
            else
            {
                //align = TextAlign.Left;
            }
            tmp = tag.GetAttr("VALIGN");
            if (tmp != null)
            {
                switch (tmp.ToUpperInvariant())
                {
                    case "MIDDLE":
                        valign = VertAlign.Middle;
                        break;
                    case "TOP":
                        valign = VertAlign.Top;
                        break;
                    case "BOTTOM":
                        valign = VertAlign.Bottom;
                        break;
                    default:
                        HtEngine.Log(HtLogLevel.Warning, "Invalid attribute valign: '{0}'", tmp);
                        valign = VertAlign.Bottom;
                        break;
                }
            }
            else
            {
                //valign = VertAlign.Bottom;
            }
        }

        /// <summary>
        /// Aligns prev line, creates new line and applies Y to it
        /// </summary>
        /// <param name="prevLine">previous line</param>
        /// <param name="viewPortWidth">viewport width</param>
        /// <param name="prevAlign">text align</param>
        /// <param name="prevVAlign">vertical align</param>
        /// <returns>new empty line</returns>
        internal DeviceChunkLine NewLine(DeviceChunkLine prevLine, int viewPortWidth, TextAlign prevAlign, VertAlign prevVAlign)
        {
            int freeY = 0;
            if (prevLine!=null)
            {
                this.FinishLine(prevLine, prevAlign, prevVAlign);
                freeY = prevLine.Y + prevLine.Height;
            }
            var newLine = OP<DeviceChunkLine>.Acquire();
            newLine.MaxWidth = viewPortWidth;
            newLine.Y = freeY;
            this.list.Add(newLine);
            return newLine;
        }

        internal void FinishLine(DeviceChunkLine line, TextAlign align, VertAlign valign)
        {
            if (line!=null)
            {
                line.HorzAlign(align);
                line.VertAlign(valign);
            }
        }
    }
}
