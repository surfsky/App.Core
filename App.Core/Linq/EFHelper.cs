using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Data.Entity;
using System.Data.Common;
using System.Data.SqlClient;
using System.Collections;

namespace App.Utils
{
    /// <summary>
    /// EntityFramework 相关方法。
    /// Linq调试可使用Expression Tree Visualizer工具：https://github.com/Feddas/ExpressionTreeVisualizer/
    /// Linq及表达式参考：http://www.360doc.com/content/15/0718/11/14416931_485658704.shtml
    /// </summary>
    public static class EFHelper
    {

        //---------------------------------------------------
        // 排序 & 分页
        //---------------------------------------------------
        /// <summary>分页</summary>
        /// <example>q.Page(2, 100);</example>
        /// <param name="pageIndex">第几页（base-0）</param>
        /// <param name="pageSize">页面大小</param>
        public static IQueryable<T> Page<T>(this IQueryable<T> query, int pageIndex, int pageSize)
        {
            int total = query.Count();
            int pageCount = Convert.ToInt32(Math.Ceiling((double)total / (double)pageSize));
            if (pageCount < 1) pageCount = 1;

            // 修正页码
            //if (pageIndex > pageCount - 1) pageIndex = pageCount - 1;  // 超出页数则显示最后一页（删除此行，否则会导致客户端无法获取无数据状态）
            if (pageIndex < 0) pageIndex = 0;
            return query.Skip(pageIndex * pageSize).Take(pageSize);
        }


        /// <summary>排序后分页（字段名是用字符串的，慎用）</summary>
        /// <example>q.SortAndPage("Name", "ASC", 2, 100);</example>
        public static IQueryable<T> SortPage<T>(this IQueryable<T> query, string sortField, string sortDirection, int pageIndex, int pageSize)
        {
            return query.Sort(sortField, sortDirection).Page(pageIndex, pageSize);
        }

        /// <summary>排序后分页</summary>
        /// <example>q.SortAndPage(t => t.Name, true, 2, 100);</example>
        public static IQueryable<T> SortPage<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> keySelector, bool ascend, int pageIndex, int pageSize)
        {
            return query.Sort(keySelector, ascend).Page(pageIndex, pageSize);
        }

        /// <summary>排序（可指定升降序）</summary>
        /// <example>q.SortBy(t => t.Name, true);</example>
        public static IQueryable<T> Sort<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> keySelector, bool ascend = true)
        {
            return ascend ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
        }

        /// <summary>排序（排序字段是非泛型的）</summary>
        /// <example>q.SortBy("Name", "ASC");</example>
        /// <remarks>就是构造 query.OrderBy() 或 query.OrderByDescending() </remarks>
        public static IQueryable<T> Sort<T>(this IQueryable<T> query, string sortField, string sortDirection = "ASC")
        {
            if (String.IsNullOrEmpty(sortField))
                return query;
            if (string.IsNullOrEmpty(sortDirection))
                sortDirection = "ASC";

            // 构造LambdaParameterExpression表达式：t => t.SortField
            var parameter = Expression.Parameter(query.ElementType, "t");  // t
            var property = Expression.Property(parameter, sortField);      // t.sortField
            var lambda = Expression.Lambda(property, parameter);           // t => t.sortField

            // 构造调用方法表达式：query.Orderby(t => t.SortField)
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

        //---------------------------------------------------
        // Where 表达式
        //---------------------------------------------------
        /// <summary>过滤（字段是非泛型的）</summary>
        /// <example>q.Where("InUsed", true)</example>
        /// <remarks>q.Where(t=> t.InUsed==true)</remarks>
        public static IQueryable<T> WhereEqual<T>(this IQueryable<T> query, string field, object o, bool nullable)
        {
            if (field.IsEmpty())
                return query;
            var t = Expression.Parameter(query.ElementType, "t");        // t
            var property = Expression.Property(t, field);                // t.field
            var val = Expression.Constant(o);                            // o
            var condition = Equal(property, val, nullable);              // t.field == o
            var lambda = Expression.Lambda<Func<T, bool>>(condition, t); // t => t.field == o

            //
            return query.Where(lambda);
        }

        /// <summary>过滤（字段是非泛型的）</summary>
        /// <example>q.WhereNot("InUsed", true)</example>
        /// <remarks>q.Where(t=> t.InUsed!=true)</remarks>
        public static IQueryable<T> WhereNotEqual<T>(this IQueryable<T> query, string field, object o, bool nullable)
        {
            if (field.IsEmpty())
                return query;
            var t = Expression.Parameter(query.ElementType, "t");       // t
            var property = Expression.Property(t, field);               // t.field
            var val = Expression.Constant(o);                           // o
            var condition = NotEqual(property, val, nullable);          // t.field != o
            var lambda = Expression.Lambda<Func<T,bool>>(condition, t); // t => t.field != o

            //
            return query.Where(lambda);
        }

        /// <summary>可空类型相等判断</summary>
        public static Expression Equal(Expression exp1, Expression exp2, bool nullable)
        {
            if (nullable)
            {
                var val = Expression.Property(exp1, "Value"); // exp1.Value
                var eq = Expression.Equal(val, exp2);         // exp1.Value==exp2
                return eq;
            }
            else
                return Expression.Equal(exp1, exp2);          // exp1==exp2
        }

        /// <summary>可空类型不相等判断</summary>
        public static Expression NotEqual(Expression exp1, Expression exp2, bool nullable)
        {
            if (nullable)
            {
                var val = Expression.Property(exp1, "Value"); // exp1.Value
                var eq = Expression.NotEqual(val, exp2);      // exp2.Value!=exp2
                return eq;
            }
            else
                return Expression.NotEqual(exp1, exp2);       // exp1==exp2
        }



        //---------------------------------------------------
        // Boolean
        //---------------------------------------------------
        public static Expression<Func<T, bool>> True<T>()  { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        /*
        // 这三个方法 EntityFramework 不支持
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
        */

        /// <summary>And 组合两个Bool表达式</summary>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.And);
        }

        /// <summary>Or 组合两个Bool表达式</summary>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.Or);
        }

        /// <summary>组合两个表达式</summary>
        public static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first) 
            // replace parameters in the second lambda expression with parameters from the first 
            // apply composition of lambda expression bodies to parameters from the first expression  
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);
            var secondBody = ExpressionRebinder.ReplaceParameters(map, second.Body);
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        /// <summary>
        /// 看不懂，能用
        /// </summary>
        internal class ExpressionRebinder : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, ParameterExpression> map;

            public ExpressionRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
            {
                return new ExpressionRebinder(map).Visit(exp);
            }

            protected override Expression VisitParameter(ParameterExpression p)
            {
                ParameterExpression replacement;
                if (map.TryGetValue(p, out replacement))
                    p = replacement;
                return base.VisitParameter(p);
            }
        }

        //---------------------------------------------------
        // SQL相关
        //---------------------------------------------------
        /// <summary>运行sql语句，返回datatable（暂时只能用于sqlserver）</summary>
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
            var key = Expression.Invoke(keySelector, keySelector.Parameters.ToArray());                // t
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
                lowExpression = Expression.GreaterThanOrEqual(key, Expression.Constant(low));            // t >= low
                highExpression = Expression.LessThanOrEqual(key, Expression.Constant(high));             // t <= high
            }
            var andExpression = Expression.AndAlso(lowExpression, highExpression);                       // (t>=low) && (t<=high)
            var lambda = Expression.Lambda<Func<TSource, bool>>(andExpression, keySelector.Parameters);  // t => (t>=low) && (t<=high)
            return source.Where(lambda);
        }


        /// <summary>大等于</summary>
        public static IQueryable<TSource> GreaterEqual<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, TKey value)
            where TKey : IComparable<TKey>
        {
            var key = Expression.Invoke(keySelector, keySelector.Parameters.ToArray());              // t
            var operation = Expression.GreaterThanOrEqual(key, Expression.Constant(value));          // t >= value
            var lambda = Expression.Lambda<Func<TSource, bool>>(operation, keySelector.Parameters);  // t => t >= value
            return source.Where(lambda);
        }

        /// <summary>小等于</summary>
        public static IQueryable<TSource> LessEqual<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, TKey value)
            where TKey : IComparable<TKey>
        {
            var key = Expression.Invoke(keySelector, keySelector.Parameters.ToArray());              // t
            var operation = Expression.LessThanOrEqual(key, Expression.Constant(value));             // t <= value
            var lambda = Expression.Lambda<Func<TSource, bool>>(operation, keySelector.Parameters);  // t => t <= value
            return source.Where(lambda);
        }


    }
}