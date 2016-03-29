using MarkdownSharp;
using UnityEngine;

public class WebViewFunction {
	private static Markdown markdownSharp = new Markdown();
	
	public static string MarkdownToRichText (string markdown) {
		var html = markdownSharp.Transform(markdown);
		
		var richtext = HTMLToRichText(html);
		
		return richtext;
	}
	
	
	private static string HTMLToRichText (string html) {
		var lines = html.Replace("<br>", "\n")
			// このへんで、キーワードごとのハイライトも可能。
			.Replace("<p>", "").Replace("</p>", "")
			.Replace("<h2>", "<size=30>").Replace("</h2>", "</size>")
			.Replace("<pre><code class=\"language-\">", "").Replace("</code></pre>", "")
			.Replace("/**/", "")
			;
		
		return lines;
	}
}