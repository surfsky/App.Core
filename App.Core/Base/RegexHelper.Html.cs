using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// 正则表达式辅助类（HTML/XML 相关）
    /// </summary>
    public partial class RegexHelper
    {
        //------------------------------
        // Html
        //------------------------------
        /// <summary>解析url，获取协议、主机、端口号</summary>
        public static void ParseUrl(string url, out string proto, out string host, out string port)
        {
            Regex r = new Regex(@"^(?<proto>\w+)://(?<host>[^/:]+):?(?<port>\d+)?/", RegexOptions.Compiled);
            Match m = r.Match(url);
            proto = m.Result("${proto}");
            host = m.Result("${host}");
            port = m.Result("${port}");
        }


        /// <summary>解析&lt;a&gt;标签</summary>
        public static List<KeyValuePair<string, string>> ParseHyperlink(string html)
        {
            List<KeyValuePair<string, string>> coll = new List<KeyValuePair<string, string>>();
            string pattern = @"<a[^>]*?href\s*=\s*(?([""'])(?<url>[^""']+)[""']+|(?<url>[^   ]+))[^>]*>(?<text>[^<]*)</a>";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match m in r.Matches(html))
            {
                string s1 = m.Result("${url}");
                string s2 = m.Result("${text}");
                coll.Add(new KeyValuePair<string, string>(s1, s2));
            }
            return coll;
        }

        /// <summary>解析&lg;title&gt;标签</summary>
        public static string ParseTitle(string html)
        {
            string pattern = @"<title>(?<title>.*?)</title>";
            Regex r = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            return r.Match(html).Result("${title}");
        }

        /// <summary>清除输入文本中的&lg;script&gt;标签</summary>
        public static string ClearScript(string html)
        {
            Regex r = new Regex("(?s)<script.*?>(.*?)</script>", RegexOptions.IgnoreCase);
            return r.Match(html).Result("$1");
        }

        /// <summary>解析url和email用超链接替代</summary>
        public static string ParseHtmlUrl(string html)
        {
            // 匹配并填充http链接
            Regex urlregex = new Regex(@"(http:\/\/([\w.]+\/?)\S*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //html = urlregex.Replace(html, "<a href=\"\" target=\"_blank\"></a>");
            html = urlregex.Replace(html, new MatchEvaluator(BuildUrl));

            // 匹配并填充email链接
            Regex emailregex = new Regex(@"([a-zA-Z_0-9.-]+@[a-zA-Z_0-9.-]+\.\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            //html = emailregex.Replace(html, "<a href=mailto:></a>");
            html = emailregex.Replace(html, new MatchEvaluator(BuildEmail));
            return html;
        }

        static string BuildUrl(Match match)
        {
            return string.Format("<a href=\"{0}\" target=\"_blank\">{0}</a>", match.Groups[1].Value);
        }

        static string BuildEmail(Match match)
        {
            return string.Format("<a href=mailto:{0}>{0}</a>", match.Groups[1].Value);
        }



        //-------------------------------------------
        // 用正则表达式解析Xml标签（仅供参考本类并没有用到）
        //-------------------------------------------
        /// <summary>获取标签列表</summary>
        /// <param name="tagOrContent">获取整个标签还是内容部分</param>
        /// <remarks>有问题：应该获取直接下属标签，而不必返回子级标签；否则若顺序错乱一下，就会取错标签了；</remarks>
        public static List<string> GetXmlTags(string content, string tagName, bool tagOrContent)
        {
            var values = new List<string>();
            if (content.IsNotEmpty())
            {
                var tagRegex = string.Format(@"<{0}[^>]*>([\s\S]*?)</\s*{0}>", tagName);
                var matches = Regex.Matches(content, tagRegex, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    var value = tagOrContent ? match.Value : GetXmlCDATAText(match.Groups[1].Value);
                    values.Add(value);
                }
            }
            return values;
        }

        /// <summary>获取标签</summary>
        public static string GetXmlTag(string content, string tagName, bool tagOrContent)
        {
            var tags = GetXmlTags(content, tagName, tagOrContent);
            return tags.Count > 0 ? tags[0] : null;
        }

        /// <summary>获取指定特性的值（如 Name="Kevin"）</summary>
        public static string GetXmlTagAttribute(string content, string tagName, string attributeName)
        {
            var tagRegex = string.Format(@"<{0}.*{1}\s*=\s*['""]([^'""]*)['""][^>]*>", tagName, attributeName);
            var match = Regex.Match(content, tagRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value;
            return null;
        }


        /// <summary>解析CDATA内容（以<![CDATA[开头)</summary>
        private static string GetXmlCDATAText(string txt)
        {
            var tagRegex = @"^<!\[CDATA\[(.*)\]\]>";
            var match = Regex.Match(txt.TrimStart(), tagRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value;
            else
                return txt;
        }

    }
}
