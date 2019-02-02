using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;

namespace App.Core
{
    /// <summary>
    /// 字符串操作辅助类
    /// </summary>
    public static class StringHelper
    {
        //--------------------------------------------------
        // 为空
        //--------------------------------------------------
        /// <summary>字符串是否为空</summary>
        public static bool IsEmpty(this string txt)
        {
            return String.IsNullOrEmpty(txt);
        }

        /// <summary>字符串是否为空</summary>
        public static bool IsNotEmpty(this string txt)
        {
            return !String.IsNullOrEmpty(txt);
        }

        /// <summary>对象是否为空或为空字符串</summary>
        public static bool IsEmpty(this object o)
        {
            return (o == null) ? true : o.ToString().IsEmpty();
        }

        /// <summary>对象是否为空或为空字符串</summary>
        public static bool IsNotEmpty(this object o)
        {
            return !o.IsEmpty();
        }

        /// <summary>获取字符串 MD5 哈希值（32位）</summary>
        /// <param name="text"></param>
        /// <returns>字符串MD5哈希值的十六进制字符串</returns>
        public static string ToMD5(this string text, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(encoding.GetBytes(text));

            var sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
                sb.AppendFormat("{0:x2}", bytes[i]);
            return sb.ToString();
        }


        //--------------------------------------------------
        // 
        //--------------------------------------------------
        /// <summary>生成随机文本</summary>
        /// <param name="chars">字符集合</param>
        /// <param name="length">要生成的文本长度</param>
        public static string BuildRandomText(string chars = "0123456789", int length = 6)
        {
            var sb = new StringBuilder();
            var rnd = new Random(BuildRandomSeed());
            for (int i = 0; i < length; i++)
            {
                var index = rnd.Next(chars.Length);
                sb.Append(chars[index]);
            }
            return sb.ToString();
        }
        static int BuildRandomSeed()
        {
            var bytes = new byte[4];
            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>Js格式化</summary>
        public static int[] ToIntArray(this string commaText)
        {
            if (String.IsNullOrEmpty(commaText))
                return new int[0];
            else
                return commaText.Split(',').Select(s => Convert.ToInt32(s)).ToArray();
        }


        /// <summary>去除HTML标签</summary>
        public static string RemoveTag(this string html)
        {
            return Regex.Replace(html, "<[^>]*>", "");
        }

        /// <summary>去除脚本标签</summary>
        public static string RemoveScriptTag(this string html)
        {
            return Regex.Replace(html, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);  // 脚本标签块
        }

        /// <summary>去除脚本标签</summary>
        public static string RemoveStyleTag(this string html)
        {
            return Regex.Replace(html, @"<style[^>]*?>.*?</style>", "", RegexOptions.IgnoreCase);    // 样式标签块
        }

        /// <summary>去除所有HTML痕迹（包括脚本、标签、注释、转义符等）</summary>
        public static string RemoveHtml(this string html)
        {
            if (html.IsEmpty()) return "";

            // 删除标签
            html = Regex.Replace(html, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);  // 脚本标签块
            html = Regex.Replace(html, @"<style[^>]*?>.*?</style>", "", RegexOptions.IgnoreCase);    // 样式标签块
            html = Regex.Replace(html, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);                  // 标签 <form> <div> </div> </form>
            html = Regex.Replace(html, @"<!–.*", "", RegexOptions.IgnoreCase);                      // 注释头
            html = Regex.Replace(html, @"–>", "", RegexOptions.IgnoreCase);                         // 注释尾

            // 处理转义符
            html = Regex.Replace(html, @"&(nbsp|#160);", " ", RegexOptions.IgnoreCase);              // 空格
            html = Regex.Replace(html, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);              // "
            html = Regex.Replace(html, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);                // &
            html = Regex.Replace(html, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);                 // <
            html = Regex.Replace(html, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);                 // >
            html = Regex.Replace(html, @"&(copy|#169);", "©", RegexOptions.IgnoreCase);              // 
            html = Regex.Replace(html, @"&(reg|#174);", "®", RegexOptions.IgnoreCase);               // 
            html = Regex.Replace(html, @"&(deg|#176);", "°", RegexOptions.IgnoreCase);              // 
            //html = Regex.Replace(html, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);           // ￠
            //html = Regex.Replace(html, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);          // ￡
            //html = Regex.Replace(html, @"&(yen|#165);", "￥", RegexOptions.IgnoreCase);              // ￥
            //html = Regex.Replace(html, @"&(middot|#183);", "·", RegexOptions.IgnoreCase);           // 
            //html = Regex.Replace(html, @"&(sect|#167);", "§", RegexOptions.IgnoreCase);             // 
            //html = Regex.Replace(html, @"&(para|#182);", "¶", RegexOptions.IgnoreCase);              // 
            html = Regex.Replace(html, @"&#(\d+);", "", RegexOptions.IgnoreCase);                    // 未知转义符
            //html = Regex.Replace(html, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);               // 换行和空白符

            //
            html.Replace("<", "＜");
            html.Replace(">", "＞");
            html.Replace(" ", " ");
            html.Replace("　", " ");
            html.Replace("/'", "'");
            //html.Replace("/"", """);
            html = HttpUtility.HtmlEncode(html).Trim();
            return html;
        }

        /// <summary>重复字符串</summary>
        public static string Repeat(this string c, int n)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < n; i++)
                sb.Append(c);
            return sb.ToString();
        }

        /// <summary>安全裁剪字符串</summary>
        public static string Clip(this string text, int n)
        {
            var l = text.Length;
            if (l <= n)
                return text.Substring(0, l);
            else
                return text.Substring(0, n);
        }

        /// <summary>获取遮罩文本（XXXXXXXXXX****XXXX）</summary>
        /// <param name="n">文本最终长度</param>
        /// <param name="mask">遮罩字符（默认为.）</param>
        public static string Mask(this string text, int n, string mask="*")
        {
            if (text.IsEmpty() || text.Length < n)
                return text;
            else
            {
                int len = text.Length;
                string masks = mask.Repeat(4);
                return text.Substring(0, len - 8) + masks + text.Substring(n - 4, 4);
            }
        }

        /// <summary>获取摘要。格式如 xxxxxx... </summary>
        public static string Summary(this string text, int n)
        {
            if (text.IsEmpty() || text.Length < n)
                return text;
            else
                return text.Substring(0, n) + "....";
        }

        /// <summary>转化为首字母小写字符串</summary>
        public static string ToLowCamel(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            return text.Substring(0, 1).ToLower() + text.Substring(1);
        }

        /// <summary>转化为首字母大写字符串</summary>
        public static string ToHighCamel(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            return text.Substring(0, 1).ToUpper() + text.Substring(1);
        }
    }
}
