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
    /// 列类型
    /// </summary>
    public enum ColumnType
    {
        [UI("不显示")] No,
        [UI("自动")]   Auto,
        [UI("文本")]   Text,
        [UI("枚举")]   Enum,
        [UI("布尔")]   Bool,
        [UI("链接 ")]  Link,
        [UI("弹窗")]   Win,
        [UI("图片")]   Image,
        [UI("文件")]   File,
    }

    /// <summary>
    /// 编辑器类型
    /// </summary>
    public enum EditorType
    {
        [UI("不显示")]       No,
        [UI("自动选择")]     Auto,

        [UI("标签")]         Label,

        //
        [UI("文本框 ")]      TextBox,
        [UI("多行文本框")]   TextArea,
        [UI("HTML编辑框")]   HtmlEditor,
        [UI("MD编辑框")]     MarkdownEditor,

        //
        [UI("数字框")]       NumberBox,

        //
        [UI("日期选择")]     DatePicker,
        [UI("时间选择")]     TimePicker,
        [UI("日期时间选择")] DateTimePicker,

        //
        [UI("图片选择")]     ImageSelector,
        [UI("文件选择")]     FileSelector,

        //
        [UI("枚举下拉框")]   EnumDropDown,
        [UI("枚举组合框")]   EnumGroup,

        //
        [UI("布尔选择器")]   BoolSwitch,
        [UI("布尔下拉框")]   BoolDropDown
    }


    /// <summary>
    /// UI 外观信息
    /// </summary>
    public class UIAttribute : ParamAttribute
    {
        //
        // 公共属性
        //
        /// <summary>表单模式下的编辑控件</summary>
        public EditorType Editor { get; set; } = EditorType.Auto;

        /// <summary>列模式下的展示方式</summary>
        public ColumnType Column { get; set; } = ColumnType.Auto;

        /// <summary>列宽</summary>
        public int ColumnWidth { get; set; } = 70;

        /// <summary>附加数据</summary>
        public object Tag { get; set; }

        /// <summary>对应的字段信息</summary>
        public PropertyInfo Field { get; set; }

        /// <summary>标题全称</summary>
        public string FullTitle
        {
            get
            {
                if (string.IsNullOrEmpty(Group)) return Title;
                else return string.Format("{0}-{1}", Group, Title);
            }
        }


        //
        // 构造函数
        //
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
        public UIAttribute(string title, Type type) : this("", title, type) { }
        public UIAttribute(string group, string title, Type type)
        {
            this.Group = group;
            this.Title = title;
            this.Type = type;
        }


        //
        // 方法
        //
        /// <summary>格式化为文本</summary>
        public override string ToString()
        {
            return this.FullTitle;
        }
    }
}