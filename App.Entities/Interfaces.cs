using App.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace App.Entities
{
    /// <summary>记录数据操作日期</summary>
    public interface ILogChange
    {
        DateTime? CreateDt { get; set; }
        DateTime? UpdateDt { get; set; }
    }

    /// <summary>导出数据接口</summary>
    public interface IExport
    {
        object Export(ExportMode type);
    }


    /// <summary>是否检测并发冲突</summary>
    public interface ICollsionDetect
    {
        /// <summary>并发冲突ID。保存时如果与库中不一致则抛出异常</summary>
        [ConcurrencyCheck]
        int? CollisionId { get; set; }
    }


    /// <summary>逻辑删除接口</summary>
    public interface IDeleteLogic
    {
        bool? InUsed { get; set; }
    }


    /// <summary>修正数据接口</summary>
    public interface IFix
    {
        /// <summary>修正实体自身数据</summary>
        object FixItem();

        /// <summary>批量修正数据</summary>
        /// <remarks>由于 C# 接口无法定义静态方法，只能用这种方法折衷实现。调用方法如：new XXX().FixBatch(); </remarks>
        int Fix();
    }

    /// <summary>初始化数据接口</summary>
    public interface IInit
    {
        /// <summary>批量初始化数据</summary>
        /// <remarks>由于 C# 接口无法定义静态方法，只能用这种方法折衷实现。调用方法如：new XXX().Init(); </remarks>
        void Init();
    }

    /// <summary>树接口</summary>
    public interface ITree :  IID
    {
        /// <summary>父ID</summary>
        long? ParentID { get; set; }

        /// <summary>名称</summary>
        string Name { get; set; }

        /// <summary>菜单在树形结构中的层级（从0开始）</summary>
        int TreeLevel { get; set; }

        /// <summary>是否可用（默认true）</summary>
        bool Enabled { get; set; }

        /// <summary>是否叶子节点（默认true）</summary>
        bool IsTreeLeaf { get; set; }
    }

    public interface ITree<T> : ITree, ICloneable
    {
        /// <summary>父节点</summary>
        T Parent { get; set; }

        /// <summary>子节点列表</summary>
        List<T> Children { get; set; }
    }

    /// <summary>
    /// 本类使用缓存数据。 
    /// 标注了本接口的类，将不直接从数据库获取数据，而是从缓存中获取数据。
    /// 这是一个标注接口，无任何成员。
    /// </summary>
    public interface ICacheAll{}

}