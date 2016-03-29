using MarkdownSharp;
using UnityEngine;

namespace Unidon {
	public class WebViewFunction {
		private static Markdown markdownSharp = new Markdown();
		
		public static string MarkdownToRichText (string markdown) {
			var html = markdownSharp.Transform(markdown);
			
			var richtext = HTMLToRichText(html);
			
			return richtext;
		}
		
		private static string HTMLToRichText (string html) {
			var lines = html.Replace("<br>", "\n")
				.Replace("<p>", "").Replace("</p>", "")
				.Replace("<h2>", "<size=30>").Replace("</h2>", "</size>")
				.Replace("<pre><code class=\"language-\">", "").Replace("</code></pre>", "")//bgcolorとかつけられそう。 
				.Replace("/**/", "")// 空の行、コントロールブロックとかに使えそうではある
				// .Replace("a", "<color=#ff0000>a</color>")//単語レベルの色変更はOK、抜き出しのロジック書いとくか。
				;
			
			return lines;
		}
	}
}