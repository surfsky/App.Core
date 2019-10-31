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
        // 类型相关
        //------------------------------------------------
        /// <summary>尝试遍历获取类型（根据类型名、数据集名称）</summary>
        public static Type TryGetType(string typeName, string assemblyName = "", bool ignoreSystemType = true)
        {
            var type = Assembly.GetExecutingAssembly().GetType(typeName);
            if (type != null)
                return type;

            // 遍历程序集去找这个类
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                string name = assembly.FullName;
                if (name == assemblyName)
                    return assembly.GetType(typeName);

                // 过滤掉系统自带的程序集
                if (ignoreSystemType)
                    if (name.StartsWith("System") || name.StartsWith("Microsoft") || name.StartsWith("mscorlib"))
                        continue;

                // 尝试获取类别
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        /// <summary>尝试创建对象（根据类型名、数据集名称）</summary>
        public static object TryCreateObject(string typeName, string assemblyName = "")
        {
            var type = TryGetType(typeName, assemblyName);
            if (type != null)
                return type.Assembly.CreateInstance(type.Name, true);
            return null;
        }

        /// <summary>获取（可空类型的）真实类型</summary>
        public static Type GetRealType(this Type type)
        {
            if (type.IsNullable())
                return GetRealType(type.GetNullableDataType());
            return type;
        }

        /// <summary>获取类型字符串（可处理可空类型）</summary>
        public static string GetTypeString(this Type type, bool shortName = true)
        {
            if (type.IsNullable())
            {
                type = type.GetNullableDataType();
                return GetTypeString(type) + "?";
            }
            if (type.IsGenericList())
            {
                type = type.GetGenericDataType();
                return string.Format("List<{0}>", GetTypeString(type));
            }
            if (type.IsGenericDict())
            {
                type = type.GetGenericDataType();
                return string.Format("Dictionary<{0}>", GetTypeString(type));
            }
            if (type.IsValueType)
                return type.Name.ToString();
            return shortName ? type.Name.ToString() : type.FullName.ToString();
        }

        /// <summary>获取类型的概述信息（可解析枚举类型）</summary>
        public static string GetTypeSummary(this Type type)
        {
            if (type.IsNullable())
                type = type.GetNullableDataType();

            var sb = new StringBuilder();
            if (type.IsEnum)
            {
                foreach (var item in Enum.GetValues(type))
                    sb.AppendFormat("{0}-{1}({2}); ", (int)item, item.ToString(), item.GetDescription());
            }
            return sb.ToString();
        }

        //------------------------------------------------
        // 类型相关
        //------------------------------------------------
        /// <summary>是否是某个类型（或子类型）</summary>
        public static bool IsType(this Type raw, Type match)
        {
            return (raw == match) ? true : raw.IsSubclassOf(match);
        }

        /// <summary>是否实现接口</summary>
        public static bool IsInterface(this Type raw, Type match)
        {
            return (raw == match) ? true : match.IsAssignableFrom(raw);
        }


        /// <summary>是否属于某个类型</summary>
        public static bool IsType(this Type type, string typeName)
        {
            if (type.ToString() == typeName)
                return true;
            if (type.ToString() == "System.Object")
                return false;
            return IsType(type.BaseType, typeName);
        }


        /// <summary>是否是列表</summary>
        public static bool IsList(this Type type)
        {
            return type.GetInterface("IList") != null;
        }

        /// <summary>是否是字典</summary>
        public static bool IsDict(this Type type)
        {
            return type.GetInterface("IDictionary") != null;
        }


        /// <summary>是否是泛型列表</summary>
        public static bool IsGenericList(this Type type)
        {
            return type.IsGenericType && type.IsList();
        }

        /// <summary>是否是泛型字典</summary>
        public static bool IsGenericDict(this Type type)
        {
            return type.IsGenericType && type.IsDict();
        }

        /// <summary>是否是匿名类</summary>
        public static bool IsAnonymous(this Type type)
        {
            return type.Name.Contains("AnonymousType");
        }

        /// <summary>是否是泛型类型</summary>
        public static bool IsGenericType(this Type type)
        {
            return type.IsGenericType;
        }

        /// <summary>是否是简单值类型: String + DateTime + 枚举 + 基元类型(Boolean， Byte， SByte， Int16， UInt16， Int32， UInt32， Int64， UInt64， IntPtr， UIntPtr， Char，Double，和Single)</summary>
        public static bool IsSimpleType(this Type type)
        {
            return (type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type.IsEnum);
        }

        /// <summary>是否是可空类型</summary>
        public static bool IsNullable(this Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        /// <summary>获取可空类型中的值类型</summary>
        public static Type GetNullableDataType(this Type type)
        {
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)))
                return type.GetGenericArguments()[0];
            return type;
        }

        /// <summary>获取泛型中的数据类型</summary>
        public static Type GetGenericDataType(this Type type)
        {
            if (type.IsGenericType)
                return type.GetGenericArguments()[0];
            return type;
        }


    }
}