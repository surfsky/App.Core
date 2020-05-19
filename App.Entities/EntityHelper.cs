using App.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace App.Entities
{

    /// <summary>
    /// 静态辅助方法
    /// </summary>
    public static class EntityHelper
    {

        /// <summary>获取实体的真实类型（而不是临时的代理类型）</summary>
        public static Type GetEntityType(this Type type)
        {
            if (type.FullName.StartsWith("System.Data.Entity.DynamicProxies"))
                return type.BaseType;
            return type;
        }

        //---------------------------------------------
        // ITree
        //---------------------------------------------
        /// <summary>获取树结构列表</summary>
        public static List<T> BuildTree<T>(this List<T> items, long? rootId = null)
            where T : class, ITree
        {
            var result = new List<T>();
            DoBuildTree(items, rootId, 0, ref result);
            return result;
        }
        /// <summary>构建树结构</summary>
        static int DoBuildTree<T>(List<T> items, long? rootId, int level, ref List<T> result)
            where T : class, ITree
        {
            int count = 0;
            var root = items.FirstOrDefault(d => d.ID == rootId);
            foreach (var item in items.Where(d => d.ParentID == rootId))
            {
                count++;
                item.TreeLevel = level;
                item.IsTreeLeaf = true;
                item.Enabled = true;
                //if (root?.Children != null)
                //    root.Children.Add(item);
                result.Add(item);

                level++;
                // 如果这个节点下没有子节点，则这是个终结节点
                int childCount = DoBuildTree(items, item.ID, level, ref result);
                if (childCount != 0)
                    item.IsTreeLeaf = false;
                level--;
            }
            return count;
        }

        /// <summary>拷贝树结构列表（并构造父子结构）</summary>
        public static List<T> CloneTree<T>(this List<T> items)
            where T : class, ITree<T>
        {
            // 先拷贝一份
            var result = new List<T>();
            foreach (var item in items)
                result.Add((T)item.Clone());

            // 构建父子结构
            foreach (var item in result)
            {
                item.Parent = result.FirstOrDefault(t => t.ID == item.ParentID);
                item.Children = result.Where(t => t.ParentID == item.ID).ToList();
            }

            //
            result = result.BuildTree();
            return result;
        }

        /// <summary>递归获取子节点（包含自身）</summary>
        /// <param name="all">所有元素</param>
        /// <param name="rootId">根元素ID</param>
        /// <returns>根元素及其子孙节点列表</returns>
        public static List<T> GetDescendants<T>(this List<T> all, long? rootId)
            where T : ITree<T>
        {
            if (rootId == null) 
                return new List<T>();
            else
                return GetDescendants(all, new List<long> { rootId.Value });
        }

        /// <summary>递归获取子节点（包含自身）</summary>
        public static List<T> GetDescendants<T>(this List<T> all, List<long> rootIds)
            where T : ITree<T>
        {
            var result = new List<T>();
            foreach (var rootId in rootIds)
            {
                var items = new List<T>();
                GetDescendantsInternal(all, items, rootId);
                result = result.Union(items);
            }
            return result;
        }

        static void GetDescendantsInternal<T>(List<T> all, List<T> items, long? rootId)
            where T : ITree<T>
        {
            if (rootId == null)
                return;
            var root = all.AsQueryable().FirstOrDefault(t => t.ID == rootId);
            if (root != null)
            {
                if (!items.Contains(root))
                    items.Add(root);
                var children = all.AsQueryable().Where(t => t.ParentID == root.ID);
                foreach (var child in children)
                    GetDescendantsInternal(all, items, child.ID);
            }
        }

    }
}