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
using System.Text;

namespace HTMLEngine.Core
{
    internal delegate bool ReaderDelegate(Reader reader);

    internal class Reader
    {
        public static readonly Reader Instance = new Reader();

        /// <summary>
        /// Ignore comments like /*...*/
        /// </summary>
        public bool AutoSkipMLComments = false;

        /// <summary>
        /// Ignore comments like //...
        /// </summary>
        public bool AutoSkipSLComments = false;

        /// <summary>
        /// Ignore whitespace
        /// </summary>
        public bool AutoSkipWhitespace = false;

        private string text;
        private int begin;
        private int end;
        private int curr;
        private readonly StringBuilder sb;

        public Reader()
        {
            //begin = end = curr = null;
            sb = new StringBuilder(100);
        }

        public string CurrentText { get { return this.IsEof ? "(eof)" : text.Substring(curr); } }

        public bool IsEof { get { return curr >= end; } }

        public long Length { get { return end - begin; } }

        public long Position { get { return curr - begin; } }

        public long Rest { get { return end - curr; } }

        public void SetSource(string text)
        {
            this.text = text;
            begin = 0;
            end = begin + text.Length;
            curr = begin;
        }

//        public bool Run(ReaderDelegate work, string text) { return Run(work, text, 0, text.Length); }
//
//        public bool Run(ReaderDelegate work, string text, int startIndex) { return Run(work, text, startIndex, text.Length - startIndex); }
//
//        public bool Run(ReaderDelegate work, string text, int startIndex, int length)
//        {
//            if (text != null)
//            {
//                throw new ApplicationException("Reader is already running");
//            }
//            
//            fixed (char* p = text)
//            {
//                this.begin = p + startIndex;
//                this.end = begin + length;
//                this.curr = this.begin;
//                var ret = work(this);
//                begin = end = curr = null;
//                return ret;
//            }
//
//        }

        public char CurrChar { get { return curr >= begin && curr < end ? text[curr] : default(char); } }
        public char NextChar { get { return curr < end - 1 ? text[curr + 1] : default(char); } }
        public char PrevChar { get { return curr > begin ? text[curr - 1] : default(char); } }


        void DoAutoSkip()
        {
            if (AutoSkipWhitespace) this.SkipWhitespace();
            while (curr < end)
            {
                int tmp = 0;
                if (AutoSkipSLComments)
                {
                    while (this.IsOnSLComments())
                    {
                        this.SkipToChar('\n');
                        if (AutoSkipWhitespace) this.SkipWhitespace();
                        tmp++;
                    }
                }
                if (AutoSkipMLComments)
                {
                    while (this.IsOnMLComments())
                    {
                        while (this.SkipToChar('*') && CurrChar == '/')
                        {
                            curr++;
                        }
                        if (AutoSkipWhitespace) this.SkipWhitespace();
                        tmp++;
                    }
                }
                if (tmp == 0) break; // no any comments found
            }
        }

        /// <summary>
        /// Skip to given char
        /// </summary>
        /// <param name="c">Given char</param>
        /// <param name="thenSkipThisChar">If true, skip given char too</param>
        /// <returns>If true, EOF is not reached</returns>
        public bool SkipToChar(char c, bool thenSkipThisChar = true)
        {
            this.DoAutoSkip();
            while (curr < end)
            {
                if (CurrChar == c)
                {
                    if (thenSkipThisChar) curr++;
                    break;
                }
            }
            this.DoAutoSkip();
            return curr < end;
        }

        public void Skip(int count)
        {
            curr += count;
            this.DoAutoSkip();
        }

        public bool SkipWhitespace()
        {
            var tmp = curr;
            while (this.IsOnWhitespace())
                curr++;
            return curr > tmp;
        }

        public string ReadToStopChar(char stopChar, bool ignoreCase = false)
        {
            this.DoAutoSkip();
            sb.Length = 0;
            while (curr < end)
            {
                var ch = CurrChar;
                if (CompareChars(ch, stopChar, ignoreCase))
                {
                    this.DoAutoSkip();
                    return sb.ToString();
                }
                sb.Append(ch);
                curr++;
            }
            this.DoAutoSkip();
            return sb.ToString();
        }

        public string ReadToStopChar(char[] chars, bool ignoreCase = false)
        {
            this.DoAutoSkip();
            sb.Length = 0;
            while (curr < end)
            {
                char c = CurrChar;
                for (int i = 0; i < chars.Length; i++)
                    if (CompareChars(c, chars[i], ignoreCase))
                    {
                        this.DoAutoSkip();
                        return sb.ToString();
                    }
                sb.Append(CurrChar);
                curr++;
            }
            this.DoAutoSkip();
            return sb.ToString();
        }

        public string ReadToStopText(string stopText, bool ignoreCase = false)
        {
            this.DoAutoSkip();
            char stopChar = stopText[0];
            sb.Length = 0;
            while (curr < end)
            {
                char c = CurrChar;
                if (CompareChars(c, stopChar, ignoreCase) && this.IsOnText(stopText))
                {
                    this.DoAutoSkip();
                    return sb.ToString();
                }
                sb.Append(c);
                curr++;
            }
            this.DoAutoSkip();
            return sb.ToString();
        }

        public string ReadToWhitespace()
        {
            this.DoAutoSkip();
            sb.Length = 0;
            while (curr < end)
            {
                if (this.IsOnWhitespace()) return sb.ToString();
                sb.Append(CurrChar);
                curr++;
            }
            this.DoAutoSkip();
            return sb.ToString();
        }

        public string ReadToWhitespaceOrChar(char c)
        {
            this.DoAutoSkip();
            sb.Length = 0;
            while (curr < end)
            {
                if (this.IsOnWhitespace() || CurrChar==c) return sb.ToString();
                sb.Append(CurrChar);
                curr++;
            }
            this.DoAutoSkip();
            return sb.ToString();
        }


        public string ReadQuotedString()
        {
            this.DoAutoSkip();
            sb.Length = 0;
            if (curr < end)
            {
                char q = CurrChar; curr++;
                while (curr < end)
                {
                    char c = CurrChar; curr++;
                    if (c == '\\' && CurrChar == q)
                    {
                        c = CurrChar; curr++;
                    }
                    else if (c == q)
                    {
                        return sb.ToString();
                    }
                    sb.Append(c);
                }
            }
            this.DoAutoSkip();
            return sb.ToString();
        }

        public bool IsOnSLComments() { return curr >= begin && curr < end - 1 && CurrChar == '/' && NextChar == '/'; }
        public bool IsOnMLComments() { return curr >= begin && curr < end - 1 && CurrChar == '/' && NextChar == '*'; }
        public bool IsOnWhitespace() { return curr < end && CurrChar <= ' '; }
        public bool IsOnQuote() { return curr < end && CurrChar == '\'' || CurrChar == '"'; }
        public bool IsOnDigit() { return curr < end && char.IsDigit(CurrChar); }
        public bool IsOnLetter() { return curr < end && char.IsLetter(CurrChar); }
        public bool IsOnLetterOrDigit() { return curr < end && char.IsLetterOrDigit(CurrChar); }
        public bool IsOnChar(char c, bool ignoreCase = false) { return curr < end && CompareChars(CurrChar, c, ignoreCase); }
        public bool IsOnChar(char[] chars, bool ignoreCase = false)
        {
            if (curr < end)
            {
                char c = CurrChar;
                for (int i = 0; i < chars.Length; i++)
                {
                    if (CompareChars(c, chars[i], ignoreCase)) return true;
                }
            }
            return false;
        }

        public bool IsOnText(string text, bool ignoreCase = false)
        {
            var len = text.Length;
            if (Rest < len) return false;
            return text.IndexOf(text, curr, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture) == curr;
//            fixed (char* pText = text)
//            {
//                char* p1 = pText;
//                char* p2 = curr;
//                for (int i = 0; i < len; i++)
//                {
//                    char c1 = *p1++;
//                    char c2 = *p2++;
//                    if (!CompareChars(c1, c2, ignoreCase)) return false;
//                }
//            }
//            return true;
        }

        static bool CompareChars(char c1, char c2, bool ignoreCase) { return ignoreCase ? char.ToUpperInvariant(c1) == char.ToUpperInvariant(c2) : c1 == c2; }

    }
}
