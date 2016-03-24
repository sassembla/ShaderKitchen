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
    internal class HtmlChunkCollection : PList<HtmlChunk>
    {
        internal override void OnRelease()
        {
            this.Clear(true);
            base.OnRelease();
        }

        public void Clear(bool releaseItems = true)
        {
            if (releaseItems)
                foreach (HtmlChunk item in this.list)
                    item.Dispose();
            this.list.Clear();
        }

        public void Read(Reader reader)
        {
            this.Clear();
            while (!reader.IsEof)
            {
                reader.SkipWhitespace();
                if (reader.IsOnChar('<'))
                {
                    var chunk = OP<HtmlChunkTag>.Acquire();
                    if (!chunk.ReadTag(reader))
                    {
                        chunk.Dispose();
                    }
                    else
                    {
                        this.Add(chunk);
                    }
                }
                else
                {
                    var chunk = OP<HtmlChunkWord>.Acquire();
                    if (!chunk.ReadWord(reader))
                    {
                        chunk.Dispose();
                        return;
                    }
                    else
                    {
                        this.Add(chunk);
                    }
                }
            }
        }
    }
}