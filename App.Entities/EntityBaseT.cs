//using EntityFramework.Extensions;
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
using App.Utils;
using System.Data.Entity.Infrastructure;
using Z.EntityFramework.Plus;

namespace App.Entities
{

    /// <summary>
    /// 数据操作基类。实现了数据访问的一些基础CRUD操作。
    /// 请用子类继承之，并实现扩展逻辑，如Search(), GetDetail()
    /// 详见 Doc 相关文档
    /// </summary>
    /// <example>
    /// public class User : EntityBase&lt;User&gt;
    /// 
    /// // New and save
    /// var u = new User();
    /// u.Name = "Kevin";
    /// u.Save();
    /// 
    /// // Get and modify
    /// var user = User.Get(5);
    /// var user = User.Get(t => t.UserID == 5);
    /// user.Age = 20;
    /// user.Save();
    /// 
    /// // Search and Bind
    /// var users = User.Search(t => t.Name.Contains("Kevin")).ToList();
    /// var data = users.SortAndPage(t => t.Name, true, 0, 50);
    /// DataGrid1.DataSource = data;
    /// DataGrid1.DataBind();
    /// 
    /// // transaction
    /// using (var transaction = AppContext.Current.Database.BeginTransaction())
    /// {
    ///     try
    ///     {
    ///         var orderItem = new OrderItem(....);
    ///         var order = new Order(....);
    ///         orderItem.Save();
    ///         order.Save();
    ///         transaction.Commit();
    ///     }
    ///     catch
    ///     {
    ///         transaction.Rollback();
    ///     }
    /// }
    /// </example>
    public class EntityBase<T> : EntityBase
        where T : EntityBase
    {
        //--------------------------------------
        // 数据集
        //--------------------------------------
        /// <summary>数据集</summary>
        public static DbSet<T> Set => Db.Set<T>();

        /// <summary>有效数据集（若为逻辑删除，返回 Set.Where(t => t.InUsed != false)）</summary>
        public static IQueryable<T> ValidSet
        {
            get
            {
                IQueryable<T> set = Set;
                if (typeof(T).IsInterface(typeof(IDeleteLogic)))
                    set = set.WhereNotEqual(nameof(IDeleteLogic.InUsed), false, true);
                return set;
            }
        }

        /// <summary>已引用关联表的数据集（注意，不包含集合类的数据类型）</summary>
        public static IQueryable<T> IncludeSet
        {
            get
            {
                IQueryable<T> set = ValidSet;
                var type = typeof(T);
                // 查找所有virtual属性，并include
                var ps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var p in ps)
                {
                    // 跳过基础数据类型和集合数据类型
                    if (p.PropertyType.IsBasicType())
                        continue;
                    if (p.PropertyType.IsCollection())
                        continue;
                    foreach (var accessor in p.GetAccessors())
                    {
                        if (accessor.IsVirtual)
                        {
                            set = set.Include(p.Name);
                            break;
                        }
                    }
                }
                return set;
            }
        }

        /// <summary>ID 生成方式</summary>
        [NotMapped]
        public static SnowflakeIDAttribute SnowflakeAttribute
        {
            get
            {
                var t = typeof(T);
                return IO.GetDict<SnowflakeIDAttribute>(t.FullName.MD5(), () => t.GetProperty("ID").GetAttribute<SnowflakeIDAttribute>());
            }
        }

        //--------------------------------------
        // 缓存
        //--------------------------------------
        /// <summary>单例</summary>
        public static T Instance => IO.GetCache(typeof(T).Name, () => Set.FirstOrDefault());

        /// <summary>所有数据的缓存</summary>
        public static List<T> All => IO.GetCache(AllCacheName, () => ValidSet.ToList());

        /// <summary>所有数据缓存名称</summary>
        public static string AllCacheName => string.Format("All{0}s", typeof(T).Name);


        /// <summary>清除该实体的全部数据缓存</summary>
        public static void ClearCache()
        {
            IO.RemoveCache(typeof(T).Name);
            IO.RemoveCache(AllCacheName);
        }
        /// <summary>加载该实体的全部数据缓存</summary>
        public static void LoadCache()
        {
            var data = All;
        }

        //--------------------------------------
        // 重载事件
        //--------------------------------------
        /// <summary>保存后刷新缓存</summary>
        public override void AfterChange(EntityOp op) => ClearCache();




        //--------------------------------------
        // CRUD
        //--------------------------------------
        /// <summary>保存修改</summary>
        public T Save(bool log = false)
        {
            // 新增或修改预处理
            var op = (Db.Entry(this).State == EntityState.Detached) ? EntityOp.New : EntityOp.Edit;
            if (op == EntityOp.New)
            {
                if (this.ID == 0)
                {
                    if (SnowflakeAttribute != null)
                        this.ID = SnowflakeID.Instance.NewID();
                }
                this.CreateDt = DateTime.Now;
                Set.Add(this as T);
            }
            else
                this.UpdateDt = DateTime.Now;

            // 保存
            BeforeSave(op);
            Db.SaveChanges();
            Log(log, op == EntityOp.New ? "新增" : "更新", this.ID, this);
            AfterChange(op);
            return this as T;
        }

        /// <summary>获取（根据ID）</summary>
        public static T Get(long? id, bool physical = true)
        {
            if (id == null) return null;
            var o = Set.Find(id);
            if (o == null)
                return null;
            else
            {
                if (!physical && typeof(T).IsInterface(typeof(IDeleteLogic)))
                {
                    bool? inused = (bool?)o.GetValue(nameof(IDeleteLogic.InUsed));
                    if (inused == false)
                        return null;
                }
                return o;
            }
        }

        /// <summary>找到首个实体</summary>
        public static T Get(Expression<Func<T, bool>> condition)
        {
            return Set.Where(condition).FirstOrDefault();
        }


        /// <summary>查找指定ID的数据列表</summary>
        public static IQueryable<T> Search(List<long> ids)
        {
            return Set.Where(t => ids.Contains(t.ID));
        }

        /// <summary>查找</summary>
        public static IQueryable<T> Search(Expression<Func<T, bool>> condition)
        {
            return condition == null ? Set : Set.Where(condition);
        }


        /*
        /// <summary>自定义条件过滤、排序等</summary>
        public static IQueryable<T> Search(
            Expression<Func<T, bool>> condition = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = ""
            )
        {
            IQueryable<T> query = Set;
            if (condition != null)
            {
                query = query.Where(condition);
                // bool judeg = typeof(TEntity).IsAssignableFrom(typeof(ILogicDelable)); 判断是否继承
                // 如果 TEntity  继承 ILogicDelable 添加条件  where (p=>p.IsDel == false)
            }
            foreach (var includeProperty in includeProperties.Split<string>())
                query = query.Include(includeProperty);

            if (orderBy != null)
                return orderBy(query);
            else
                return query;
        }
        */

        /// <summary>清空数据</summary>
        public static int Clear(bool log=false)
        {
            //return Set.Delete();
            Log(log, "清空数据", null, "", typeof(T));
            var items = Set.ToList();
            for (int i= 0; i < items.Count; i++)
                items[i].Delete(log);
            return items.Count;
        }

        /// <summary>批量物理删除</summary>
        public static int Delete(Expression<Func<T, bool>> whereExpression, bool log = false)
        {
            var q = Set.Where(whereExpression);
            Log(log, "批量物理删除", null, q.ToString(), typeof(T));
            return q.Delete();
        }

        /// <summary>删除（如要删除对应的附属资源，请填写类型名参数）</summary>
        public static void Delete(long id, bool log = false)
        {
            var item = Get(id);
            if (item != null)
                (item as EntityBase<T>).Delete(log);
        }

        /// <summary>删除。若类实现了IDeleteLogic接口则采用逻辑删除；若类实现了IDeleteRecursive接口则实现递归删除；默认采用物理删除；</summary>
        public override void Delete(bool log = false)
        {
            Log(log, "删除", this.ID, this);
            // 逻辑删除
            if (this is IDeleteLogic)
            {
                (this as IDeleteLogic).InUsed = false;
                this.Save();
            }
            // 物理删除
            else
            {
                // 级联删除附属资源
                DeleteRes();
                DeleteHistories();
                OnDeleteReference(this.ID);
                Set.Where(t => t.ID == this.ID).Delete();
            }

            // 删除后处理
            AfterChange(EntityOp.Delete);
        }

        /// <summary>删除相关数据</summary>
        public override void OnDeleteReference(long id)
        {
            /*
            if (typeof(T).IsInterface(typeof(ITree)))
            {
                var children = Set.Where(m => m.ParentID == id).ToList();   // TODO: 用代码动态构造
                foreach (var child in children)
                {
                    OnDeleteReference(child.ID);
                    Set.Where(t => t.ID == child.ID).Delete();
                }
            }
            */
        }


        //---------------------------------------------
        // 几个虚拟方法，如有需要请在子类实现。
        // 经测试，静态方法不支持virtual，所以在子类实现时请加个 new 关键字
        //---------------------------------------------
        /// <summary>获取详情（包括关联表信息）</summary>
        public static T GetDetail(long id)
        {
            return IncludeSet.FirstOrDefault(t => t.ID == id);
        }

        /*
        /// <summary>递归删除（包括子表数据）</summary>
        public static void DeleteRecursive(long id)
        {
            ClearCache();  //?
            throw new NotImplementedException();
        }
        */


        //---------------------------------------------
        // 统计
        //---------------------------------------------
        /// <summary>日增统计</summary>
        public static List<StatItem> StatDayNew(DateTime startDt, DateTime? endDt, Expression<Func<T, bool>> whereExpression = null)
        {
            var q = Set.Where(t => t.CreateDt >= startDt);
            if (endDt != null) q = q.Where(t => t.CreateDt <= endDt);
            if (whereExpression != null) q = q.Where(whereExpression);

            // 每日的增量数据
            var items = q
                .GroupBy(t => new
                {
                    Day = DbFunctions.TruncateTime(t.CreateDt).Value
                })
                .Select(t => new
                {
                    Day = t.Key.Day,
                    Cnt = t.Count()
                })
                .OrderBy(t => new { t.Day })
                .ToList()
                .Select(t => new StatItem("", t.Day.ToString("MMdd"), t.Cnt))
                .ToList()
                ;
            ;
            return items;
        }

        /// <summary>日存量统计</summary>
        public static List<StatItem> StatDayAmount(DateTime startDt, DateTime? endDt, Expression<Func<T, bool>> whereExpression = null)
        {
            var n = Set.Where(t => t.CreateDt < startDt).Where(whereExpression).Count();    // 初始数据
            var items = StatDayNew(startDt, endDt, whereExpression); // 每日新增数据
            return ToAmountData(items, n);
        }

        /// <summary>将日数据转化为累计数据</summary>
        public static List<StatItem> ToAmountData(List<StatItem> items, int baseAmount)
        {
            // 存量 = 之前量 + 今日增量
            return items
                .Each2((item, preItem) => item.Value = item.Value + (preItem?.Value ?? 0))
                .Each(t => t.Value += baseAmount)
                ;
        }
    }
}