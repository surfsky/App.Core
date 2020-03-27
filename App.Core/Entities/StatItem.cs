using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entities
{
    /// <summary>
    /// 统计数据项（统计及报表用到）。格式如 {Name:"商品1", Step:"201901", Value:13}
    /// </summary>
    public class StatItem
    {
        public string Name { get; set; }
        public string Step { get; set; }
        public double Value { get; set; }

        /// <summary>统计数据项</summary>
        /// <param name="name">名称.如“商品1”</param>
        /// <param name="step">步骤名称。如“201902”</param>
        /// <param name="value">值。如“23”</param>
        public StatItem(string name, string step, double value)
        {
            this.Name = name;
            this.Step = step;
            this.Value = value;
        }
    }



}
