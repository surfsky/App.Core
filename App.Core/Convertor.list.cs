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
    /// 类型转换-列表操作（遍历、转换、过滤）。类似.Select()方法，进行了转换操作
    /// </summary>
    public static partial class Convertor
    {
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
        public static List<Int64> CastInt64(this IEnumerable source)
        {
            return source.Cast<Int64>(t =>
                t.IsEnum()
                    ? Convert.ToInt64(t)
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

        /// <summary>遍历过滤（同Where，但名字会冲突; Query; Search; Filter）</summary>
        public static List<T> Search<T>(this IEnumerable<T> source, Func<T, bool> func)
        {
            var result = new List<T>();
            foreach (var item in source)
                if (func(item))
                    result.Add(item);
            return result;
        }
    }
}