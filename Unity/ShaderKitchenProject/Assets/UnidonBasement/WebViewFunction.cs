using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MarkdownSharp;
using UnityEngine;

namespace Unidon {
	public static class WebViewFunction {
		private static Markdown markdownSharp = new Markdown();
		
		public static string MarkdownToRichText (string markdown) {
			// この時点で、なんか表示されないような抜け道を用意すべきなのか。
			// 

			var html = markdownSharp.Transform(markdown);
			
			var richtext = HTMLToRichText(html);
			
			return richtext;
		}
		
		private static string HTMLToRichText (string html) {
			var lines = html.Replace("<br>", "\n")
			
			/*
				基礎サイズとか
			*/
			.Replace("<p>", "").Replace("</p>", "")
			.Replace("<h2>", "<size=30>").Replace("</h2>", "</size>")
			.Replace("<pre><code class=\"language-\">", "").Replace("</code></pre>", "")
			.Replace("/**/", "")

			/*
				word tagging
			*/
			.SetWordWrappingTag(
				new Dictionary<TagPair, List<string>> {
					// blue
					{new TagPair("<color=#00aaff>", "</color>"), new List<string>{"Shader ", "SubShader", "Properties", "Tags", "Pass", "float4", "float2", "fixed4", "fixed2", "half4", "half2", "sampler2D", "return",}},
					
					// green
					{new TagPair("<color=#00ffaa>", "</color>"), new List<string>{"struct", "Fallback", "tex2D", "mul", "TRANSFORM_TEX",}},
					
					// purple
					{new TagPair("<color=#ff33ff>", "</color>"), new List<string>{"CGPROGRAM", "ENDCG", "#pragma", "#include",}},
				}
			)
			
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
		
		private static string SetWordWrappingTag (this string source, Dictionary<TagPair, List<string>> tagDict) {
			foreach (var tag in tagDict.Keys) {
				foreach (var keyword in tagDict[tag]) {
					source = source.Replace(keyword, tag.startTag + keyword + tag.endTag);
				}
			}
			return source;
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
		
		/**
			web-download function
		*/
		[DllImport("__Internal")] public static extern void DownloadFile(string fileUrl);
		
		/**
			copy function
		*/
		[DllImport("__Internal")] public static extern void CopyToClipboard(string message);
	}
}