using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;

namespace App.Core
{
    /// <summary>
    /// 反射相关静态方法和属性
    /// </summary>
    public  static partial class ReflectionHelper
    {
        
        //------------------------------------------------
        // 特性
        //------------------------------------------------
        /// <summary>获取指定特性</summary>
        public static T GetAttribute<T>(this Type type) where T : Attribute
        {
            T[] arr = (T[])type.GetCustomAttributes(typeof(T), true);
            return (arr.Length == 0) ? null : arr[0];
        }

        /// <summary>获取指定特性</summary>
        public static T GetAttribute<T>(this PropertyInfo p) where T : Attribute
        {
            T[] arr = (T[])p.GetCustomAttributes(typeof(T), true);
            return (arr.Length == 0) ? null : arr[0];
        }

        /// <summary>获取指定特性</summary>
        public static List<T> GetAttributes<T>(this Type type) where T : Attribute
        {
            T[] arr = (T[])type.GetCustomAttributes(typeof(T), true);
            return arr.ToList();
        }

        /// <summary>获取指定特性</summary>
        public static List<T> GetAttributes<T>(this PropertyInfo p) where T : Attribute
        {
            T[] arr = (T[])p.GetCustomAttributes(typeof(T), true);
            return arr.ToList();
        }

        //------------------------------------------------
        // 事件
        //------------------------------------------------
        /// <summary>获取事件调用者列表</summary>
        public static List<Delegate> GetEventSubscribers(object o, string eventName)
        {
            var type = o.GetType();
            var field = type.GetField(eventName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
            if (field != null)
            {
                var d = (Delegate)field.GetValue(o);
                var delegates = d.GetInvocationList();
                return delegates.ToList();
            }
            return new List<Delegate>();
        }


        //------------------------------------------------
        // 方法
        //------------------------------------------------
        /// <summary>获取当前方法信息</summary>
        public static MethodBase GetCurrentMethodInfo()
        {
            return new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
        }

        /*
        /// <summary>获取方法名（失败）</summary>
        public static string GetMethodName<T>(Expression<Func<T, Delegate>> expr)
        {
            var name = "";
            if (expr.Body is MethodCallExpression)
                name = ((MethodCallExpression)expr.Body).Method.Name;
            return name;
        }
        */


        //------------------------------------------------
        // 辅助
        //------------------------------------------------
        /// <summary>组合各个对象的属性，输出为字典</summary>
        public static Dictionary<string, object> CombineObject(params object[] objs)
        {
            var dict = new Dictionary<string, object>();
            foreach (object o in objs)
            {
                if (o == null)
                    continue;
                foreach (PropertyInfo pi in o.GetType().GetProperties())
                    dict[pi.Name] = pi.GetValue(o);
            }
            return dict;
        }
    }
}