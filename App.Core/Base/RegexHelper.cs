using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
/*
基本规则

直接量字符，在全面加上\，如
    \f      换页符
    \n      换行符
    \r      回车
    \t      制表符
    \v      垂直制表符
    \/      一个 / 直接量
    \\      一个 \ 直接量
    \.      一个 . 直接量
    \*      一个 * 直接量
    \+      一个 + 直接量
    \?      一个 ? 直接量
    \|      一个 | 直接量
    \(      一个 ( 直接量
    \)      一个 ) 直接量
    \[      一个 [ 直接量
    \]      一个 ] 直接量
    \{      一个 { 直接量
    \}      一个 } 直接量
    \XXX    由十进制数 XXX 指 定的ASCII码字符
    \Xnn    由十六进制数 nn 指定的ASCII码字符
    \cX     控制字符^X. 例如, \cI等价于 \t, \cJ等价于 \n。\cM匹配Ctrl-M
    [\b]    一个退格直接量(特例)
    \1 \2.. 命名组引用。如[a-z]\1匹配aa，bb等重复字符

边界表达式
    ^       匹配一个输入或一行的开头，/^a/匹配"an A"，而不匹配"An a"
    $       匹配一个输入或一行的结尾，/a$/匹配"An a"，而不匹配"an A"
    \b      匹配一个单词的边界(border)。如ion\b  匹配以ion结尾的任何字。可用/b...../b确保边界字符正确
    \B      匹配一个单词的非边界。如\BX\B 匹配字中间的任何X

正则表达式的字符类
    字符    含义                                           例子      可匹配的对象
    ------  ----                                           ----      ------------
    .       除了换行符之外的任意字符,等价于[^\n]           i.ation   isation、ization
    \s      任何空白符,等价于[\t\n\r\f\v]                  \sa       [space]a, \ta, \na（\t和\n与C#的\t和\n含义相同）
    \S      任何非空白符,等价于[^\t\n\r\f\v]               \SF       aF,rF,cF，但不能是\tf
    \w      任何可以组成单词的字符：在服务器端表示为任意Unicode单词字符（如汉字！）[\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Pc}]，在javascript中才等价于[a-zA-Z0-9]。
    \W      任何不可以组成单词的字符
    \d      任何数字,等价于[0-9]
    \D      除了数字之外的任何字符,等价于[^0-9]

数目表达式（正则表达式的复制字符）
    字符         含义
    -----        -----
    {n, m}       匹配前一项至少n次,但是不能超过m次        {1,40}表示1到40个字符
    {n, }        匹配前一项n次,或者多次                   {1,}表示1个以上的字符
    {n}          匹配前一项恰好n次                        {3}表示3个字符
    ?            前导字符可以重复0次或1次,等价于{0,1}     ra?t 可匹配 rt、rat。java(script)?可匹配java或javascript
    *            前导字符可以重复0次或多次.等价于{0,}     ra*t 可匹配 rt、rat、raat等
    +            前导字符可以重复1次或多次,等价于{1,}     ra+t 可匹配 rat、raat等

范围表达式
    用[]括起来
        [...]   位于括号之内的任意字符。如[abcdef]表示a-f的所有字符
        [^...]  不在括号之中的任意字符。如[^abc]表示非abc的所有字符
    可用连字符表示连续的字符范围
        [a-z]         a, b, ......z
        [B-F]         B, C, ......F
        [0-9]         0, 1, ..... 9
        [0-9]+        必须包含一个数字，如9、83和3443
    可用分隔符表示可替换的字符
        ma[n|p]       man 或者 map
        ab|cd|ef]     ab 或 cd 或 ef

分组表达式
    用()括起来
        分组计法          引用计法       匹配成功后的计法
        (\w)                \1           $1
        (?<name>\w)         \k<name>     ${name}
*/
namespace App.Utils
{
    /// <summary>
    /// 正则表达式辅助类。包含常用的正则表达式字符串和正则表达式使用示例
    /// </summary>
    public static class RegexHelper
    {
        //------------------------------
        // Replicate
        //------------------------------
        public static string AA = @"(\w)\1";
        public static string AAA = @"(\w)\1\1";
        public static string AAAA = @"(\w)\1\1\1";
        public static string AAAAA = @"(\w)\1\1\1\1";
        public static string AAAAAA = @"(\w)\1\1\1\1\1";
        public static string AAAAAAA = @"(\w)\1\1\1\1\1\1";
        public static string AABB = @"(\w)\1(\w)\2";
        public static string ABAB = @"(\w)(\w)\1\2";


        //------------------------------
        // Number
        //------------------------------
        public static string Int            = @"^-?\d*$";   // 整数
        public static string PositiveInt    = @"^\d*$";     // 正整数
        public static string NegativeInt    = @"^-\d*$";    // 负整数
        public static string NonPositiveInt = @"^-\d*|0$";  // 非正整数（负数＋0）
        public static string NonNegativeInt = @"^\d*|0$";   // 非负整数（正数＋0）

        public static string Float = @"^-?([1-9]\d*\.\d*|0\.\d*[1-9]\d*|0?\.0+|0)$";            // 浮点数
        public static string PositiveFloat = @"^[1-9]\d*\.\d*|0\.\d*[1-9]\d*$";                 // 正浮点数
        public static string NagativeFloat = @"^-([1-9]\d*\.\d*|0\.\d*[1-9]\d*)$";              // 负浮点数
        public static string NonPositiveFloat = @"^(-([1-9]\d*\.\d*|0\.\d*[1-9]\d*))|0?\.0+|0$";// 非正浮点数（负浮点数 + 0）
        public static string NonNagativeFloat = @"^[1-9]\d*\.\d*|0\.\d*[1-9]\d*|0?\.0+|0$";     // 非负浮点数（正浮点数 + 0）

        public static string Currency = @"-?\d+(\.\d+?)";           // 货币金额
        public static string PositiveCurrency = @"\d+(\.\d+?)";     // 正货币金额

        //------------------------------
        // Date & Time
        //------------------------------
        public static string Time = @"(\d{2}):(\d{2}):(\d{2})";  // 如：13:04:06
        public static string Date = @"(\d{4})-(\d{2})-(\d{2})"; // 如：2003-12-05
        public static string DateTime = @"(\d{4})-(\d{2})-(\d{2}) (\d{2}):(\d{2}):(\d{2})"; // 如：2003-12-05 13:04:06

        //------------------------------
        // Special string
        //------------------------------
        public static string EnglishChar = @"^[A-Za-z]+$";          // 英文字母字符串
        public static string EnglishUpper = @"^[A-Z]+$";            // 大写英文字母字符串
        public static string EnglishLower = @"^[a-z]+$";            // 小写英文字母字符串
        public static string EnglishAndNumber = @"^[A-Za-z0-9]+$";  // 英文字母和数字组成的字符串
        public static string EnglishWord = @"^\w+$";                // 英文字母、数字、下划线组成的字符串

        //------------------------------
        // Web
        //------------------------------
        public static string Email = @"(\w+)@(\w+)\.([a-zA-Z]{2,5})";
        public static string Url = @"^[a-zA-z]+://(\w+(-\w+)*)(\.(\w+(-\w+)*))*(\?\S*)?$";
        public static string IP = @"\d+\.\d+\.\d+\.\d+";
        public static string HtmlTag = @"<(\S*?)[^>]*>.*?</\1>|<.*? />";

        //------------------------------
        // Chinese
        //------------------------------
        public static string ChineseTel = @"\d{3}-\d{8}|\d{4}-\d{7}";   // 国内电话号码。3或4位区号
        public static string ChinesePost = @"^[1-9]\d{5}";              // 中国邮编
        public static string ChineseIdCard = @"\d{15}|\d{18}";          // 身份证号码

        //------------------------------
        // English
        //------------------------------
        public static string EnglishTel = @"^\D?(\d{3})\D?\D?(\d{3})\D?(\d{4})$"; // (425)-555-0123 425-555-0123 425 555 0123
        public static string EnglishPost = @"^(\d{5}-\d{4}|\d{5}|\d{9})$|^([a-zA-Z]\d[a-zA-Z] \d[a-zA-Z]\d)$";  // 5 个或 9 个数字的美国邮政编码

        //------------------------------
        // Char
        //------------------------------
        public static string ChineseChar = @"[\u4e00-\u9fa5]+";         // 中文字符串
        public static string ChineseSymbol = @"[^\uFF00-\uFFFF]+";      // 全角字符
        public static string BiByteChar = @"[\x00-\xff]+";              // 双字节字符（包括中文字符）
        public static string BlankLine = @"\n\s*\r";                    // 空白行
        public static string PreAndFixBlank = @"^\s*|\s*$";             // 首尾空白字符

        //------------------------------
        // Special
        //------------------------------
        public static string Account = @"^[a-zA-Z][a-zA-Z0-9\-]{4,19}$";    // 字母打头，包括字母数字横杠，5到20个字节
        public static string EnglishUserName = @"[a-zA-Z'\-\s]{1,40}";      // 英文姓名名称，包括字母引号横杠，最多40个字节
        public static string QQ = @"^[1-9][0-9]{4,}";                   // QQ号码
        public static string Password = @"^[\w~!@#$%&: \^\*\(\)\[\]\{\}\-\+]{6,20}$"; // 密码：英文字符、特殊字符，6到20位？
        public static string StrongPassword1 = @"^.*(?=.{8,})(?=.*\d)(?=.*[a-zA-Z]).*$";          // 长度大于8，必须包含字母、数字
        public static string StrongPassword2 = @"^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).*$";  // 长度大于8，必须包含大小写字母、数字
        public static string StrongPassword3 = @"^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[~!@#$%&: \^\*\(\)\[\]\{\}\-\+]).*$";  // 长度大于6，必须包含大小写字母、数字、符号
        public static string StrongPassword4 = @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,10}$";        // 长度8-10，必须包含大小写字母和数字
        public static string StrongPassword5 = @"^(?!(?:[^a-zA-Z]|\D|[a-zA-Z0-9])$).{8,}$";       // http://www.cnblogs.com/symbol441/articles/978515.html


        //------------------------------
        // Basic
        //------------------------------
        /// <summary>判断指定字符串和正则表达式是否匹配</summary>
        /// <param name="text">输入字符串</param>
        /// <param name="regex">正则表达式</param>
        /// <returns></returns>
        public static bool IsMatch(string text, string regex, RegexOptions options=RegexOptions.IgnoreCase)
        {
            return new Regex(regex, options).IsMatch(text);
        }

        /// <summary>按照指定正则表达式搜索字符串，并输出匹配的结果字符串</summary>
        /// <param name="text">输入字符串</param>
        /// <param name="regex">供匹配的正则表达式</param>
        /// <param name="part">供输出的匹配部件名称（不填写则返回一个匹配结果）, eg: $1, ${title}</param>
        /// <returns></returns>
        /// <example>
        /// string result = RegexHelper.Search("@<html><title>fdsfdsfds</title></html>", @"<title>(?<title>.*?)</title>", "title");
        /// </example>
        public static string Search(this string text, string regex, string part = null)
        {
            Regex r = new Regex(regex);
            Match m = r.Match(text);
            if (part != null && !part.StartsWith("$"))
                part = "${" + part + "}";
            if (m.Success)
                return (part != null) ? m.Result(part) : m.Value;
            else
                return "";
        }

        /// <summary>将定字符串用匹配正则表达式解析，并用替代正则表达式转换</summary>
        /// <param name="text">输入字符串</param>
        /// <param name="matchRegex">匹配表达式</param>
        /// <param name="replaceRegex">替代表达式</param>
        /// <returns></returns>
        /// <example>
        /// var txt = "03/01/2019";
        /// var day1 = txt.ReplaceRegex(
        ///     @"\b(\d{1,2})/(\d{1,2})/(\d{2,4})\b",
        ///     "$3-$1-$2"
        ///    );
        /// var day2 = txt.ReplaceRegex(
        ///      @"\b(?<month>\d{1,2})/(?<day>\d{1,2})/(?<year>\d{2,4})\b",
        ///      "${year}-${month}-${day}"
        ///     );
        /// </example>
        public static string ReplaceRegex(this string text, string matchRegex, string replaceRegex, RegexOptions options=RegexOptions.IgnoreCase)
        {
            return Regex.Replace(text, matchRegex, replaceRegex, options);
        }

        /// <summary>查找匹配字符串，并自行替换</summary>
        /// <param name="text"></param>
        /// <param name="matchRegex"></param>
        /// <param name="callback">替换方法</param>
        /// <example>
        /// var text1 = "world wororld worororld";
        /// var text2 = text1.ReplaceRegex(@"wor\w*ld", (m) => m.Length.ToString());
        /// Assert.AreEqual(text2, "5 7 9"); 
        /// </example>
        public static string ReplaceRegex(this string text, string matchRegex, Func<Match, string> callback)
        {
            return Regex.Replace(text, matchRegex, new MatchEvaluator(callback), RegexOptions.IgnoreCase);
        }

        //------------------------------
        // Misc
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

        /// <summary>将 mm/dd/yy 的日期形式更换为 yy-mm-dd 的日期形式代替 。 </summary>
        public static string MMDDYY2YYMMDD(string input)
        {
            return Regex.Replace(
                input,
                "\\b(?<month>\\d{1,2})/(?<day>\\d{1,2})/(?<year>\\d{2,4})\\b",
                "${year}-${month}-${day}"
            );
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


        /// <summary>查找所有单词</summary>
        /// <example>
        /// var text = "hello world,你好+世界, '测试+System'";
        /// var list = RegexHelper.FindWords(text);  // hello,world,你好,世界,测试,System
        /// </example>
        public static List<Word> FindWords(string text)
        {
            var words = new List<Word>();
            var pattern = @"\w+";
            var r = new Regex(pattern);
            for (Match m = r.Match(text); m.Success; m = m.NextMatch())
            {
                var txt = m.Value;
                var word = words.FirstOrDefault(t => t.Text == txt);
                if (word != null)
                    word.Count++;
                else
                    words.Add(new Word(txt, 1));
            }
            return words;
        }

        /// <summary>查找重复的单词（请求文本必须用空格或逗号隔开）</summary>
        public static List<Word> FindReplicatedWord(string text)
        {
            /*
            var words = new List<string>();
            //var pattern = @"(?<word>\w+)[\s,]+(\k<word>)";
            var pattern = @"(\w+)([\s,]*\w+)*\1";
            var r = new Regex(pattern);
            for (Match m = r.Match(text); m.Success; m = m.NextMatch())
            {
                var word = m.Result("${word}");
                if (!words.Contains(word))
                    words.Add(word);
            }
            return words;
            */
            return FindWords(text).Search(t => t.Count > 1);
        }


        /// <summary>
        /// 单词（名称、位置、个数）
        /// </summary>
        public class Word
        {
            public string Text { get; set; }
            public int Count { get; set; } = 0;
            public Word(string text, int count)
            {
                this.Text = text;
                this.Count = count;
            }
            public override string ToString()
            {
                return string.Format("{0}({1})", Text, Count);
            }
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

        /// <summary>
        /// Like 语法（类似数据库的like语法）
        /// 注：不支持换行字符串
        /// </summary>
        public static bool Like(string text, string likeExpression)
        {
            // 将%*替换为.* 将_替换为.{1}
            if (likeExpression != null)
            {
                likeExpression = likeExpression.Replace("*", ".*");
                likeExpression = likeExpression.Replace("%", ".*");
                likeExpression = likeExpression.Replace("_", ".{1}");

                Regex regex = new Regex(likeExpression, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                return regex.Match(text).Success;
            }
            return false;
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
