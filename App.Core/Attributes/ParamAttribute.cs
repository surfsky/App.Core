using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// 导出时机
    /// </summary>
    [Flags]
    public enum ExportType : int
    {
        [UI("不导出")] No = 0,
        [UI("简单")] Simple = 1,
        [UI("普通")] Normal = 2,
        [UI("详细")] Detail = 4,
        [UI("全部")] All = Simple | Normal | Detail
    }

    /// <summary>
    /// 数据模型描述
    /// </summary>
    public interface IParam
    {
        /// <summary>名称</summary>
        string Name { get; set; }

        /// <summary>数据类型</summary>
        Type Type { get; set; }

        /// <summary>格式化字符串</summary>
        string Format { get; set; }

        /// <summary>是否只读</summary>
        bool ReadOnly { get; set; }

        /// <summary>是否必填</summary>
        bool Required { get; set; }

        /// <summary>长度</summary>
        int Length { get; set; }

        /// <summary>精度（小数类型）</summary>
        int Precision { get; set; }

        /// <summary>正则表达式</summary>
        string Regex { get; set; }

        /// <summary>默认值</summary>
        object Default { get; set; }

        /// <summary>允许的值</summary>
        List<object> AllowValues { get; set; }
    }



    /// <summary>
    /// 参数信息
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class ParamAttribute : TAttribute, IParam
    {
        //
        // IParam
        //
        /// <summary>名称</summary>
        public string Name { get; set; }

        /// <summary>数据类型</summary>
        public Type Type { get; set; }

        /// <summary>格式化字符串</summary>
        public string Format { get; set; } = "{0}";

        /// <summary>是否只读</summary>
        public bool ReadOnly { get; set; } = false;

        /// <summary>是否必填</summary>
        public bool Required { get; set; } = false;

        /// <summary>长度</summary>
        public int Length { get; set; } = -1;

        /// <summary>精度（小数类型）</summary>
        public int Precision { get; set; } = 2;

        /// <summary>正则表达式</summary>
        public string Regex { get; set; }

        /// <summary>该参数是否在界面显示</summary>
        public bool Visible { get; set; } = true;

        /// <summary>默认值</summary>
        public object Default { get; set; }

        /// <summary>允许的值</summary>
        public List<object> AllowValues { get; set; }

        /// <summary>导出时机</summary>
        public ExportType Export { get; set; } = ExportType.All;

        //
        // 只读属性
        //
        /// <summary>参数类型名</summary>
        public string TypeName => this.Type?.GetTypeString();

        /// <summary>可选值列表</summary>
        public string Values => this.Type?.GetTypeValues();


        //
        // 构造函数
        //
        public ParamAttribute() { }
        public ParamAttribute(string name, string title, bool required=false)
        {
            this.Name = name;
            this.Title = title;
            this.Required = required;
        }
        public ParamAttribute(string name, string title, Type type, bool required=false)
        {
            this.Name = name;
            this.Title = title;
            this.Type = type;
            this.Required = required;
        }

        //
        // 方法
        //
        /// <summary>格式化为文本</summary>
        public override string ToString()
        {
            return $"{Name} {TypeName} {Remark}";
        }
    }
}
