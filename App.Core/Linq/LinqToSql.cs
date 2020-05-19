using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace App.Utils
{
    /// <summary>
    /// 表达式转化为SQL（Linq to sql 基本原理）（未测试)
    /// 参考:
    /// https://www.cnblogs.com/linxingxunyan/p/6245396.html
    /// https://www.cnblogs.com/kakura/p/6108950.html
    /// 任务：
    /// 将所有的Expression改为泛型的，如Expression<Func(T1,T2)>
    /// 现在用户是无法调用的
    /// </summary>
    public class LinqToSql
    {
        /// <summary>构建 Update SQL</summary>
        public static string BuildSelectSql<T>(MemberInitExpression select, Expression condition)
        {
            string members = GetMembersText(select);
            string where = GetConditionText(condition);
            string sql = "Select {0} from {1} where {2}";
            string table = typeof(T).Name;
            return string.Format(sql, members, table, where);
        }

        /// <summary>构建 Update SQL</summary>
        public static string BuildUpdateSql<T>(MemberInitExpression expr)
        {
            string txt = GetSetText(expr);
            string sql = "Update {0} set {1}";
            string table = typeof(T).Name;
            sql = string.Format(sql, table, txt);
            return sql;
        }

        /// <summary>构建 Update SQL</summary>
        public static string BuildDeleteSql<T>(Expression condition)
        {
            string txt = GetConditionText(condition);
            string sql = "Delete from {0} where {1}";
            string table = typeof(T).Name;
            sql = string.Format(sql, table, txt);
            return sql;
        }

        /// <summary>构建 Insert SQL</summary>
        public static string BuildInsertSql<T>(MemberInitExpression expr)
        {
            string members, values;
            Type type = typeof(T);
            members = GetMembersText(expr);
            values = GetValuesText(expr);

            // 构建insert表达式
            string sql = "Insert into {0}({1}) Values({2})";
            string table = type.Name;
            sql = string.Format(sql, table, members, values);
            return sql;
        }

        /// <summary>获取成员列表</summary>
        public static string GetMembersText(MemberInitExpression expr)
        {
            return string.Format(", ", GetMembers(expr));
        }
        public static List<string> GetMembers(MemberInitExpression expr)
        {
            List<string> items = new List<string>();
            foreach (MemberAssignment item in expr.Bindings)
                items.Add(item.Member.Name);
            return items;
        }

        /// <summary>获取值列表</summary>
        public static string GetValuesText(MemberInitExpression expr)
        {
            return string.Format(", ", GetValues(expr));
        }
        public static List<string> GetValues(MemberInitExpression expr)
        {
            List<string> items = new List<string>();
            foreach (MemberAssignment item in expr.Bindings)
                items.Add(GetConstantText((ConstantExpression)item.Expression));
            return items;
        }

        /// <summary>获取设置表达式文本。如: GetSetText(() => new Student{Name="xx", Age=20})</summary>
        public static string GetSetText(MemberInitExpression expr)
        {
             string result = "";
             List<string> members = new List<string>();
             foreach (MemberAssignment item in expr.Bindings)
             {
                 string update = item.Member.Name + "=" + GetConstantText((ConstantExpression)item.Expression);
                 members.Add(update);
             }
             result = string.Join(", ", members);
             return result;
         }

        /// <summary>获取表达式的文本（可用于构建sql的where表达式）</summary>
        /// <example>
        /// Expression<Func<User, bool>> expr = (n => n.ID > 1 && n.ID < 100 && n.Name != "张三" && n.Age >= 60  && n.Birthday != null);
        /// Console.WriteLine(GetText(expr));
        /// 以后改为泛型版本
        /// public static string GetConditionText<T>(Expression<Func<T, bool>> exp)
        /// </example>
        public static string GetConditionText(Expression expr)
        {
            if (expr is LambdaExpression)     return GetConditionText((expr as LambdaExpression).Body);
            if (expr is BinaryExpression)     return GetBinaryText(expr as BinaryExpression);
            if (expr is MemberExpression)     return GetMemberText(expr as MemberExpression);
            if (expr is MethodCallExpression) return GetMethodCallText(expr as MethodCallExpression);
            if (expr is ConstantExpression)   return GetConstantText(expr as ConstantExpression);
            if (expr is UnaryExpression)      return GetUnaryText(expr as UnaryExpression);
            return "";
        }

        /// <summary>判断包含函数的表达式</summary>
        /// <remarks>作为示例，以下仅解析Contains方法</remarks>
        private static string GetMethodCallText(MethodCallExpression expr)
        {
            // Contains方法转化为in语句
            if (expr.Method.Name.Contains("Contains"))
            {
                //获得调用者（集合）
                var getter = Expression.Lambda<Func<object>>(expr.Object).Compile();
                var data = getter() as IEnumerable;

                //获得参数
                var caller = expr.Arguments[0];
                while (caller.NodeType == ExpressionType.Call)
                    caller = (caller as MethodCallExpression).Object;
                var list = (from object i in data select "'" + i + "'").ToList();

                // 构建in语句
                var field = GetMemberText(caller as MemberExpression);
                return string.Format("{0} IN ({1})", field, string.Join(",", list.ToArray()));
            }
            else
            {
                throw new NotSupportedException("不支持的函数调用:" + expr.Method.Name);
            }
        }

        /// <summary>获取一元元运算符表达式字符串。如：！~ ++ -- -（负号） *（指针） & sizeof nameof ??</summary>
        public static string GetUnaryText(UnaryExpression exp)
        {
            return GetConditionText(exp.Operand);
        }

        /// <summary>获取常量表达式字符串。如："Cherry", 100</summary>
        public static string GetConstantText(ConstantExpression exp)
        {
            return GetValueText(exp.Value);
        }

        /// <summary>获取常量字符串。如："Cherry", 100</summary>
        public static string GetValueText(object value)
        {
            if (value == null)
                return "NULL";
            if (value is string)
                return string.Format("'{0}'", value);
            if (value is DateTime)
                return string.Format("'{0:yyyy-MM-dd HH:mm:ss}'", value);
            if (value is bool)
                return (bool)value ? "1" : "0";
            return value.ToString();
        }

        /// <summary>获取二元表达式字符串。如： a+b</summary>
        public static string GetBinaryText(BinaryExpression exp)
        {
            string left = GetConditionText(exp.Left);
            string oper = GetOperatorText(exp.NodeType);
            string right = GetConditionText(exp.Right);
            if (right == "NULL")
            {
                if (oper == "=")
                    oper = " is ";
                else
                    oper = " is not ";
            }
            return left + oper + right;
        }

        /// <summary>获取成员表达式字符串。如：user.Name </summary>
        public static string GetMemberText(MemberExpression exp)
        {
            return exp.Member.Name;
        }

        /// <summary>获取操作符表达式字符串。如：+ </summary>
        public static string GetOperatorText(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.OrElse:             return " OR ";
                case ExpressionType.Or:                 return "|";
                case ExpressionType.AndAlso:            return " AND ";
                case ExpressionType.And:                return "&";
                case ExpressionType.GreaterThan:        return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan:           return "<";
                case ExpressionType.LessThanOrEqual:    return "<=";
                case ExpressionType.NotEqual:           return "<>";
                case ExpressionType.Add:                return "+";
                case ExpressionType.Subtract:           return "-";
                case ExpressionType.Multiply:           return "*";
                case ExpressionType.Divide:             return "/";
                case ExpressionType.Modulo:             return "%";
                case ExpressionType.Equal:              return "=";
            }
            return "";
        }
    }
}