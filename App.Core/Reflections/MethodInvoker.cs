using App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils
{
    /// <summary>
    /// 方法调用器
    /// </summary>
    public static class MethodInvoker
    {
        /// <summary>调用静态方法</summary>
        public static object InvokeMethod(Type type, string methodName, Dictionary<string, object> args)
        {
            var info = type.GetMethod(methodName);
            return InvokeMethod(null, info, args);
        }

        /// <summary>调用成员方法</summary>
        public static object InvokeMethod(object obj, string methodName, Dictionary<string, object> args)
        {
            var info = obj.GetType().GetMethod(methodName);
            return InvokeMethod(obj, info, args);
        }

        /// <summary>调用成员方法</summary>
        /// <param name="obj">对象。如果为空，会尝试根据方法信息自动创建一个对象。</param>
        public static object InvokeMethod(object obj, MethodInfo info, Dictionary<string, object> args)
        {
            if (obj == null && !info.IsStatic)
                obj = Activator.CreateInstance(info.DeclaringType);
            object[] parameters = BuildMethodParameters(info, args);
            return info.Invoke(obj, parameters);
        }

        static object[] BuildMethodParameters(MethodInfo info, Dictionary<string, object> args)
        {
            args = args ?? new Dictionary<string, object>();
            List<object> array = new List<object>();
            if (info == null)
                return array.ToArray();

            // 遍历方法参数，找到匹配的输入参数
            foreach (var pi in info.GetParameters())
            {
                // 未找到匹配参数，尝试取方法的默认参数
                if (!args.Keys.Contains(pi.Name))
                {
                    if (pi.HasDefaultValue)
                        array.Add(pi.DefaultValue);
                    else if (pi.ParameterType.IsNullable())
                        array.Add(null);
                    continue;
                }

                // 找到匹配的输入参数
                var o = args[pi.Name];
                var type = pi.ParameterType;

                // 如果值为空字符串，尝试取方法的默认参数
                if ( o.IsEmpty() && type != typeof(string) && pi.HasDefaultValue)
                {
                    array.Add(pi.DefaultValue);
                    continue;
                }

                // 转换数据类型
                //var value = o.ToText().Parse(type);
                var value = o.To(type);
                array.Add(value);
            }
            return array.ToArray();
        }

    }
}
