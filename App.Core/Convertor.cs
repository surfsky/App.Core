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
    /// ParseXXXX(string)      负责将字符串解析为对应的类型
    /// ToXXX()                负责将各种数据类型相互转换
    /// CastXXX()              负责列表元素的遍历、转换、筛选
    /// XXXEncode() XXXDecode  负责编解码
    /// </summary>
    public static partial class Convertor
    {

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

        /*
        // 以下代码由于侵入性太强及未处理异常而废除，请改用ParseXXX() 方法
        /// <summary>将可空对象转化为整型</summary>
        public static int? ToInt(this object o)
        {
            if (o.IsEmpty()) return null;
            return Convert.ToInt32(o);
        }


        /// <summary>将可空对象转化为长整型</summary>
        public static long? ToInt64(this object o)
        {
            if (o.IsEmpty()) return null;
            return Convert.ToInt64(o);
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
        */


        /// <summary>数字转化为枚举</summary>
        public static T? ToEnum<T>(this int? n) where T : struct
        {
            if (n == null)  return null;
            return (T)Enum.ToObject(typeof(T), n);
        }
        /// <summary>数字转化为枚举</summary>
        public static T? ToEnum<T>(this long? n) where T : struct
        {
            if (n == null) return null;
            return (T)Enum.ToObject(typeof(T), n);
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
        public static string ToSeparatedString(this IEnumerable source, char seperator=',')
        {
            if (source == null)
                return "";
            string txt = "";
            foreach (var item in source)
                txt += item.ToString() + seperator;
            return txt.TrimEnd(seperator);
        }
    }
}