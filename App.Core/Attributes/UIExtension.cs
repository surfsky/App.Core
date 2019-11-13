using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Xml.Serialization;

namespace App.Core
{
    /// <summary>
    /// UI Attribute 辅助扩展方法
    /// </summary>
    public static class UIExtension
    {
        //--------------------------------------------
        // 获取 UIAttribute
        //--------------------------------------------
        /// <summary>获取类拥有的 UIAttribute 列表</summary>
        public static List<UIAttribute> GetUIAttributes(this Type type)
        {
            var attrs = new List<UIAttribute>();
            foreach (var prop in type.GetProperties())
            {
                UIAttribute attr = ReflectionHelper.GetAttribute<UIAttribute>(prop);
                if (attr == null)
                    attr = new UIAttribute("", prop.Name);
                attr.Name = prop.Name;
                attr.Field = prop;
                attr.Type = attr.Type ?? prop.PropertyType;  // 编辑器的数据来源类型
                attrs.Add(attr);
            }
            return attrs;
        }

        /// <summary>获取类型说明</summary>
        public static UIAttribute GetUIAttribute(this Type type)
        {
            return type.GetCustomAttribute<UIAttribute>();
        }

        /// <summary>获取  UIAttribute </summary>
        public static UIAttribute GetUIAttribute(this Type type, string propertyName)
        {
            var info = type.GetProperty(propertyName);
            return GetUIAttribute(info);
        }

        /// <summary>获取  UIAttribute </summary>
        public static UIAttribute GetUIAttribute(this PropertyInfo info)
        {
            if (info != null)
                return info.GetCustomAttribute<UIAttribute>();
            return null;
        }

        /// <summary>获取枚举值的文本说明（来自UIAttribute或DescriptionAttribute）</summary>
        public static UIAttribute GetUIAttribute(this object enumValue)
        {
            var info = GetEnumField(enumValue);
            if (info != null)
                return info.GetCustomAttribute<UIAttribute>();
            return null;
        }



        //--------------------------------------------
        // Get UIAttribute property
        //--------------------------------------------
        /*
         * Type : MemberInfo
        /// <summary>获取类型说明（来自 UIAttribute 或 DescriptionAttribute）</summary>
        public static string GetDescription(this Type type)
        {
            if (type != null)
            {
                var attr1 = type.GetCustomAttribute<UIAttribute>();
                var attr2 = type.GetCustomAttribute<DescriptionAttribute>();
                if (attr1 != null) return attr1.Title;
                if (attr2 != null) return attr2.Description;
                return type.Name;
            }
            return "";
        }
        */

        /// <summary>获取说明（来自TAttribute, UIAttribute, DescriptionAttribute）</summary>
        /// <param name="info">类型或成员</param>
        public static string GetDescription(this MemberInfo info)
        {
            if (info != null)
            {
                var attr1 = info.GetCustomAttribute<UIAttribute>();
                if (attr1 != null) return attr1.Title.GetResText();

                var attr2 = info.GetCustomAttributes().FirstOrDefault(t => t.GetType() == typeof(TAttribute)) as TAttribute;
                if (attr2 != null) return attr2.Title.GetResText();

                var attr3 = info.GetCustomAttribute<DescriptionAttribute>();
                if (attr3 != null) return attr3.Description.GetResText();

                return info.Name.GetResText();
            }
            return "";
        }


        /// <summary>获取枚举值的文本说明。RoleType.Admin.GetDescription()</summary>
        public static string GetDescription(this object enumValue)
        {
            if (enumValue == null || !enumValue.IsEnum())
                return "";
            MemberInfo info = GetEnumField(enumValue);
            return GetDescription(info);
        }

        /// <summary>获取属性的文本说明。UIExtension.GetDescription<Product>(t =&lt; t.Name)</summary>
        public static string GetDescription<T>(this Expression<Func<T, object>> expression)
        {
            Type type = typeof(T);
            string propertyName = expression.GetExpressionName();
            return type.GetProperty(propertyName)?.GetDescription();
        }

        /// <summary>获取属性的文本说明。product.GetDescription(t =&lt; t.Name)</summary>
        public static string GetDescription<T>(this T t, Expression<Func<T, object>> expression)
        {
            Type type = typeof(T);
            string propertyName = expression.GetExpressionName();
            return type.GetProperty(propertyName)?.GetDescription();
        }

        //--------------------------------------------
        // 辅助方法
        //--------------------------------------------
        /// <summary>获取属性对应的 UI 类型（尝试取属性的 UI.EditorType 标注值，没有的话取属性的自身类型）</summary>
        public static Type GetUIType(this PropertyInfo info)
        {
            if (info != null)
            {
                var dataType = info.GetCustomAttribute<UIAttribute>()?.Type;
                return dataType ?? info.PropertyType;
            }
            return null;
        }

        /// <summary>获取枚举值对应的 UI 数据类型（尝试取枚举值的 UI.Type 标注值，没有的话取枚举类型））</summary>
        public static Type GetUIType(this object enumValue)
        {
            var info = GetEnumField(enumValue);
            if (info != null)
            {
                var dataType = info.GetCustomAttribute<UIAttribute>()?.Type;
                return dataType ?? info.FieldType;
            }
            return null;
        }

        /// <summary>获取类型的分组信息。RoleType.GetUIGroup()</summary>
        public static string GetUIGroup(this Type type)
        {
            var ui = GetUIAttribute(type);
            if (ui != null)
                return ui.Group;
            return "";
        }

        /// <summary>获取枚举值的分组信息。RoleType.Admin.GetUIGroup()</summary>
        public static string GetUIGroup(this object enumValue)
        {
            var ui = GetUIAttribute(enumValue);
            if (ui != null)
                return ui.Group;
            return "";
        }

        /// <summary>获取枚举值对应的字段</summary>
        static FieldInfo GetEnumField(this object enumValue)
        {
            if (enumValue == null) return null;
            var enumType = enumValue.GetType();
            return enumType.GetField(enumValue.ToString());
        }

    }

}