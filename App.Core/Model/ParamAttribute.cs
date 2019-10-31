using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Model
{
    /// <summary>
    /// 参数信息
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class ParamAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string DefaultValue { get; set; }
        public string Remark { get; set; }
        public int Length { get; set; }
        public bool Optional { get; set; } = false;

        public ParamAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
        public ParamAttribute(string name, string description, bool optional)
        {
            this.Name = name;
            this.Description = description;
            this.Optional = optional;
        }
        public ParamAttribute(string name, string description, Type type)
        {
            this.Name = name;
            this.Description = description;
            this.Type = type.GetTypeString();
            this.Remark = type.GetTypeSummary();
        }
        public ParamAttribute(string name, string description, string type, string remark, string defaultValue)
        {
            this.Name = name;
            this.Description = description;
            this.Type = type;
            this.Remark = remark;
            this.DefaultValue = defaultValue;
        }
    }
}
