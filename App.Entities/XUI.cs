using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Utils;
//using EntityFramework.Extensions;
using Newtonsoft.Json;

namespace App.Entities
{
    /// <summary>UI 类型</summary>
    public enum XUIType
    {
        [UI("表单")] Form,
        [UI("列表")] List
    }

    [UI("系统", "UI配置")]
    public class XUI : EntityBase<XUI>, ICacheAll
    {
        [UI("名称")]       public string Name { get; set; }
        [UI("类型")]       public XUIType? Type { get; set; }
        [UI("实体类名")]   public string EntityTypeName { get; set; }
        [UI("配置")]       public string SettingText { get; set; }
        [UI("错误")]       public string Error { get; set; }

        [NotMapped]
        public UISetting Setting { get; set; }
        //{
        //    get { return this.SettingText.ParseJson<UISetting>(); }
        //    set { this.SettingText = value.ToJson(); }
        //}

        [NotMapped]
        public Type EntityType { get; set; }// => ReflectionHelper.TryGetType(this.EntityTypeName);


        //-----------------------------------------------
        // 缓存
        //-----------------------------------------------
        /// <summary>所有（有缓存）</summary>
        public new static List<XUI> All => IO.GetCache(AllCacheName, () =>
        {
            var items = Set.OrderBy(m => m.Name).ToList();
            foreach (var item in items)
                item.Parse();
            return items;
        });

        /// <summary>任何数据变更都尝试解析配置</summary>
        public override void AfterChange(EntityOp op)
        {
            if (op != EntityOp.Delete)
                Parse();
            base.AfterChange(op);
        }

        /// <summary>解析配置</summary>
        public void Parse()
        {
            try
            {
                this.Setting = this.SettingText.ParseJson<UISetting>();
                this.EntityType = Reflector.GetType(this.EntityTypeName);
                this.Error = "";
                if (this.Setting.EntityType == null)
                    this.Error = "EntityType 为空";
            }
            catch (Exception ex)
            {
                this.Error = ex.Message;
            }
        }


        //-----------------------------------------------
        // 查询
        //-----------------------------------------------
        /// <summary>查询</summary>
        public static IQueryable<XUI> Search(XUIType? type, string typeName)
        {
            IQueryable<XUI>             q = All.AsQueryable();
            if (typeName.IsNotEmpty())  q = q.Where(t => t.EntityTypeName== typeName);
            if (type != null)           q = q.Where(t => t.Type == type);
            return q;
        }

        /// <summary>获取实体类配置信息</summary>
        public static UISetting GetSetting(XUIType type, Type entityType)
        {
            if (entityType == null)
                return null;
            UISetting setting;
            var settings = XUI.Search(type, entityType.FullName).ToList();
            if (settings.Count > 0)
                setting = settings[0].Setting;
            else
                setting = new UISetting(entityType);
            return setting;
        }
    }
}