using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// 常用扩展方法
    /// </summary>
    public static class Utils
    {
        //--------------------------------------------------
        // String
        //--------------------------------------------------
        /// <summary>获取文本</summary>
        public static string GetText(string format, params object[] args)
        {
            return (args.Length == 0) ? format : string.Format(format, args);
        }

        //--------------------------------------------------
        // 为空
        //--------------------------------------------------
        /// <summary>判断对象是否不为空、空字符串、空列表</summary>
        public static bool IsNotEmpty(this object o)
        {
            return !o.IsEmpty();
        }

        /// <summary>判断对象是否为空、空字符串、空列表</summary>
        public static bool IsEmpty(this object o)
        {
            if (o == null)
                return true;
            if (o is string)
                return string.IsNullOrEmpty(o as string);
            if (o is IEnumerable)
                return (o as IEnumerable).Count() == 0;
            return false;
        }

        /// <summary>获取列表的长度</summary>
        public static int Count(this IEnumerable data)
        {
            var n = 0;
            var e = data.GetEnumerator();
            while (e.MoveNext())
            {
                n++;
            }
            return n;
        }



        //--------------------------------------------------
        // 逻辑、断言
        //--------------------------------------------------
        /// <summary>模拟VBA的 IIF 函数。逻辑如 var result = o.IIF(t=>t>0, "Positive", "Nagetive");</summary>
        public static TResult IIF<TSource, TResult>(this TSource o, Func<TSource, bool> condition, TResult trueValue, TResult falseValue)
        {
            if (condition(o))
                return trueValue;
            else
                return falseValue;
        }

        /// <summary>断言（如果逻辑表达式不成立，则抛出异常）</summary>
        public static void Assert(bool condition, string failInfo)
        {
            if (!condition)
                throw new Exception(failInfo);
        }


        //--------------------------------------------------
        // 列表
        //--------------------------------------------------
        /// <summary>找到第一个匹配的位置</summary>
        public static int IndexOf<T>(this IEnumerable<T> data, Func<T, bool> condition)
        {
            int n = -1;
            foreach (var o in data)
            {
                n++;
                if (condition(o))
                    return n;
            }
            return n;
        }

    }
}
