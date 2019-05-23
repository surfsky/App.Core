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


    }
}