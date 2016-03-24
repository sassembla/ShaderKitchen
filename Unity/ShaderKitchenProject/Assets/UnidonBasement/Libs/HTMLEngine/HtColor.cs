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
using System.Runtime.InteropServices;

namespace HTMLEngine
{
    [StructLayout(LayoutKind.Explicit)]
    public struct HtColor
    {
        public static readonly HtColor transparent = RGBA(0, 0, 0, 0);
        public static readonly HtColor _error = RGBA(0xFF, 0, 0);

        public static readonly HtColor maroon = Parse("#800000");
        public static readonly HtColor red = Parse("#FF0000");
        public static readonly HtColor orange = Parse("#FFA500");
        public static readonly HtColor yellow = Parse("#FFFF00");
        public static readonly HtColor olive = Parse("#808000");
        public static readonly HtColor purple = Parse("#800080");
        public static readonly HtColor fuchsia = Parse("#FF00FF");
        public static readonly HtColor white = Parse("#FFFFFF");
        public static readonly HtColor lime = Parse("#00FF00");
        public static readonly HtColor green = Parse("#008000");
        public static readonly HtColor navy = Parse("#000080");
        public static readonly HtColor blue = Parse("#0000FF");
        public static readonly HtColor aqua = Parse("#00FFFF");
        public static readonly HtColor teal = Parse("#008080");
        public static readonly HtColor black = Parse("#000000");
        public static readonly HtColor silver = Parse("#C0C0C0");
        public static readonly HtColor gray = Parse("#808080");

        [FieldOffset(0)]
        public byte R;
        [FieldOffset(1)]
        public byte G;
        [FieldOffset(2)]
        public byte B;
        [FieldOffset(3)]
        public byte A;
        [FieldOffset(0)]
        public uint RAW;

        public bool IsTransparent
        {
            get { return this.A == 0; }
        }

        public static HtColor RGBA(byte r, byte g, byte b, byte a = 0xFF)
        {
            return new HtColor
            {
                R = r,
                G = g,
                B = b,
                A = a
            };
        }

        private static bool TryParse(string rs, string gs, string bs, ref byte r, ref byte g, ref byte b)
        {
            return byte.TryParse(rs, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out r) &&
                   byte.TryParse(gs, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out g) &&
                   byte.TryParse(bs, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out b);
        }

        private static bool TryParse(string rs, string gs, string bs, string aa, ref byte r, ref byte g, ref byte b, ref byte a)
        {
            return byte.TryParse(rs, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out r) &&
                   byte.TryParse(gs, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out g) &&
                   byte.TryParse(bs, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out b) &&
                   byte.TryParse(aa, NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out a);
        }

        public static HtColor Parse(string text) { return Parse(text, _error); }

        public static HtColor Parse(string text, HtColor onError)
        {
            if (string.IsNullOrEmpty(text)) return onError;
            if (text.StartsWith("#"))
            {
                string rs, gs, bs;
                byte r = 0, g = 0, b = 0;
                switch (text.Length)
                {
                    case 4: // #RGB
                        rs = text.Substring(1, 1);
                        rs += rs;
                        gs = text.Substring(2, 1);
                        gs += gs;
                        bs = text.Substring(3, 1);
                        bs += bs;
                        if (TryParse(rs, gs, bs, ref r, ref g, ref b))
                            return RGBA(r, g, b);
                        break;
                    case 7: // #RRGGBB
                        rs = text.Substring(1, 2);
                        gs = text.Substring(3, 2);
                        bs = text.Substring(5, 2);
                        if (TryParse(rs, gs, bs, ref r, ref g, ref b))
                            return RGBA(r, g, b);
                        break;
                    case 9: // #RRGGBBAA
                        rs = text.Substring(1, 2);
                        gs = text.Substring(3, 2);
                        bs = text.Substring(5, 2);
                        byte a = 0xFF;
                        var aa = text.Substring(7, 2);
                        if (TryParse(rs, gs, bs, aa, ref r, ref g, ref b, ref a))
                            return RGBA(r, g, b, a);
                        break;
                }
            }
            else
            {
                switch (text)
                {
                    case "transparent":
                        return transparent;

                    case "maroon":
                        return maroon;
                    case "red":
                        return red;
                    case "orange":
                        return orange;
                    case "yellow":
                        return yellow;
                    case "olive":
                        return olive;
                    case "purple":
                        return purple;
                    case "fuchsia":
                        return fuchsia;
                    case "white":
                        return white;
                    case "lime":
                        return lime;
                    case "green":
                        return green;
                    case "navy":
                        return navy;
                    case "blue":
                        return blue;
                    case "aqua":
                        return aqua;
                    case "teal":
                        return teal;
                    case "black":
                        return black;
                    case "silver":
                        return silver;
                    case "gray":
                        return gray;
                }
            }

            return onError;
        }

        public override string ToString() { return string.Format("{0:X2}{1:X2}{2:X2}({3:X2})", this.R, this.G, this.B, this.A); }
    }
}