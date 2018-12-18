using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;

namespace App.Core
{
    /// <summary>
    /// 负责各种类型转换、列表类型转换
    /// </summary>
    public static class Convertor
    {
        //--------------------------------------------------
        // 类型转换
        //--------------------------------------------------
        /// <summary>将可空对象转化为字符串</summary>
        public static string ToText(this object o, string format="{0}")
        {
            //return o == null ? "" : o.ToString();
            return string.Format(format, o);
        }

        /// <summary>将可空bool对象转化为字符串</summary>
        public static string ToText(this bool? o, string trueText = "true", string falseText = "false")
        {
            return o == null
                ? ""
                : (o.Value ? trueText : falseText)
                ;
        }

        /// <summary>将可空对象转化为整型字符串</summary>
        public static string ToIntText(this object o)
        {
            if (o == null) return "";
            else return Convert.ToInt32(o).ToString();
        }

        /// <summary>将可空对象转化为整型</summary>
        public static int ToInt32(this object o)
        {
            if (o == null) return -1;
            else return Convert.ToInt32(o);
        }

        /// <summary>将可空对象转化为时间类型</summary>
        public static DateTime ToDateTime(this object o)
        {
            return o == null ? new DateTime() : DateTime.Parse(o.ToString());
        }

        //--------------------------------------------------
        // 转化为文本
        //--------------------------------------------------
        /// <summary> url编码</summary> 
        public static string ToUrlEncode(this string o)
        {
            return HttpUtility.UrlEncode(o);
        }

        /// <summary> url解码</summary> 
        public static string ToUrlDecode(this string o)
        {
            return HttpUtility.UrlDecode(o);
        }

        /// <summary>转化为逗号分隔的字符串</summary>
        public static string ToCommaString(this IEnumerable source)
        {
            if (source == null)
                return "";
            string txt = "";
            foreach (var item in source)
                txt += item.ToString() + ",";
            return txt.TrimEnd(',');
        }

        /// <summary>获取Xml安全文本</summary>
        public static string ToXmlSafeText(this object obj)
        {
            if (obj == null)
                return "";

            // "<" 字符和"&"字符对于XML来说是严格禁止使用的，此处用CDATA解决
            var txt = obj.ToString();
            if (txt.IndexOfAny(new char[] { '<', '&' }) != -1)
                return string.Format("<![CDATA[ {0} ]]>", txt);
            return txt;
        }


        //--------------------------------------------------
        // 列表处理
        //--------------------------------------------------
        /// <summary>数组转化为列表</summary>
        public static List<T> ToList<T>(params T[] steps)
        {
            List<T> items = new List<T>();
            foreach (var step in steps)
                items.Add(step);
            return items;
        }

        /// <summary>字符串转化为枚举（支持枚举字符串或数字）</summary>
        public static T? ToEnum<T>(this string o) where T : struct
        {
            if (!string.IsNullOrEmpty(o))
            {
                if (RegexHelper.IsMatch(o, RegexHelper.Integer))
                {
                    int n = int.Parse(o);
                    return (T)Enum.ToObject(typeof(T), n);
                }
                else
                {
                    Enum.TryParse<T>(o, true, out T result);
                    return result;
                }
            }
            return null;
        }

        /// <summary>转化为整型列表</summary>
        public static List<int> CastInt(this IEnumerable source)
        {
            return source.Cast<int>(t => 
                t.IsEnum() 
                    ? Convert.ToInt32(t) 
                    : int.Parse(t.ToString())
                    );
        }

        /// <summary>转化为整型列表</summary>
        public static List<string> CastString(this IEnumerable source)
        {
            return source.Cast<string>(t => t.ToString());
        }

        /// <summary>转化为枚举列表</summary>
        public static List<T> CastEnum<T>(this IEnumerable source) where T : struct
        {
            return source.Cast<T>(t => (T)Enum.ToObject(typeof(T), Convert.ToInt32(t)));
        }

        /// <summary>遍历并转换</summary>
        public static List<T> Cast<T>(this IEnumerable source, Func<object, T> func)
        {
            var result = new List<T>();
            foreach (var item in source)
                result.Add(func(item));
            return result;
        }

        /// <summary>遍历并转换</summary>
        public static List<object> Cast<T>(this IEnumerable<T> source, Func<T, object> func)
        {
            return Cast<T, object>(source, func);
        }

        /// <summary>遍历并转换</summary>
        public static List<TOut> Cast<T, TOut>(this IEnumerable<T> source, Func<T, TOut> func)
        {
            var result = new List<TOut>();
            foreach (var item in source)
                result.Add(func(item));
            return result;
        }
    }
}