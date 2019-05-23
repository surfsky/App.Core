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

        /// <summary>重复字符串</summary>
        public static string Repeat(this string c, int n)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < n; i++)
                sb.Append(c);
            return sb.ToString();
        }

        /// <summary>安全裁剪字符串（可替代SubString()方法）</summary>
        public static string SubText(this string text, int n)
        {
            if (text.IsEmpty()) return "";
            var l = text.Length;
            if (l <= n)
                return text.Substring(0, l);
            else
                return text.Substring(0, n);
        }

        /// <summary>裁掉尾部的匹配字符串</summary>
        public static string TrimEnd(this string text, string match)
        {
            if (text.IsEmpty()) return "";
            var reg = string.Format("{0}$", match);
            return  Regex.Replace(text, reg, "", RegexOptions.IgnoreCase);
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
            if (text.IsEmpty())
                return "";
            return text.Substring(0, 1).ToLower() + text.Substring(1);
        }

        /// <summary>转化为首字母大写字符串</summary>
        public static string ToHighCamel(this string text)
        {
            if (text.IsEmpty())
                return "";
            return text.Substring(0, 1).ToUpper() + text.Substring(1);
        }


        //--------------------------------------------------
        // 正则表达式处理字符串
        //--------------------------------------------------
        /// <summary>去除 XML 标签（包含注释）</summary>
        public static string RemoveTag(this string text)
        {
            if (text.IsEmpty()) return "";
            text = Regex.Replace(text, "<[^>]*>", "");                                               // 标签
            text = Regex.Replace(text, @"<!–.*", "", RegexOptions.IgnoreCase);                      // 注释头
            text = Regex.Replace(text, @"–>", "", RegexOptions.IgnoreCase);                         // 注释尾
            return text;
        }

        /// <summary>去除脚本标签块</summary>
        public static string RemoveScriptBlock(this string text)
        {
            if (text.IsEmpty()) return "";
            return Regex.Replace(text, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);  // 脚本标签块
        }

        /// <summary>去除样式标签块</summary>
        public static string RemoveStyleBlock(this string text)
        {
            if (text.IsEmpty()) return "";
            return Regex.Replace(text, @"<style[^>]*?>.*?</style>", "", RegexOptions.IgnoreCase);    // 样式标签块
        }

        /// <summary>去除不可见的空白字符（[\t\n\r\f\v]）</summary>
        public static string RemoveBlank(this string text)
        {
            if (text.IsEmpty()) return "";
            return Regex.Replace(text, @"\s+", "", RegexOptions.IgnoreCase);
        }

        /// <summary>去除空白字符转义符（[\t\n\r\f\v]）</summary>
        public static string RemoveBlankTranslator(this string text)
        {
            if (text.IsEmpty()) return "";
            text = Regex.Replace(text, @"\\t+", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\\n+", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\\r+", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\\f+", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\\v+", "", RegexOptions.IgnoreCase);
            return text;
        }

        /// <summary>瘦身：合并多个空白符为一个空格；去除头尾的空格</summary>
        public static string Slim(this string text)
        {
            if (text.IsEmpty()) return "";
            text = Regex.Replace(text, @"\s+", " ", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\s+", " ", RegexOptions.IgnoreCase);
            return text.Trim();
        }

        /// <summary>去除所有HTML痕迹（包括脚本、标签、注释、转义符等）</summary>
        public static string RemoveHtml(this string text)
        {
            if (text.IsEmpty()) return "";

            // 删除标签
            text = Regex.Replace(text, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);  // 脚本标签块
            text = Regex.Replace(text, @"<style[^>]*?>.*?</style>", "", RegexOptions.IgnoreCase);    // 样式标签块
            text = Regex.Replace(text, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);                  // 标签 <form> <div> </div> </form>
            text = Regex.Replace(text, @"<!–.*", "", RegexOptions.IgnoreCase);                      // 注释头
            text = Regex.Replace(text, @"–>", "", RegexOptions.IgnoreCase);                         // 注释尾

            // 处理转义符
            text = Regex.Replace(text, @"&(nbsp|#160);", " ", RegexOptions.IgnoreCase);              // 空格
            text = Regex.Replace(text, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);              // "
            text = Regex.Replace(text, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);                // &
            text = Regex.Replace(text, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);                 // <
            text = Regex.Replace(text, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);                 // >
            text = Regex.Replace(text, @"&(copy|#169);", "©", RegexOptions.IgnoreCase);              // 
            text = Regex.Replace(text, @"&(reg|#174);", "®", RegexOptions.IgnoreCase);               // 
            text = Regex.Replace(text, @"&(deg|#176);", "°", RegexOptions.IgnoreCase);              // 
            //text = Regex.Replace(text, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);           // ￠
            //text = Regex.Replace(text, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);          // ￡
            //text = Regex.Replace(text, @"&(yen|#165);", "￥", RegexOptions.IgnoreCase);              // ￥
            //text = Regex.Replace(text, @"&(middot|#183);", "·", RegexOptions.IgnoreCase);           // 
            //text = Regex.Replace(text, @"&(sect|#167);", "§", RegexOptions.IgnoreCase);             // 
            //text = Regex.Replace(text, @"&(para|#182);", "¶", RegexOptions.IgnoreCase);              // 
            text = Regex.Replace(text, @"&#(\d+);", "", RegexOptions.IgnoreCase);                    // 未知转义符
            //html = Regex.Replace(html, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);               // 换行和空白符

            //
            text.Replace("<", "＜");
            text.Replace(">", "＞");
            text.Replace(" ", " ");
            text.Replace("　", " ");
            text.Replace("/'", "'");
            //text.Replace("/"", """);
            //text = HttpUtility.HtmlDecode(text);
            return text.Trim();
        }


    }
}
