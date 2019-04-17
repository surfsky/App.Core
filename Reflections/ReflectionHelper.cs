using System;
using System.Collections.Generic;
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
    public static class ReflectionHelper
    {
        //------------------------------------------------
        // 数据集相关
        //------------------------------------------------
        /// <summary>获取调用者数据集版本号</summary>
        public static Version AssemblyVersion
        {
            get { return Assembly.GetCallingAssembly().GetName().Version; }
        }

        /// <summary>获取调用者数据集路径</summary>
        public static string AssemblyPath
        {
            get { return Assembly.GetCallingAssembly().Location; }
        }

        /// <summary>获取调用者数据集目录</summary>
        public static string AssemblyDirectory
        {
            get { return new FileInfo(AssemblyPath).DirectoryName; }
        }

        /// <summary>获取某个类型归属的程序集版本号</summary>
        public static Version GetVersion(Type type)
        {
            return type.Assembly.GetName().Version;
        }


        //------------------------------------------------
        // 类型相关
        //------------------------------------------------
        /// <summary>尝试遍历获取类型（根据类型名、数据集名称）</summary>
        public static Type TryGetType(string typeName, string assemblyName = "")
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
        // 辅助
        //------------------------------------------------
        /// <summary>获取当前方法名（未测试）</summary>
        public static string GetCurrentMethodName()
        {
            return new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;
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



        //------------------------------------------------
        // 特性相关
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


        //------------------------------------------------
        // 读写属性
        //------------------------------------------------
        /// <summary>获取对象的属性值。也可考虑用dynamic实现。</summary>
        /// <param name="propertyName">属性名。可考虑用nameof()表达式来实现强类型。</param>
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            Type type = obj.GetType();
            PropertyInfo pi = type.GetProperty(propertyName);
            return pi.GetValue(obj);
        }

        /// <summary>设置对象的属性值。</summary>
        public static void SetPropertyValue(this object obj, string propertyName, object propertyValue)
        {
            PropertyInfo pi = obj.GetType().GetProperty(propertyName);
            Type type = pi.PropertyType;
            Type propertyType = type.GetRealType();
            var valueType = propertyValue?.GetType().GetRealType();

            // 比较属性的类型和值的类型, 如果一致就直接赋值, 不一致就转化为文本再处理
            if (propertyType == valueType)
                pi.SetValue(obj, propertyValue, null);
            else
            {
                var txt = propertyValue.ToText();
                if (propertyValue.IsEnum())
                    txt = ((int)propertyValue).ToString();
                else if (propertyType == typeof(DateTime))
                    txt = string.Format("{0:yyyy-MM-dd HH:mm:ss}", propertyValue);
                SetPropertyValue(obj, propertyName, txt);
            }
        }

        /// <summary>设置对象的属性值（用文本，转化为相应的数据类型），需要测试，给非空类型赋予可空数据会出错的</summary>
        public static void SetPropertyValue(this object obj, string propertyName, string propertyValue)
        {
            PropertyInfo pi = obj.GetType().GetProperty(propertyName);
            Type type = pi.PropertyType;
            if (type == typeof(string))
            {
                pi.SetValue(obj, propertyValue, null);
                return;
            }

            // 将字符串转化为对应的值类型
            Type realType = type.GetRealType();
            object value = propertyValue;
            if      (type == typeof(bool))                 value = propertyValue.ParseBool() ?? false;
            else if (type == typeof(Int64))                value = propertyValue.ParseLong() ?? 0;
            else if (type == typeof(Int32))                value = propertyValue.ParseInt() ?? 0;
            else if (type == typeof(Int16))                value = propertyValue.ParseShort() ?? 0;
            else if (type == typeof(DateTime))             value = propertyValue.ParseDate() ?? new DateTime();
            else if (type == typeof(float))                value = propertyValue.ParseFloat() ?? 0.0;
            else if (type == typeof(double))               value = propertyValue.ParseDouble() ?? 0.0;
            else if (type == typeof(decimal))              value = propertyValue.ParseDecimal() ?? (decimal)0.0;
            else if (type == typeof(bool?))                value = propertyValue.ParseBool();
            else if (type == typeof(Int64?))               value = propertyValue.ParseLong();
            else if (type == typeof(Int32?))               value = propertyValue.ParseInt();
            else if (type == typeof(Int16?))               value = propertyValue.ParseShort();
            else if (type == typeof(DateTime?))            value = propertyValue.ParseDate();
            else if (type == typeof(float?))               value = propertyValue.ParseFloat();
            else if (type == typeof(double?))              value = propertyValue.ParseDouble();
            else if (type == typeof(decimal?))             value = propertyValue.ParseDecimal();
            else if (type.IsEnum)                          value = propertyValue.ParseEnum(realType);
            else if (type.IsNullable() && realType.IsEnum) value = propertyValue.ParseEnum(realType);

            // 赋值
            pi.SetValue(obj, value, null);
        }



        /// <summary>获取对象的属性值（强类型版本）。var name = user.GetPropertyValue(t=> t.Name);</summary>
        public static TValue GetPropertyValue<T, TValue>(this T obj, Expression<Func<T, TValue>> property)
        {
            return property.Compile().Invoke(obj);
        }

        /// <summary>设置对象的属性值（强类型版本）。user.SetPropertyValue(t=> t.Name, "Cherry");</summary>
        public static void SetPropertyValue<T, TValue>(this T obj, Expression<Func<T, TValue>> property, TValue value)
        {
            string name = GetPropertyName(property);
            typeof(T).GetProperty(name).SetValue(obj, value, null);
        }


        //------------------------------------------------
        // Linq 强类型方法
        //------------------------------------------------
        /// <summary>获取表达式属性信息</summary>
        public static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T, object>> expr)
        {
            return GetPropertyInfo(expr.Body);
        }

        /// <summary>获取表达式属性信息</summary>
        public static PropertyInfo GetPropertyInfo(this Expression expr)
        {
            if (expr is LambdaExpression)
                expr = (expr as LambdaExpression).Body;

            // 一元操作符: array.Length, Convert(t.CreatDt)
            if (expr is UnaryExpression)
            {
                var expr1 = (UnaryExpression)expr;
                var expr2 = (MemberExpression)expr1.Operand;
                return GetPropertyInfo(expr2);
            }
            // 成员操作符： t.Dept.Name : body=t.Dept, member=Name
            if (expr is MemberExpression)
            {
                var expr1 = (MemberExpression)expr;
                if (expr1.Expression is MemberExpression)
                    return GetPropertyInfo(expr1.Expression);
                else
                    return expr1.Member as PropertyInfo;
            }
            return null;
        }


        /// <summary>获取表达式属性名。var name = GetPropertyName<User>(t => t.Dept.Name);</summary>
        public static string GetExpressionName<T>(this Expression<Func<T, object>> expr)
        {
            return GetExpressionName(expr.Body);
        }


        /// <summary>获取表达式属性名。var name = GetPropertyName<User>(t => t.Dept.Name);</summary>
        public static string GetExpressionName(this Expression expr)
        {
            // 一元操作符: array.Length, Convert(t.CreatDt)
            if (expr is UnaryExpression)
            {
                var expr1 = (UnaryExpression)expr;
                var expr2 = (MemberExpression)expr1.Operand;
                return GetExpressionName(expr2);
            }
            // 成员操作符： t.Dept.Name => body=t.Dept, member=Name
            if (expr is MemberExpression)
            {
                var expr1 = (MemberExpression)expr;
                var name = expr1.Member.Name;
                if (expr1.Expression is MemberExpression)
                    return GetExpressionName(expr1.Expression) + "." + name;
                else
                    return name;
            }
            // 参数本身：t 返回类型名
            if (expr is ParameterExpression)
            {
                return ((ParameterExpression)expr).Type.Name;
            }
            return "";
        }


        /// <summary>获取类的属性名。var name = GetPropertyName&lt;User, string&gt;(t => t.Name);</summary>
        public static string GetPropertyName<T, TMember>(this Expression<Func<T, TMember>> property)
        {
            return GetMemberInfo(property).Name;
        }

        /// <summary>获取对象的属性名。可用于获取一些匿名对象的属性名。GetPropertyName(() => user.Name</summary>
        public static string GetPropertyName<T>(this Expression<Func<T>> expr)
        {
            return (((MemberExpression)(expr.Body)).Member).Name;
        }

        /// <summary>获取类的成员信息。GetMemberInfo<User>(t => t.Name);</summary>
        public static MemberInfo GetMemberInfo<T, TMember>(this Expression<Func<T, TMember>> property)
        {
            MemberExpression me;
            if (property.Body is UnaryExpression)
                me = ((UnaryExpression)property.Body).Operand as MemberExpression;    // array.Length 数组长度是一元操作符
            else
                me = property.Body as MemberExpression;
            return me.Member;
        }

        /// <summary>获取方法名（失败）</summary>
        public static string GetMethodName<T>(Expression<Func<T, object>> expr)
        {
            var name = "";
            if (expr.Body is MethodCallExpression)
                name = ((MethodCallExpression)expr.Body).Method.Name;
            return name;
        }


    }
}