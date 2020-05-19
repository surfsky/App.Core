using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Utils;
//using EntityFramework.Extensions;

namespace App.Entities
{
    /// <summary>
    /// 系统配置表。
    /// 系统所有的状态信息、枚举信息都可在本表维护。可供用户在后台查看。
    /// </summary>
    [UI("系统", "状态配置")]
    public class XState : EntityBase<XState>
    {
        [UI("类别")]     public string Category { get; set; }
        [UI("键")]       public string Key { get; set; }
        [UI("值")]       public string Value { get; set; }
        [UI("标题")]     public string Title { get; set; }
        [UI("默认值")]   public bool?  IsDefault { get; set; }

        //-------------------------------------------
        // 方法
        //-------------------------------------------
        public static IQueryable<XState> Search(string catetory, string key="")
        {
            IQueryable<XState> q = Set;
            if (!catetory.IsEmpty())  q = q.Where(t => t.Category == catetory);
            if (!key.IsEmpty())       q = q.Where(t => t.Key == key);
            return q;
        }

        /// <summary>获取配置值</summary>
        public static string GetValue(string category, string key)
        {
            var config = XState.Search(category, key).FirstOrDefault();
            return (config == null) ? "" : config.Value;
        }

        /// <summary>设置配置值</summary>
        public static void SetValue(string category, string key, object value)
        {
            XState config = XState.Search(category, key).FirstOrDefault();
            if (config == null)
                new XState() { Key = key, Value = value.ToString(), Category = category }.Save();
            else
            {
                config.Value = value.ToString();
                config.SetModified();
                config.Save();
            }
        }

    }
}