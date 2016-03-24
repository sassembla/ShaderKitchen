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
using HTMLEngine;
using HTMLEngine.Unity3D;
using UnityEngine;

public class HtmlBehaviour : MonoBehaviour
{
    /// <summary>
    /// Is html text changed?
    /// </summary>
    private bool changed = true;

    /// <summary>
    /// Current html
    /// </summary>
    private string _html = "";

    private const string demo0 =
        @"<p align=center><font face=title size=24><font color=yellow>HTMLEngine</font> for <font color=lime>Unity3D</font> (c) 2012 Profixy</font></p>
<br>
<p align=center><b>This is fully functional engine.<br>It's free to use.<br></b></p>
<br><p align=center>You <b>can</b> <s><font color=gray>not</font></s> edit<br>html inside text area. After all.</p>
<br><p><i>Here is no any &lt;html&gt;, &lt;body&gt; etc tags. Only small subset of tags supported yet:</i></p>
<br>
<p><code>- &lt;p [align=(left|right|center|justify)]<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;[valign=(top|middle|bottom)]&gt;</code></p>
<p><code>- &lt;img src='...' [width=...] [height=...]&gt;</code></p>
<p><code>- &lt;font [face=...] [size=...] [color=...]&gt;</code></p>
<p><code>- &lt;effect name=... [amount=...] [color=...]&gt;</code></p>
<p><code>- &lt;br&gt; &lt;b&gt; &lt;i&gt; &lt;u&gt; &lt;s&gt; &lt;code&gt;</code></p>
<p><code>- &lt;a href='...'&gt;</code></p>
<br>
<p><i>Also this demo contains some internal resources (fonts and images) than can be used.</i></p>
<br>
<p><b>Available fonts:</b> 'default16', 'default16b', 'default16bi', 'default16i', 'title24'</p>
<p><b>Available images:</b> 'smiles/cool', 'smiles/sad', 'smiles/smile', 'smiles/winl', 'logos/unity', 'logos/unity2'</p>
";

    private const string demo1 =
        @"<p align=center><font face=title size=24><font color=yellow>HTMLEngine</font> for <font color=lime>Unity3D</font> (c) 2012 Profixy</font></p>
<p align=left>Without effect:</p>
<p align=center>Normal text<u>underlined</u><s>striked</s></p>
<p align=center><b>Bold text<u>underlined</u><s>striked</s></b></p>
<p align=center><i>Italic text<u>underlined</u><s>striked</s></i></p>
<p align=center><b><i>Bold and italic text<u>underlined</u><s>striked</s></i></b></p>
<p align=left>Shadow effect:</p>
<effect name=shadow color=black>
<p align=center>Normal text<u>underlined</u><s>striked</s></p>
<p align=center><b>Bold text<u>underlined</u><s>striked</s></b></p>
<p align=center><i>Italic text<u>underlined</u><s>striked</s></i></p>
<p align=center><b><i>Bold and italic text<u>underlined</u><s>striked</s></i></b></p>
</effect>
<p align=left>Outline effect:</p>
<effect name=outline color=black>
<p align=center>Normal text <u>underlined</u><s>striked</s></p>
<p align=center><b>Bold text<u>underlined</u><s>striked</s></b></p>
<p align=center><i>Italic text<u>underlined</u><s>striked</s></i></p>
<p align=center><b><i>Bold and italic text<u>underlined</u><s>striked</s></i></b></p>
</effect>
";

    private const string demo2 =
        @"<p align=center><font face=title size=24><font color=yellow>HTMLEngine</font> for <font color=lime>Unity3D</font> (c) 2012 Profixy</font></p>
<font size=24>
<p align=center><effect name=outline><font color=black>Outlined text</font></effect></p>
<p align=center><effect name=outline color=yellow><font color=black>Outlined yellow text</font></effect></p>
<p align=center><effect name=outline color=#FFFFFF80 amount=2>Some stuppid effect i got</effect></p>
<p align=center><effect name=shadow>Default shadowed text</effect></p>
<p align=center><effect name=shadow color=black>Strong-shadowed text</effect></p>
<p align=center><effect name=shadow color=#FFFFFF80 amount=2>Some shadowed text</effect></p>
</font>
    ";

    private const string demo3 =
        @"<p align=center><font face=title size=24><font color=yellow>HTMLEngine</font> for <font color=lime>Unity3D</font> (c) 2012 Profixy</font></p>
<br><p align=justify>Justify aligned text. Justify aligned text. Justify aligned text. Justify aligned text. Justify aligned text. Justify aligned text. Justify aligned text. Justify aligned text.</p>
<br><p align=center><font color=gray>Centered text. Centered text. Centered text. Centered text. Centered text. Centered text. Centered text. Centered text. Centered text. Centered text. Centered text. Centered text.</font></p>
<br><p align=right>Right aligned text. Right aligned text. Right aligned text. Right aligned text. Right aligned text. Right aligned text. Right aligned text. Right aligned text. Right aligned text.</p>
<br><p align=left><font color=gray>Left aligned text. Left aligned text. Left aligned text. Left aligned text. Left aligned text. Left aligned text. Left aligned text. Left aligned text. Left aligned text.</font></p>
        ";

    private const string demo4 =
        @"<p align=center><font face=title size=24><font color=yellow>HTMLEngine</font> for <font color=lime>Unity3D</font> (c) 2012 Profixy</font></p>
<br><p align=center valign=top>Picture <img src='smiles/sad'> with &lt;p valign=top&gt;</p>
<br><p align=center valign=middle>Picture <img src='smiles/smile'> with &lt;p valign=middle&gt; much better than others in this case <img src='smiles/cool'></p>
<br><p align=center valign=bottom>Picture <img src='smiles/sad'> with &lt;p valign=bottom&gt;</p>
<br><p align=justify valign=bottom><img src='logos/unity'> is a feature rich, fully integrated development engine for the creation of interactive 3D content. It provides complete, out-of-the-box functionality to assemble high-quality, high-performing content and publish to multiple platforms.</p>
<br><p align=center><img src='logos/unity2'></p>
        ";

    private const string demo5 =
        @"<p align=center><font face=title size=24><font color=yellow>HTMLEngine</font> for <font color=lime>Unity3D</font> (c) 2012 Profixy</font></p>
<br><p align=center>Now we try to make something dynamic inside text markup...</p>
<br><p align=center valign=middle>Due to performance we can not parse text every frame <img src='smiles/sad'>, but we can reserve some place to draw things inside compiled html! <img src='smiles/cool'></p>
<br><p align=center>It's possible with img tag. Look at source.</p>
<br><p align=center><img src='#time'></p>
<br><p align=center>With same technique we can render animated pictures and even results from render targets.</p>
";

    private const string demo6 =
        @"<p align=center><font face=title size=24><font color=yellow>HTMLEngine</font> for <font color=lime>Unity3D</font> (c) 2012 Profixy</font></p>
<br><p align=center>Links support</p>
<br><p align=left valign=middle>1) <a href='plaintextlink'>Simple plain text link.</a></p>
<br><p align=left valign=middle>2) <a href='textandimage'>Simple text and <img src='smiles/smile'> image link.</a></p>
<br><p align=left valign=middle>3) <a href='biglink'>Multiline link <img src='smiles/smile'>. Multiline link <img src='smiles/smile'>. Multiline link <img src='smiles/smile'>. Multiline link <img src='smiles/smile'>. Multiline link <img src='smiles/smile'>. Multiline link <img src='smiles/smile'>.</a></p>
<br><br><p align=center>Try to click around and see to left-bottom corner for results</p>
<br><br><p align=center>At last we have some basic stuff to interact with player</p>
";

    private const string demo7 =
        @"<p align=center><font face=title size=24><font color=yellow>HTMLEngine</font> for <font color=lime>Unity3D</font> (c) 2012 Profixy</font></p>
<br><br><p align=center>Some stuff using HtmlGUI.Label here</p>
<br><br><p align=center>Below is totally dynamic html text, and statistics for compile and drawing</p>
";

    /// <summary>
    /// bar index
    /// </summary>
    private int bar;

    /// <summary>
    /// bar button texts
    /// </summary>
    private readonly string[] barTexts = new[]
    {"Start", "Text styles", "Text effects", "Para aligns", "Images", "Other", "Links", "HtmlGUI"};

    /// <summary>
    /// bar button htmls
    /// </summary>
    private readonly string[] barHtmls = new[] {demo0, demo1, demo2, demo3, demo4, demo5, demo6, demo7};

    /// <summary>
    /// our html compiler
    /// </summary>
    private HtCompiler compiler;

    /// <summary>
    /// html view x
    /// </summary>
    private const int x = 5;

    /// <summary>
    /// html view y
    /// </summary>
    private const int y = 50;

    /// <summary>
    /// html view width
    /// </summary>
    private readonly int width = Screen.width/2 - x;

    /// <summary>
    /// html view height (calculated by compiler)
    /// </summary>
    private int height;

    /// <summary>
    /// setting text here will raise _changed flag
    /// </summary>
    private string html
    {
        get { return this._html; }
        set
        {
            this._html = value;
            this.changed = true;
        }
    }

    private string currentLink;

    public void Awake()
    {
        Debug.Log("Initializing Demo");

        // our logger
        HtEngine.RegisterLogger(new HtmlLogger());
        // our device
        HtEngine.RegisterDevice(new HtmlDevice());
        // our compiler
        this.compiler = HtEngine.GetCompiler();
        // set up first html
        this.html = demo0;
        
    }

    public void Update()
    {
        if (this.changed)
        {
            // we have new html text, so compile it
            this.compiler.Compile(this._html, this.width);
            // store height of compiled html
            this.height = this.compiler.CompiledHeight;
            // release changed flag
            this.changed = false;
        }
    }

    public void OnGUI()
    {
        float f = 0;
        if (Event.current.type == EventType.Repaint)
            f = Time.realtimeSinceStartup;


        // buttons at top
        this.bar = GUI.Toolbar(new Rect(5, 5, Screen.width - 10, 40), this.bar, this.barTexts);

        // if buttons index changed, set new html
        if (GUI.changed)
            this.html = this.barHtmls[this.bar];

        // show html source at right area
        string tmp = GUI.TextArea(new Rect(x + this.width + x, y, this.width - x, Screen.height - y - x), this.html);
        if (GUI.changed && tmp != this.html)
        {
            // html text changed
            this.html = tmp;
        }

        // only on repaint event we draw our compiled html
        if (Event.current.type == EventType.Repaint)
        {
            var r = new Rect(x, y, this.width, this.height);
            GUI.BeginGroup(r);
            this.compiler.Draw(Time.deltaTime);
            GUI.EndGroup();

            if (bar == 7)
            {
                HtmlGUI.Label(new Rect(10, 300, width, 400),
                              string.Format(@"<p align=center>Dynamic html text! Current time:<br>
<font color=lime><code>{0}</code></font><br><br>
Do not use that often, coz compile time will eat some FPS<br>
Compiling time of this text:<br>
<font color=yellow><code>{1:F4} ms</code></font><br>
And drawing time of this text:<br><font color=yellow><code>{2:F4} ms</code></font></p>", DateTime.Now, HtmlGUI.lastCompilerTookSeconds*1000, HtmlGUI.lastDrawTookSeconds*1000));
            }
        }

        // catch mouseUp to detect links
        if (Event.current.type == EventType.mouseUp)
        {
            // remember we have offset of html container (x,y)
            currentLink = this.compiler.GetLink((int) Event.current.mousePosition.x-x,(int) Event.current.mousePosition.y-y);
            if (currentLink != null)
            {
                Debug.Log("Link clicked: " + currentLink);
            }
            else
            {
                currentLink = "no links here";
                Debug.Log("No links");
            }
        }
        if (Event.current.type == EventType.Repaint)
        {
            f = Time.realtimeSinceStartup - f;
            // show current link onto screen
            GUI.Label(new Rect(10, Screen.height - 30, Screen.width, 30),
                      string.Format("OnGUI time (Repaint): {0:F4}ms. Current link: {1}", f*1000, currentLink));
        }
    }

    public void OnDestroy()
    {
        // we need to dispose compiler to prevent GC
        if (compiler != null)
        {
            compiler.Dispose();
            compiler = null;
        }
    }
}