﻿/**
 * This file is part of the MarkdownSharp package
 * For the full copyright and license information,
 * view the LICENSE file that was distributed with this source code.
 */

using System;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MarkdownSharp.Extensions.Wiki
{
    /// <summary>
    /// Create short link for https://wikipedia.org
    /// ex: https://en.wikipedia.org/wiki/Southern_Ontario => en_wiki://Southern_Ontario
    /// </summary>
    public class Articles : IExtensionInterface
    {
        private static Regex _wikiArticles = new Regex(@"
                    (?:https?\:\/\/)
                    (?:www\.)?
                    ([a-z]+)
                    \.wikipedia\.org\/wiki\/
                    ([^\^\s\(\)\[\]\<\>]+)", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);


        public string Transform(string text)
        {
            return _wikiArticles.Replace(text, new MatchEvaluator(ArticleEvaluator));
        }


        private string ArticleEvaluator(Match match)
        {
			Debug.LogError("ArticleEvaluator まだ動かないよ！");
            string lang = match.Groups[1].Value;
            string title = "dummy";//WebUtility.UrlDecode(match.Groups[2].Value);

            return String.Format(
                "[{0}_wiki://{1}](https://{0}.wikipedia.org/wiki/{1})",
                lang, title
            );
        }
    }
}
