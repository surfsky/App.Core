using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// 参数信息
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class ParamAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Require { get; set; } = true;
        public Type Type { get; set; } = typeof(string);
        public int Length { get; set; }
        public string Regex { get; set; }
        public string Default { get; set; }
        public string Remark { get; set; }

        public string TypeName => this.Type?.GetTypeString();


        public ParamAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
        public ParamAttribute(string name, string description, bool require)
        {
            this.Name = name;
            this.Description = description;
            this.Require = require;
        }
        public ParamAttribute(string name, string description, bool require, Type type)
        {
            this.Name = name;
            this.Description = description;
            this.Type = type;
            this.Require = require;
            this.Remark = type.GetTypeSummary();
        }
    }
}
