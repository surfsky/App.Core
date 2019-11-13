using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;

namespace App.Core
{
    //========================================================
    // UISetting
    //========================================================
    /// <summary>
    /// UI 设置。可根据该类动态设置用户界面（如网格、表单等）
    /// </summary>
    [XmlInclude(typeof(UIAttribute))]
    public class UISetting : IID
    {
        /// <summary>实体ID</summary>
        public long ID { get; set; }

        /// <summary>标题</summary>
        public string Title { get; set; }

        /// <summary>数据模型类型</summary>
        public Type ModelType { get; set; }

        /// <summary>成员</summary>
        public List<UIAttribute> Items { get; set; } = new List<UIAttribute>();

        /// <summary>分组</summary>
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