using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace App.Utils
{
    /// <summary>
    /// 历史版本信息
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class HistoryAttribute : Attribute
    {
        public string Date { get; set; }
        public string User { get; set; }
        public string Info { get; set; }

        public HistoryAttribute(string date, string user, string info)
        {
            this.Date = date;
            this.User = user;
            this.Info = info;
        }
    }
}
