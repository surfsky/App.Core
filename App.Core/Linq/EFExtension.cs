using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Data.Entity;
using System.Data.Common;
using System.Data.SqlClient;
using System.Collections;

namespace App.Core
{
    /// <summary>
    /// EntityFramework 相关方法。
    /// Linq调试可使用Expression Tree Visualizer工具：https://github.com/Feddas/ExpressionTreeVisualizer/
    /// Linq及表达式参考：http://www.360doc.com/content/15/0718/11/14416931_485658704.shtml
    /// </summary>
    public static class EFExtension
    {
        //---------------------------------------------------
        // SQL相关
        //---------------------------------------------------
        /// <summary>运行sql语句，返回datatable</summary>
        public static DataTable ExecuteSelectSql(this DbContext ctx, string sql)
        {
            // command 
            var db = ctx.Database;
            var conn = db.Connection;
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = System.Data.CommandType.Text;

            // data
            DataTable dt = new DataTable();
            DbDataAdapter adp = new SqlDataAdapter();  // TODO: 暂时只能用于sqlserver，想办法改为自动适配的
            adp.SelectCommand = cmd;
            adp.Fill(dt);

            return dt;
        }


        //---------------------------------------------------
        // Between
        // http://blog.csdn.net/lee576/article/details/45076349
        //---------------------------------------------------
        /// <summary>
        /// 扩展Between 操作符（仅适合数字类型）
        /// 使用 var query = db.People.Between(person => person.Age, 18, 21);
        /// string 类型比较会出错，以后再想办法。觉得麻烦就直接写where吧。
        /// </summary>
        public static IQueryable<TSource> Between<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, TKey low, TKey high)
            where TKey : IComparable<TKey>
        {
            Expression key = Expression.Invoke(keySelector, keySelector.Parameters.ToArray());
            Expression lowExpression, highExpression;
            if (low is string)
            {
                // 字符串类型要用CompareTo来模拟大等于操作。str.CompareTo("xxxx") >= 0
                var methodInfo = typeof(String).GetMethod("CompareTo", new Type[] { typeof(String) });
                lowExpression = Expression.GreaterThanOrEqual(
                            Expression.Call(key, methodInfo, Expression.Constant(low)),
                            Expression.Constant(0, typeof(Int32))
                            );
                highExpression = Expression.GreaterThanOrEqual(
                            Expression.Call(key, methodInfo, Expression.Constant(high)),
                            Expression.Constant(0, typeof(Int32))
                            );
            }
            else
            {
                lowExpression = Expression.GreaterThanOrEqual(key, Expression.Constant(low));
                highExpression = Expression.LessThanOrEqual(key, Expression.Constant(high));
            }
            Expression andExpression = Expression.AndAlso(lowExpression, highExpression);
            Expression<Func<TSource, bool>> lambda = Expression.Lambda<Func<TSource, bool>>(andExpression, keySelector.Parameters);
            return source.Where(lambda);
        }


        /// <summary>大等于</summary>
        public static IQueryable<TSource> GreaterEqual<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, TKey value)
            where TKey : IComparable<TKey>
        {
            Expression key = Expression.Invoke(keySelector, keySelector.Parameters.ToArray());
            Expression operation = Expression.GreaterThanOrEqual(key, Expression.Constant(value));
            Expression<Func<TSource, bool>> lambda = Expression.Lambda<Func<TSource, bool>>(operation, keySelector.Parameters);
            return source.Where(lambda);
        }

        /// <summary>小等于</summary>
        public static IQueryable<TSource> LessEqual<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, TKey value)
            where TKey : IComparable<TKey>
        {
            Expression key = Expression.Invoke(keySelector, keySelector.Parameters.ToArray());
            Expression operation = Expression.LessThanOrEqual(key, Expression.Constant(value));
            Expression<Func<TSource, bool>> lambda = Expression.Lambda<Func<TSource, bool>>(operation, keySelector.Parameters);
            return source.Where(lambda);
        }


        //---------------------------------------------------
        // Boolean
        //---------------------------------------------------
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
        {
            var invokedExpression = Expression.Invoke(expression2, expression1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.Or(expression1.Body, invokedExpression), expression1.Parameters);
        }
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
        {
            var invokedExpression = Expression.Invoke(expression2, expression1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.And(expression1.Body, invokedExpression), expression1.Parameters);
        }
        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
        {
            var negated = Expression.Not(expression.Body);
            return Expression.Lambda<Func<T, bool>>(negated, expression.Parameters);
        }

        //---------------------------------------------------
        // 排序 & 分页
        //---------------------------------------------------
        /// <summary>排序后分页（字段名是用字符串的，不安全，慎用）</summary>
        /// <example>q.SortAndPage("Name", "ASC", 2, 100);</example>
        [Obsolete("请使用强类型方法 SortAndPage(t => t.Column, true, 0, 10)")]
        internal static IQueryable<T> SortAndPage<T>(this IQueryable<T> query, string sortField, string sortDirection, int pageIndex, int pageSize)
        {
            return query.SortBy(sortField , sortDirection).Page(pageIndex, pageSize);
        }

        /// <summary>排序后分页</summary>
        /// <example>q.SortAndPage(t => t.Name, true, 2, 100);</example>
        public static IQueryable<T> SortAndPage<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> keySelector, bool ascend, int pageIndex, int pageSize)
        {
            return query.SortBy(keySelector, ascend).Page(pageIndex, pageSize);
        }

        /// <summary>排序（可指定升降序）</summary>
        /// <example>q.SortBy(t => t.Name, true);</example>
        public static IQueryable<T> SortBy<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> keySelector, bool ascend = true)
        {
            return ascend ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }

        /// <summary>排序</summary>
        /// <example>q.SortBy("Name", "ASC");</example>
        [Obsolete("请用强类型版本")]
        public static IQueryable<T> SortBy<T>(this IQueryable<T> query, string sortField, string sortDirection = "ASC")
        {
            if (String.IsNullOrEmpty(sortField))
                return query;
            if (string.IsNullOrEmpty(sortDirection))
                sortDirection = "ASC";

            // 构造表达式
            ParameterExpression parameter = Expression.Parameter(query.ElementType, String.Empty);
            MemberExpression property = Expression.Property(parameter, sortField);
            LambdaExpression lambda = Expression.Lambda(property, parameter);
            string methodName = (sortDirection == "ASC") ? "OrderBy" : "OrderByDescending";
            Expression methodCallExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { query.ElementType, property.Type },
                query.Expression,
                Expression.Quote(lambda)
                );

            //
            return query.Provider.CreateQuery<T>(methodCallExpression);
        }


        /// <summary>分页</summary>
        /// <example>q.Page(2, 100);</example>
        /// <param name="pageIndex">第几页（base-0）</param>
        /// <param name="pageSize">页面大小</param>
        public static IQueryable<T> Page<T>(this IQueryable<T> query, int pageIndex, int pageSize)
        {
            int total = query.Count();
            int pageCount = Convert.ToInt32(Math.Ceiling((double)total / (double)pageSize));
            if (pageCount < 1)            pageCount = 1;
            if (pageIndex > pageCount-1)  pageIndex = pageCount - 1;
            if (pageIndex < 0)            pageIndex = 0;

            return query.Skip(pageIndex * pageSize).Take(pageSize);
        }


        
    }
}