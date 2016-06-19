using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MarkdownSharp;
using UnityEngine;

namespace Unidon {
	public static class WebViewFunction {
		private static Markdown markdownSharp = new Markdown();
		
		public struct PointsAndRichText {
			public string richText;
			public List<PointInfo> points;

			public PointsAndRichText (string richText, List<PointInfo> points) {
				this.richText = richText;
				this.points = points;
			}
		}

		public static PointsAndRichText MarkdownToRichText (string markdown) {
			var modifiedMarkdownAndPointInfos = GetOperatablePointInfos(markdown);
			
			var html = markdownSharp.Transform(modifiedMarkdownAndPointInfos.modifiedMarkdown);
			
			var richText = HTMLToRichText(html);
			
			return new PointsAndRichText(richText, modifiedMarkdownAndPointInfos.pointInfos);
		}

		private struct MarkdownAndPointInfos {
			public string modifiedMarkdown;
			public List<PointInfo> pointInfos;

			public MarkdownAndPointInfos (string modifiedMarkdown, List<PointInfo> pointInfos) {
				this.modifiedMarkdown = modifiedMarkdown;
				this.pointInfos = pointInfos;
			}
		}

		public struct PointInfo {
			public string id;
			public int startLineCount;
			public int endLineCount;
			// この辺にオプションの種類を列挙したものと、そのオプションのパラメータとかを持ちたい。具体的には
			// 押したら何か起きる、とか。
			// 閉じる、とかもその一種でありたい。

			public PointInfo (string id, int startLineCount, int endLineCount, string options) {
				this.id = id;
				this.startLineCount = startLineCount;
				this.endLineCount = endLineCount;
				Debug.LogWarning("まだオプションどうしようか考えてない。");
			}
		}

		private struct IdentifierAndOptions {
			public string identifier;
			public string options;
			public int startLine;

			public IdentifierAndOptions (string identifier, string options, int startLine) {
				this.identifier = identifier;
				this.options = options;
				this.startLine = startLine;
			}
		}

		private static MarkdownAndPointInfos GetOperatablePointInfos (string sourceMarkdown) {
			var pointInfos = new List<PointInfo>();

			var lines = sourceMarkdown.Split('\n');
			
			/*
				parse operatable identifier.
			*/
			var identifierAndOptions = new List<IdentifierAndOptions>();
			var start = new Regex(@".*[[](.*):(.*)[]]->");
			var end = new Regex(@".*<-(.*)");

			for (var i = 0; i < lines.Length; i++) {
				var line = lines[i];

				// detect start of operatable.
				var startMatch = start.Match(line);
				if (startMatch.Success) {
					var identifier = startMatch.Groups[1].Value;
					var options = startMatch.Groups[2].Value;
					
					identifierAndOptions.Add(new IdentifierAndOptions(identifier, options, i));
					
					/*
						replace line.
					*/
					lines[i] = string.Empty;
				}

				// detect end of operatable.
				var endMatch = end.Match(line);
				if (endMatch.Success) {
					var identifier = endMatch.Groups[1].Value;
					
					var index = identifierAndOptions.FindIndex(record => record.identifier == identifier);
					
					if (index == -1) continue;// no same identifier found! maybe syntax error.

					/*
						same start identifier found.
					*/
					
					var startLine = identifierAndOptions[index].startLine;
					var endLine = i;
					var options = identifierAndOptions[index].options;

					pointInfos.Add(new PointInfo(identifier, startLine, endLine, options));

					/*
						replace target line of markdown.
					*/
					lines[i] = string.Empty;
				}
			}
			var modifiedMarkdown = string.Join("\n", lines);
			
			return new MarkdownAndPointInfos(modifiedMarkdown, pointInfos);
		}
		
		private static string HTMLToRichText (string html) {
			var lines = html.Replace("<br>", "\n")
			
			/*
				基礎サイズとかの設定
			*/
			.Replace("<p>", string.Empty).Replace("</p>", string.Empty)
			.Replace("<h2>", "<size=30>").Replace("</h2>", "</size>")
			.Replace("<pre><code class=\"language-\">", string.Empty).Replace("</code></pre>", string.Empty)
			.Replace("/**/", string.Empty)

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