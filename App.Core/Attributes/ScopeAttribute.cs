using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils
{
    /// <summary>
    /// 服务范围标签
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class ScopeAttribute : Attribute
    {
        public string Scope { get; set; }
        public ScopeAttribute(string scope)
        {
            this.Scope = scope;
        }
    }
}
