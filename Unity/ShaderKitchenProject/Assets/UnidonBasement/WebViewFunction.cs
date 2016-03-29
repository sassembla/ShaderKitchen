using System.Collections.Generic;
using MarkdownSharp;
using UnityEngine;

namespace Unidon {
	public static class WebViewFunction {
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

			//追加 青
			.Replace("Shader ","<color=#00aaff>Shader </color>")
			.Replace("SubShader","<color=#00aaff>SubShader</color>")
			.Replace("Properties","<color=#00aaff>Properties</color>")
			.Replace("Tags","<color=#00aaff>Tags</color>")
			.Replace("Pass","<color=#00aaff>Pass</color>")
			.Replace("float4","<color=#00aaff>float4</color>")
			.Replace("float2","<color=#00aaff>float2</color>")
			.Replace("fixed4","<color=#00aaff>fixed4</color>")
			.Replace("fixed2","<color=#00aaff>fixed2</color>")
			.Replace("half4","<color=#00aaff>half4</color>")
			.Replace("half2","<color=#00aaff>half2</color>")
			.Replace("sampler2D","<color=#00aaff>sampler2D</color>")
			.Replace("return","<color=#00aaff>return</color>")
			//追加 緑
			.Replace("struct","<color=#00ffaa>Struct </color>")
			.Replace("Fallback","<color=#00ffaa>Fallback</color>")
			.Replace("tex2D","<color=#00ffaa>tex2D</color>")
			.Replace("mul","<color=#00ffaa>mul</color>")
			.Replace("TRANSFORM_TEX","<color=#00ffaa>TRANSFORM_TEX</color>")
			//追加 紫
			.Replace("CGPROGRAM","<color=#ff33ff>CGPROGRAM</color>")
			.Replace("ENDCG","<color=#ff33ff>ENDCG</color>")
			.Replace("#pragma","<color=#ff33ff>#pragma</color>")
			.Replace("#include","<color=#ff33ff>#include</color>")
			
			// 囲まれてる範囲の色を変える系
			.SetTagsOfWrappedWith(
				//ダブルクォートの色
				new Dictionary<string, string[]> {
					{"\"", new string[]{"<color=#ffaa00>", "</color>"}}
				}
			)
			
			// ラインの先頭をみて色を変える系
			.SetTagsOfLineStartsWith(
				new Dictionary<string, string[]> {
					//コメントアウトの色
					{"//", new string[]{"<color=#00ffff>", "</color>"}}
				}
			)
			;

			return lines;
		}
		
		private static string SetTagsOfWrappedWith (this string source, Dictionary<string, string[]> keyAndTag) {
			// まだ考え中。
			return source;
		}
		
		private static string SetTagsOfLineStartsWith (this string source, Dictionary<string, string[]> keyAndTag) {
			// まだ考え中。
			return source;
		}
	}
}