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

using System;
using System.Collections.Generic;

namespace HTMLEngine.Core
{
    internal class HtmlChunkTag : HtmlChunk
    {
        private static readonly char[] TAG_NAME_STOP_CHARS = new[] {' ', '/', '>'};
        private static readonly char[] ATTR_NAME_STOP_CHARS = new[] {' ', '='};
        private static readonly char[] ATTR_VALUE_STOP_CHARS = new[] {' ', '/', '>'};

        internal override void OnRelease()
        {
            this.Attrs.Clear();
            base.OnRelease();
        }

        readonly Dictionary<string, string> Attrs =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public string GetAttr(string attrName)
        {
            string r;
            return Attrs.TryGetValue(attrName, out r) ? r : null;
        }

        public bool IsClosing;
        public bool IsSingle;
        public string Tag;

        public bool ReadTag(Reader reader)
        {
            reader.AutoSkipWhitespace = true;
            reader.SkipWhitespace();
            if (!reader.IsOnChar('<')) return false;
            reader.Skip(1);
            this.IsClosing = false;
            if (reader.CurrChar == '/')
            {
                this.IsClosing = true;
                reader.Skip(1);
            }
            this.Tag = reader.ReadToStopChar(TAG_NAME_STOP_CHARS);

            while (reader.IsOnLetter())
            {
                string attrName = reader.ReadToStopChar(ATTR_NAME_STOP_CHARS);
                reader.ReadToStopChar('=');
                reader.Skip(1);
                string attrValue = reader.IsOnQuote()
                                       ? reader.ReadQuotedString()
                                       : reader.ReadToStopChar(ATTR_VALUE_STOP_CHARS);
                this.Attrs[attrName] = attrValue;
                reader.SkipWhitespace();
            }

            switch (this.Tag)
            {
                case "br":
                case "hr":
                case "img":
                case "meta":
                    this.IsSingle = true;
                    break;
                default:
                    this.IsSingle = reader.CurrChar == '/';
                    break;
            }
            reader.ReadToStopChar('>');
            if (reader.CurrChar != '>') return false;
            else reader.Skip(1);
            return IsTagSupported();
        }

        bool IsTagSupported()
        {
            // testing for supported tags
            switch (Tag)
            {
                case "a":
                case "img":
                case "p":
                case "div":
                case "br":
                case "font":
                case "code":
                case "b":
                case "i":
                case "u":
                case "s":
                case "strike":
                case "effect":
                    return true;
                default:
                    HtEngine.Log(HtLogLevel.Warning, "Ignoring unsupported tag: " + Tag);
                    return false;
            }
        }


        public override string ToString() { return string.Format("<{0}>", IsClosing ? "/" + Tag : Tag); }
    }
}