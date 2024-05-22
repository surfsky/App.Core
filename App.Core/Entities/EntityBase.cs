using EntityFramework.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
//using App.Components;
using App.Core;
using System.Data.Entity.Infrastructure;

namespace App.Entities
{

    /// <summary>
    /// 实体操作方式
    /// </summary>
    public enum EntityOp
    {
        [UI("新增")]  New,
        [UI("编辑")]  Edit,
        [UI("删除")]  Delete,
    }

    /// <summary>
    /// 使用 SnowflakeID 算法创建ID
    /// </summary>
    public class SnowflakeIDAttribute : Attribute { }

    /// <summary>
    /// 数据实体基类，定义了
    /// (1) 公共属性（如id的生成、历史附表、资源附表等）。
    /// (2) 导出逻辑
    /// 数据操作请用泛型类 EntityBase&lt;T&gt;
    /// </summary>
    public class EntityBase : IID, ILogChange, IExport, IFix, IInit
    {
        /// <summary>ID字段。如要自定义数据库字段名，请重载并加上[Column("XXXID")]</summary>
        /// <remarks>此处用virtual标注，不会在本表中生成数据库字段，而在子类表中生成字段</remarks>
        [Key]
        [UI("ID", Mode=PageMode.View | PageMode.Edit, ReadOnly=true)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [SnowflakeID]
        public virtual long ID { get; set; }
        
        [UI("创建时间", Mode=PageMode.Edit, ReadOnly=true)]    
        public virtual DateTime? CreateDt { get; set; }
        
        [UI("更改时间", Mode=PageMode.Edit, ReadOnly = true)]  
        public virtual DateTime? UpdateDt { get; set; }

        //---------------------------------------------
        // 虚拟方法
        //---------------------------------------------
        /// <summary>保存前处理（如设置某些计算字段）</summary>
        public virtual void BeforeSave(EntityOp op)  {}

        /// <summary>数据 CURD 更改后处理（如统计、刷新缓存）</summary>
        public virtual void AfterChange(EntityOp op) {}

        /// <summary>删除相关数据（如相关表，级联数据）</summary>
        public virtual void OnDeleteReference(long id) { }

        /// <summary>删除</summary>
        public virtual void Delete(bool log = false)
        {
            Log(log, "删除", this.ID, this);
            // 逻辑删除
            if (this is IDeleteLogic)
            {
                (this as IDeleteLogic).InUsed = false;
            }
            // 物理删除
            else
            {
                // 级联删除附属资源
                DeleteRes();
                DeleteHistories();
                OnDeleteReference(this.ID);
                var entry = Db.Entry(this);
                entry.State = EntityState.Deleted;
            }
            Db.SaveChanges();

            // 删除后处理
            AfterChange(EntityOp.Delete);
        }


        //---------------------------------------------
        // 数据库
        //---------------------------------------------
        /// <summary>数据上下文。默认采用Common.Db。如需定制，请调用SetDb(),ReleaseDb()方法设置和取消。</summary>
        public static DbContext Db
        {
            //get { return AppContext.Current; }
            get { return CoreConfig.Db; }
        }

        /// <summary>设置状态为已修改</summary>
        public void SetModified()
        {
            Db.Entry(this).State = EntityState.Modified;
        }


        //---------------------------------------------
        // 日志
        // 可考虑做成事件暴露给调用者
        //---------------------------------------------
        protected void Log(bool save, string action, object id, object data)
        {
            Log(save, action, id, data, this.GetType());
        }
        protected static void Log(bool save, string action, object id, object data, Type type)
        {
            if (!save) return;
            string json = Jsonlizer.ToJson(data, 20, true, true);  // 序列化为json，并跳过复杂的属性
            string txt = string.Format("{0}: ID={1}, Type={2}, Data={3}", action, id, type, json);
            CoreConfig.Log("Database", txt);
        }




        //---------------------------------------------
        // 导出
        //---------------------------------------------
        /// <summary>获取导出对象（可用于接口数据输出）</summary>
        /// <param name="type">导出详细信息还是概述信息</param>
        /// <remarks>
        /// - 统一用 IExport 接口，以功能换性能。
        /// - 不采用 Expression 的原因：有些复杂导出属性要用方法生成，无法被EF解析。
        /// - 对于字段实在太多的类，如有有性能问题，可先 Select 后再 Export，注意字段要一致。
        /// - 可标注属性 [UI("xx", Export=ExportType.Detail]，并用默认 Export 方法导出。
        /// </remarks>
        /// <example>
        /// var item = User.Get(..).Export();
        /// var items = User.Search(....).ToList().Cast(t => t.Export());
        /// </example>
        /// <todo>
        /// 现有的Export区分三种类型的代码非常繁琐（参考User.Export），故可考虑自动拼装属性，逻辑如：
        /// - 子类根据 ExportType 分别导出各自属性（不重叠）
        /// - 基类的方法自动组装这些字段（Detail包含Normal, Normal包含Simple）
        /// - 可用字典，或用动态类实现该逻辑
        /// </todo>
        public virtual object Export(ExportMode type = ExportMode.Normal)
        {
            return this;
        }
        /// <summary>导出json</summary>
        public string ExportJson(ExportMode type = ExportMode.Normal)
        {
            return Export(type).ToJson();
        }


        //---------------------------------------------
        // UI 配置
        //---------------------------------------------
        // 理论上用静态方法更合适，用new覆盖
        /// <summary>网格设置信息</summary>
        public virtual UISetting GridUI()
        {
            return new UISetting(this.GetType());
        }
        /// <summary>表单设置i信息</summary>
        public virtual UISetting FormUI()
        {
            return new UISetting(this.GetType());
        }
        public virtual UISetting SearchUI()
        {
            var m = GetSearchMethod(this.GetType());
            if (m != null)
                return new UISetting(m);
            return null;
        }
        /// <summary>找到实体检索方法（具有[SearcherAttribute]，若没有则尝试找名称为"Search"的方法）</summary>
        static MethodInfo GetSearchMethod(Type type)
        {
            foreach (var m in type.GetMethods())
            {
                if (m.GetAttribute<SearcherAttribute>() != null)
                    return m;
            }
            return type.GetMethods("Search", false).FirstOrDefault();
        }



        //---------------------------------------------
        // 全局唯一键
        //---------------------------------------------
        [NotMapped, JsonIgnore]
        [UI("全局唯一ID", Column = ColumnType.None, Editor = EditorType.None, Export = ExportMode.None)]
        public string UniID
        {
            get
            {
                var type = this.GetType();
                if (type.FullName.Contains("System.Data.Entity.DynamicProxies"))
                    type = type.BaseType;
                return BuildUniID(type.Name, this.ID);
            }
        }
        protected static string BuildUniID(string prefix, long id)
        {
            return string.Format("{0}-{1}", prefix, id);
        }


        //---------------------------------------------
        // 操作历史
        //---------------------------------------------
        [NotMapped]
        [UI("操作历史", Column = ColumnType.None, Editor = EditorType.None, Export = ExportMode.Detail)]
        public List<History> Histories
        {
            get
            {
                var key = this.UniID;
                var items = History.Set.Where(t => t.Key == key).OrderBy(t => t.CreateDt).ToList();
                return (items.Count == 0) ? null : items;
            }
        }

        [NotMapped]
        [UI("最后操作历史", Column = ColumnType.None, Editor = EditorType.None, Export = ExportMode.Detail)]
        public History LastHistory
        {
            get
            {
                var key = this.UniID;
                return History.Search(key: key).Sort(t => t.CreateDt, false).FirstOrDefault();
            }
        }

        /// <summary>TODO：增加操作历史</summary>
        public History AddHistory(long? userId, string statusName, int? status = null, string remark = "", List<string> fileUrls = null)
        {
            //var user = User.Get(userId);
            return History.AddHistory(
                this.UniID, userId, statusName,
                statusId: status,
                userName: "", //user?.NickName,
                userMobile: "", //user?.Mobile,
                remark: remark,
                fileUrls: fileUrls
                );
        }

        /// <summary>删除附属历史</summary>
        public void DeleteHistories()
        {
            History.DeleteBatch(this.UniID);
        }

        //---------------------------------------------
        // 附属资源
        //---------------------------------------------
        // 资源
        [NotMapped]
        [UI("所有附件", Column = ColumnType.None, Editor = EditorType.None, Export = ExportMode.Detail)]
        public List<Res> Reses
        {
            get
            {
                var key = this.UniID;
                var items = Res.Set.Where(t => t.Key == key).OrderBy(t => t.Seq).ToList();
                return (items.Count == 0) ? null : items;  // 如果数目为空，强制输出null，以简化json结构
            }
        }
        [NotMapped, JsonIgnore]
        [UI("图片附件", Column = ColumnType.None, Editor = EditorType.None, Export = ExportMode.Detail)]
        public List<Res> Images
        {
            get
            {
                var key = this.UniID;
                var items = Res.Set.Where(t => t.Key == key).Where(t => t.Type == ResType.Image).OrderBy(t => t.Seq).ToList();
                return (items.Count == 0) ? null : items;
            }
        }
        [NotMapped, JsonIgnore]
        [UI("文件附件", Column = ColumnType.None, Editor = EditorType.None, Export = ExportMode.Detail)]
        public List<Res> Files
        {
            get
            {
                var key = this.UniID;
                var items = Res.Set.Where(t => t.Key == key).Where(t => t.Type == ResType.File).OrderBy(t => t.Seq).ToList();
                return (items.Count == 0) ? null : items;
            }
        }

        /// <summary>删除附属资源</summary>
        public void DeleteRes()
        {
            Res.DeleteBatch(this.UniID);
        }

        /// <summary>删除附属资源</summary>
        public static void DeleteRes(Type type, long id)
        {
            //string uniId = BuildUniID(typeof(T).Name, id);
            string uniId = BuildUniID(type.Name, id);
            Res.DeleteBatch(uniId);
        }

        /// <summary>添加资源</summary>
        public void AddRes(List<string> fileUrls)
        {
            string resKey = this.UniID;
            foreach (var url in fileUrls)
                Res.Add(ResType.File, resKey, url);
        }

        //---------------------------------------------
        // IInit & IFix
        //---------------------------------------------
        /// <summary>批量初始化数据（请用 new T().Init()方式调用）</summary>
        public virtual void Init()
        {
        }

        /// <summary>批量修正数据（请用 new T().Fix()方式调用）</summary>
        public virtual int Fix()
        {
            return 0;
        }

        /// <summary>修正单条数据</summary>
        public virtual object FixItem()
        {
            return this;
        }

    }
}