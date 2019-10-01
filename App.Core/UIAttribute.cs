using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;

namespace App.Core
{
    /// <summary>
    /// 编辑器类型
    /// </summary>
    public enum EditorType
    {
        [UI(title: "自动选择")]     Auto,
        [UI(title: "标签")]         Label,
        [UI(title: "文本框 ")]      TextBox,
        [UI(title: "多行文本框")]   TextArea,
        [UI(title: "HTML编辑框")]   HtmlEditor,
        [UI(title: "MD编辑框")]     MarkdownEditor,
        [UI(title: "数字框")]       NumberBox,
        [UI(title: "日期选择")]     DatePicker,
        [UI(title: "时间选择")]     TimePicker,
        [UI(title: "日期时间选择")] DateTimePicker,
        [UI(title: "图片")]         Image,
        [UI(title: "枚举下拉框")]   EnumDropDownList
    }

    /// <summary>
    /// 字段展示场合（表单、网格等）
    /// </summary>
    public enum ShowType : int
    {
        [UI(title: "不展示")]              No = 0,
        [UI(title: "表单页面显示该字段")]  Form = 1,
        [UI(title: "网格页面显示该字段")]  Grid = 2,
        [UI(title: "只读页面显示该字段")]  View = 4,
        [UI(title: "全部")]                All = Form | Grid | View
    }


    /// <summary>
    /// 字段导出方式
    /// </summary>
    public enum ExportType : int
    {
        [UI(title: "不导出")]   No = 0,
        [UI(title: "简单")]     Simple = 1,
        [UI(title: "普通")]     Normal = 2,
        [UI(title: "详细")]     Detail = 4,
        [UI(title: "全部")]     All = Simple | Normal | Detail
    }

    //========================================================
    // UIAttribute
    //========================================================
    /// <summary>
    /// 赋予类或枚举成员，用于描述该成员的 UI 外观信息
    /// </summary>
    public class UIAttribute : Attribute
    {
        /// <summary>标题</summary>
        public string Title { get; set; }

        /// <summary>分组</summary>
        public string Group { get; set; }

        /// <summary>格式化字符串</summary>
        public string Format { get; set; } = "{0}";

        /// <summary>显示数据类型</summary>
        public Type Type { get; set; }

        /// <summary>附加数据</summary>
        public string Tag { get; set; }

        //---------------------------------------------
        // 显隐及输出控制
        //---------------------------------------------
        /// <summary>何时显示</summary>
        public ShowType Show { get; set; } = ShowType.All;

        /// <summary>何时导出</summary>
        public ExportType Export { get; set; } = ExportType.All;


        //---------------------------------------------
        // 数据格式
        //---------------------------------------------
        /// <summary>是否只读</summary>
        public bool ReadOnly { get; set; } = false;

        /// <summary>是否必填</summary>
        public bool Required { get; set; } = false;

        /// <summary>精度（小数类型）</summary>
        public int Precision { get; set; } = 2;

        /// <summary>正则表达式</summary>
        public string Regex { get; set; } = "";


        //---------------------------------------------
        // 编辑控件
        //---------------------------------------------
        /// <summary>宽度</summary>
        public int Width { get; set; } = 70;

        /// <summary>高度</summary>
        public int Height { get; set; } = 50;

        /// <summary>编辑控件</summary>
        public EditorType Editor { get; set; } = EditorType.Auto;

        /// <summary>编辑控件扩展信息</summary>
        public string EditorData { get; set; } = "";



        //---------------------------------------------
        // 自动计算字段
        //---------------------------------------------
        // 全名
        [XmlIgnore]
        public string FullTitle
        {
            get
            {
                if (string.IsNullOrEmpty(Group)) return Title;
                else return string.Format("{0}-{1}", Group, Title);
            }
        }

        /// <summary>对应的字段信息</summary>
        [XmlIgnore]
        public PropertyInfo Field { get; set; }


        //---------------------------------------------
        // 构造函数
        //---------------------------------------------
        public UIAttribute(string title) : this("", title, "{0}") { }
        public UIAttribute(string group, string title, string formatString="{0}")
        {
            this.Group = group;
            this.Title = title;
            this.Format = formatString;
        }
        public UIAttribute(string title, ExportType export) : this("", title, export) { }
        public UIAttribute(string group, string title, ExportType export)
        {
            this.Group = group;
            this.Title = title;
            this.Export = export;
        }
        public UIAttribute(string title, ShowType show) : this("", title, show) { }
        public UIAttribute(string group, string title, ShowType show)
        {
            this.Group = group;
            this.Title = title;
            this.Show = show;
        }
        public UIAttribute(string title, EditorType type) : this("", title, type) { }
        public UIAttribute(string group, string title, EditorType type)
        {
            this.Group = group;
            this.Title = title;
            this.Editor = type;
        }
        public UIAttribute(string title, Type type) : this("", title, type) { }
        public UIAttribute(string group, string title, Type type)
        {
            this.Group = group;
            this.Title = title;
            this.Type = type;
        }

    }


    //========================================================
    // UIExtension
    //========================================================
    /// <summary>
    /// 辅助扩展方法
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
                attr.Field = prop;
                attr.Type = attr.Type ?? prop.PropertyType;  // 编辑器的数据来源类型
                attrs.Add(attr);
            }
            return attrs;
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

        /// <summary>获取字段说明</summary>
        public static string GetDescription(this PropertyInfo info)
        {
            return GetDescription(info as MemberInfo);
        }

        /// <summary>获取字段说明（来自 UIAttribute 或 DescriptionAttribute）</summary>
        public static string GetDescription(this MemberInfo info)
        {
            if (info != null)
            {
                var attr1 = info.GetCustomAttribute<UIAttribute>();
                var attr2 = info.GetCustomAttribute<DescriptionAttribute>();
                if (attr1 != null) return attr1.Title;
                if (attr2 != null) return attr2.Description;
                return info.Name;
            }
            return "";
        }


        /// <summary>获取枚举值的文本说明。RoleType.Admin.GetDescription()</summary>
        public static string GetDescription(this object enumValue)
        {
            if (enumValue == null)
                return "";
            MemberInfo info = GetEnumField(enumValue);
            return GetDescription(info);
        }

        /// <summary>获取属性的文本说明。typeof(Product).GetDescription("Name");</summary>
        public static string GetDescription(this Type type, string propertyName)
        {
            PropertyInfo info = type.GetProperty(propertyName);
            return GetDescription(info);
        }


        /// <summary>获取属性的文本说明。UIExtension.GetDescription<Product>(t => t.Name)</summary>
        public static string GetDescription<T>(this Expression<Func<T, object>> expression)
        {
            Type type = typeof(T);
            string propertyName = ReflectionHelper.GetExpressionName(expression);
            return GetDescription(type, propertyName);
        }

        /// <summary>获取属性的文本说明。product.GetDescription(t => t.Name)</summary>
        public static string GetDescription<T>(this T t, Expression<Func<T, object>> expression)
        {
            Type type = typeof(T);
            string propertyName = ReflectionHelper.GetExpressionName(expression);
            return GetDescription(type, propertyName);
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

    //========================================================
    // UISetting
    //========================================================
    /// <summary>
    /// UI 设置。可根据该类动态设置用户界面（如网格、表单等）
    /// TODO: 未完成
    /// </summary>
    [XmlInclude(typeof(UIAttribute))]
    public class UISetting : IID
    {
        public long ID { get; set; }
        public string Title { get; set; }
        public Type ModelType { get; set; }
        public List<UIAttribute> Items { get; set; }
        public Dictionary<string, List<UIAttribute>> Groups { get; set; }


        // 构造函数
        public UISetting()
        {
        }
        public UISetting(Type type, string title = "", int id = -1)
        {
            var attr = ReflectionHelper.GetAttribute<UIAttribute>(type);

            // title
            if (!string.IsNullOrEmpty(title))
                this.Title = title;
            else
            {
                if (attr != null && !string.IsNullOrEmpty(attr.Title))
                    this.Title = attr.Title;
                else
                    this.Title = type.Name;
            }

            //
            this.ID = id;
            this.ModelType = type;
            this.Items = type.GetUIAttributes();
            this.Groups = new Dictionary<string, List<UIAttribute>>();
            foreach (var item in Items)
            {
                if (!Groups.Keys.Contains(item.Group))
                    Groups.Add(item.Group, new List<UIAttribute>());
                Groups[item.Group].Add(item);
            }
        }
    }
}