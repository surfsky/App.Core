using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Data.Common;
using System.Collections;


namespace App.Utils
{
    /// <summary>
    /// 简单的json序列化类。
    /// 由于Json.net序列化属性过多的对象时会生成很长的json字符串，且耗时过大。不太适合存储在日志中
    /// 故编写了该类，用于限制json的长度。可限制：
    /// （1）是否序列化列表属性
    /// （2）是否限制属性的个数
    /// </summary>
    /// <remarks>
    /// 可考虑
    /// </remarks>
    public class Jsonlizer
    {
        /// <summary> 对象转换为Json字符串</summary> 
        /// <param name="maxProperties">最大属性数目</param>
        /// <param name="skipListProperty">是否跳过列表</param>
        public static string ToJson(object obj, int maxProperties = 20, bool skipListProperty = true, bool skipReadonlyProperty = true, bool skipComplexProperty=true)
        {
            if (obj == null)
                return "{}";
            try
            {
                int i = 0;
                StringBuilder json = new StringBuilder();
                json.Append("{");
                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    // 获取属性值
                    Type type = prop.PropertyType;
                    if (skipReadonlyProperty && !prop.CanWrite)    continue;  // 跳过只读属性
                    object val = prop.GetGetMethod().Invoke(obj, null);
                    if (val == null)      continue;

                    // 构造属性值字符串
                    if (++i > maxProperties) break;
                    StringBuilder sb = new StringBuilder();
                    if (val is string)
                        sb.AppendFormat("\"{0}\"", ToJsonSafeString(val.ToString()));
                    else if (val is bool)
                        sb.AppendFormat("{0}", val.ToString().ToLower());
                    else if (val is Int16 || val is Int32 || val is Int64 || val is UInt16 || val is UInt32 || val is UInt64 || val is float || val is double || val is decimal)
                        sb.AppendFormat("{0}", val);
                    else if (val is DateTime)
                        sb.AppendFormat("\"{0:yyyy-MM-dd HH:mm:ss}\"", val);
                    else if (val is IEnumerable)
                    {
                        if (skipListProperty)     continue;  // 跳过列表属性
                        sb.Append(ToJson((IEnumerable)val));
                    }
                    else if (!type.IsValueType)
                    {
                        // 互相引用的属性序列化时容易造成死循环。要想实现的话，估计要用个字典来管理，详情可参考Log4Net代码（本项目暂不实现）
                        if (skipComplexProperty)  continue;  // 跳过复杂属性（引用属性）
                        sb.Append(ToJson(val, maxProperties, skipListProperty, skipReadonlyProperty));
                    }
                    else
                        sb.AppendFormat("\"{0}\"", ToJsonSafeString(val.ToString()));
                    json.AppendFormat("\"{0}\":{1},", prop.Name, sb);
                }
                return json.ToString().TrimEnd(',') + "}";
            }
            catch
            {
                return "{}";
            }
        }

        /// <summary> 
        /// 对象集合转换Json 
        /// </summary> 
        public static string ToJson(IEnumerable array, int maxProperties = 20, bool skipListProperty = true, bool skipReadonlyProperty=true)
        {
            string json = "[";
            foreach (object item in array)
                json += ToJson(item, maxProperties, skipListProperty, skipReadonlyProperty) + ",";
            json = json.TrimEnd(',');
            return json + "]";
        }

        /// <summary>
        /// 过滤特殊字符
        /// </summary>
        public static string ToJsonSafeString(String s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s.ToCharArray()[i];
                switch (c)
                {
                    case '\"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '/': sb.Append("\\/"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\v': sb.Append("\\v"); break;
                    case '\0': sb.Append("\\0"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }
    }
}