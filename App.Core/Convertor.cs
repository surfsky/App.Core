using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace App.Core
{
    /// <summary>
    /// 负责各种类型转换、列表类型转换
    /// ParseXXXX(string) 负责将字符串解析为对应的类型
    /// ToXXX()           负责将各种数据类型相互转换
    /// CastXXX()         负责列表元素的遍历、转换、筛选
    /// </summary>
    public static partial class Convertor
    {
        //--------------------------------------------------
        // 将文本解析为值或对象
        //--------------------------------------------------
        /// <summary>将文本转化为简单值</summary>
        public static object Parse<T>(this string text) where T : struct
        {
            return text.Parse(typeof(T));
        }
        /// <summary>将文本转化为简单值</summary>
        public static object Parse(this string text, Type type)
        {
            if (type == typeof(string))
                return text;

            if (type.IsNullable())
            {
                type = type.GetRealType();
                if (type == typeof(int))      return text.ParseInt();
                if (type == typeof(long))     return text.ParseLong();
                if (type == typeof(float))    return text.ParseFloat();
                if (type == typeof(double))   return text.ParseDouble();
                if (type == typeof(decimal))  return text.ParseDecimal();
                if (type == typeof(short))    return text.ParseShort();
                if (type == typeof(bool))     return text.ParseBool();
                if (type == typeof(DateTime)) return text.ParseDate();
                if (type.IsEnum())            return text.ParseEnum(type);
            }
            else
            {
                if (type == typeof(int))      return text.ParseInt().Value;
                if (type == typeof(long))     return text.ParseLong().Value;
                if (type == typeof(float))    return text.ParseFloat().Value;
                if (type == typeof(double))   return text.ParseDouble().Value;
                if (type == typeof(decimal))  return text.ParseDecimal().Value;
                if (type == typeof(short))    return text.ParseShort().Value;
                if (type == typeof(bool))     return text.ParseBool().Value;
                if (type == typeof(DateTime)) return text.ParseDate().Value;
                if (type.IsEnum())            return text.ParseEnum(type);
            }

            return text;
        }


        /// <summary>字符串解析为枚举（支持枚举字符串或数字，遇到）</summary>
        public static T? ParseEnum<T>(this string o) where T : struct
        {
            if (o.IsNotEmpty())
            {
                try
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
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public static object ParseEnum(this string text, Type enumType)
        {
            try
            {
                return text.IsEmpty() ? null : Enum.Parse(enumType, text, true);
            }
            catch
            {
                return null;
            }
        }

        public static DateTime? ParseDate(this string text)
        {
            return text.IsEmpty() ? null : new DateTime?(DateTime.Parse(text));
        }

        public static decimal? ParseDecimal(this string text)
        {
            return text.IsEmpty() ? null : new decimal?(decimal.Parse(text));
        }

        public static double? ParseDouble(this string text)
        {
            return text.IsEmpty() ? null : new double?(double.Parse(text));
        }

        public static float? ParseFloat(this string text)
        {
            return text.IsEmpty() ? null : new float?(float.Parse(text));
        }

        public static long? ParseLong(this string text)
        {
            return text.IsEmpty() ? null : new Int64?(Int64.Parse(text));
        }

        public static int? ParseInt(this string text)
        {
            return text.IsEmpty() ? null : new Int32?(Int32.Parse(text));
        }

        public static short? ParseShort(this string txt)
        {
            return txt.IsEmpty() ? null : new Int16?(Int16.Parse(txt));
        }

        public static bool? ParseBool(this string text)
        {
            return text.IsEmpty() ? null : new bool?(Boolean.Parse(text));
        }

        /// <summary>解析查询字符串（如id=1&amp;name=Kevin）为字典</summary>
        public static FreeDictionary<string, string> ParseDict(this string text)
        {
            var dict = new FreeDictionary<string, string>();
            var regex = new Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", RegexOptions.Compiled);
            var matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                var key = match.Result("$2");
                var value = match.Result("$3");
                dict.Add(key, value);
            }
            return dict;
        }



        //--------------------------------------------------
        // 类型互相转换
        //--------------------------------------------------
        /// <summary>将可空对象转化为字符串</summary>
        public static string ToText(this object o, string format="")
        {
            if (format != null && !format.Contains("{"))
                format = "{0:" + format + "}";
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
            return Convert.ToInt32(o).ToString();
        }

        /// <summary>对象转换为字节数组</summary>
        public static byte[] ToBytes(this object o)
        {
            if (o == null)
                return null;
            else
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter ser = new BinaryFormatter();
                ser.Serialize(ms, o);
                byte[] bytes = ms.ToArray();
                ms.Close();
                return bytes;
            }
        }

        /// <summary>字符串转换为字节数组</summary>
        public static byte[] ToBytes(this string txt, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return txt.IsEmpty() ? new byte[0] : encoding.GetBytes(txt);
        }

        /// <summary>将可空对象转化为整型</summary>
        public static int? ToInt(this object o)
        {
            if (o.IsEmpty()) return null;
            return Convert.ToInt32(o);
        }

        /// <summary>将可空对象转化为Float</summary>
        public static float? ToFloat(this object o)
        {
            if (o.IsEmpty()) return null;
            return Convert.ToSingle(o);
        }

        /// <summary>将可空对象转化为Double</summary>
        public static double? ToDouble(this object o)
        {
            if (o.IsEmpty()) return null;
            return Convert.ToDouble(o);
        }

        /// <summary>将可空对象转化为时间类型</summary>
        public static bool? ToBool(this object o)
        {
            if (o.IsEmpty()) return null;
            return Boolean.Parse(o.ToString());
        }

        /// <summary>将可空对象转化为时间类型</summary>
        public static DateTime? ToDateTime(this object o)
        {
            if (o.IsEmpty()) return null;
            return DateTime.Parse(o.ToString());
        }


        /// <summary>数字转化为枚举</summary>
        public static T? ToEnum<T>(this int? n) where T : struct
        {
            if (n == null)  return null;
            return (T)Enum.ToObject(typeof(T), n);
        }



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

        /// <summary>数组转化为列表</summary>
        public static List<T> ToList<T>(params T[] steps)
        {
            List<T> items = new List<T>();
            foreach (var step in steps)
                items.Add(step);
            return items;
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
    }
}