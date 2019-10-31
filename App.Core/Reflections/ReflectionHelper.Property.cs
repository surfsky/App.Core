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
            return (expr == null) ? "" : GetExpressionName(expr.Body);
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

        
    }
}