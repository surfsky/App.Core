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
        // 方法
        //------------------------------------------------
        /// <summary>获取类的方法（包括祖先的）。注意 Type.GetMethods()只能获取当前类下的方法。</summary>
        /// <param name="searchAncestors">是否检索祖先的同名方法</param>
        public static List<MethodInfo> GetMethods(this Type type, string methodName, bool searchAncestors=true)
        {
            var methods = new List<MethodInfo>();

            // 遍历到父节点，寻找指定方法
            var t = type;
            var ms = new List<MethodInfo>();
            while (t != null)
            {
                ms = t.GetMethods().Where(m => m.Name == methodName).ToList();
                methods.AddRange(ms);
                if (!searchAncestors)
                    break;
                t = t.BaseType;
            }
            return methods;
        }


        //------------------------------------------------
        // 特性
        //------------------------------------------------
        /// <summary>获取指定特性列表（支持Type、Property、Method等）</summary>
        public static List<T> GetAttributes<T>(this MemberInfo m) where T : Attribute
        {
            // 这种方法会获取包含基类的Attribute，不合适
            // return (T[])m.GetCustomAttributes(typeof(T), true).ToList();  
            // 准确获取完全一致的  Attribute，不包含基类
            return m.GetCustomAttributes()
                .Where(t => t.GetType() == typeof(T))
                .Select(t => t as T)
                .ToList()
                ;
        }

        /// <summary>获取指定特性（不抛出异常）</summary>
        public static T GetAttribute<T>(this MemberInfo m) where T : Attribute
        {
            return m.GetAttributes<T>().FirstOrDefault();
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
        public static MethodInfo GetCurrentMethod()
        {
            return new System.Diagnostics.StackTrace().GetFrame(1).GetMethod() as MethodInfo;
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