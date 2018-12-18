using App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    /// <summary>
    /// 方法操作辅助方法（调用方法）
    /// </summary>
    public class MethodInvoker
    {
        /// <summary>调用方法</summary>
        public static object InvokeMethod(Type type, string methodName, Dictionary<string, object> args)
        {
            var info = type.GetMethod(methodName);
            return InvokeMethod(null, info, args);
        }

        public static object InvokeMethod(object obj, string methodName, Dictionary<string, object> args)
        {
            var info = obj.GetType().GetMethod(methodName);
            return InvokeMethod(obj, info, args);
        }

        /// <summary>调用方法</summary>
        public static object InvokeMethod(object obj, MethodInfo info, Dictionary<string, object> args)
        {
            object[] parameters = BuildMethodParameters(info, args);
            return info.Invoke(obj, parameters);
        }

        static object[] BuildMethodParameters(MethodInfo info, Dictionary<string, object> args)
        {
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
                if (o == "" && type != typeof(string) && pi.HasDefaultValue)
                {
                    array.Add(pi.DefaultValue);
                    continue;
                }

                // 转换数据类型
                var value = Cast(o, type);
                array.Add(value);
            }
            return array.ToArray();
        }

        // 转换数据类型
        private static object Cast(object o, Type type)
        {
            if (type.IsNullable())
                return Cast(o, type.GetRealType());

            // 字典转化为对象
            if (o is Dictionary<string, object>)
                return DicToObj(o as Dictionary<string, object>, type);
            else if (o != null)
            {
                if (o is string && o != "" && type.IsEnum)          // 处理枚举文本类型
                    return Enum.Parse(type, o.ToString(), true);
                else if (type == typeof(string))                    // 处理字符串类型
                    return o.ToString();
                else if (type.IsValueType)                          // 处理简单值类型
                    return Convert.ChangeType(o, type);
                else
                    return Newtonsoft.Json.JsonConvert.DeserializeObject(o.ToString(), type);  // 处理对象类型。该方法可以解析可空类型，但无法解析简单数据类型
            }

            return o;
        }


        // 将字典转化为指定对象
        public static object DicToObj(Dictionary<string, object> dic, Type type)
        {
            object o = type.Assembly.CreateInstance(type.FullName);
            foreach (PropertyInfo p in type.GetProperties())
            {
                string name = p.Name;
                if (dic.ContainsKey(name))
                {
                    Type propertyType = p.PropertyType;
                    object propertyValue = dic[name];
                    if (propertyValue is Dictionary<string, object>)
                        propertyValue = DicToObj(propertyValue as Dictionary<string, object>, propertyType);
                    else
                        propertyValue = ToBasicObject(propertyValue, propertyType);
                    p.SetValue(o, propertyValue, null);
                }
            }
            return o;
        }

        public static object ToBasicObject(object o, Type type)
        {
            if (type.IsSubclassOf(typeof(Enum)))
                return Enum.Parse(type, o.ToString());

            switch (type.FullName)
            {
                case "System.DateTime":
                    return Convert.ToDateTime(o);
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                    return Convert.ToInt64(o);
                case "System.Boolean":
                    return Convert.ToBoolean(o);
                case "System.Char":
                    return Convert.ToChar(o);
                case "System.Decimal":
                case "System.Double":
                case "System.Single":
                    return Convert.ToDouble(o);
                default:
                    return o;
            }
        }
    }
}
