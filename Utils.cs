using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// 常用扩展方法
    /// </summary>
    public static class Utils
    {
        /// <summary>断言（如果逻辑表达式不成立，则抛出异常）</summary>
        public static void Assert(bool condition, string failInfo)
        {
            if (!condition)
                throw new Exception(failInfo);
        }

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


    }
}
