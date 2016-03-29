using System.Collections.Generic;
using System.Linq;
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
			.SetTagsOfWrappedLine(
				new Dictionary<char, TagPair> {
					//ダブルクォートの色
					{'\"', new TagPair("<color=#ffaa00>", "</color>")}
				}
			)
			
			// ラインの中身の特定文字列の組み合わせを探して、以降の色を変える系
			.SetTagsOfLineContains(
				new Dictionary<string, TagPair> {
					//コメントアウトの色
					{"//", new TagPair("<color=#00ffff>", "</color>")}
				}
			)
			;

			return lines;
		}
		
		private static string SetTagsOfWrappedLine (this string source, Dictionary<char, TagPair> keyAndTag) {
			var newLines = source.Split('\n');
			var lines = source.Split('\n');
			
			foreach (var line in lines.Select((val, index) => new {index, val})) {
				
				var lineText = string.Empty;
				
				foreach (var key in keyAndTag.Keys) {
					/*
						先頭に空白 + keyが来ると、なんかダメ。
					*/
					
					if (line.val.Contains(key)) {
						// this array contains empty head of line.
						var splittedLineArray = line.val.Split(key);
						
						var containedByTopOfLine = line.val.StartsWith(key.ToString());
						
						var pairTopIndex = 0;
						if (!containedByTopOfLine) {
							lineText = splittedLineArray[0];
							pairTopIndex = 1;// 0 element is not wrapped by key. start from 1 of array element.
						}
						
						/*
							pairTopIndexの要素から先を、交互にtag + keyで挟む。
							<TAG>KEYsomething1KEY</TAG>else1<TAG>KEYsomething2KEY</TAG>else2 ...  
						*/
						var skipNext = false;
						for (var i = pairTopIndex; i < splittedLineArray.Length; i++) {
							if (skipNext) {
								skipNext = false;
								lineText += splittedLineArray[i];
								continue;
							}
							
							var part = splittedLineArray[i];
							
							// matched.
							lineText += keyAndTag[key].startTag + key + splittedLineArray[i] + key + keyAndTag[key].endTag;
							skipNext = true;
						}
						newLines[line.index] = lineText;
					} 
				}
			}
			
			return string.Join("\n", newLines);
		}
		
		private static string SetTagsOfLineContains (this string source, Dictionary<string, TagPair> keyAndTag) {
			var newLines = source.Split('\n');
			var lines = source.Split('\n');
			
			foreach (var line in lines.Select((val, index) => new {index, val})) {
				foreach (var key in keyAndTag.Keys) {
					if (line.val.Contains(key)) {
						var index = line.val.IndexOf(key);
						var head = line.val.Substring(0, index);
						var tail = line.val.Substring(index, line.val.Length - index);
						newLines[line.index] = head + keyAndTag[key].startTag + tail + keyAndTag[key].endTag;
					} 
				}
			}
			
			return string.Join("\n", newLines);
		}
		
		private struct TagPair {
			public readonly string startTag;
			public readonly string endTag;
			
			public TagPair (string startTag, string endTag) {
				this.startTag = startTag;
				this.endTag = endTag;
			}	
		}
	}
}