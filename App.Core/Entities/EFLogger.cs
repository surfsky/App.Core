//using App.Components;
using App.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Web;

namespace App.Entities
{
    /// <summary>
    /// 自定义的 EntityFramework 数据库操作日志记录器
    /// DbInterception.Add(new EFLogger());
    /// </summary>
    public class EFLogger : IDbCommandInterceptor
    {
        //------------------------------------------
        // 接口方法
        //------------------------------------------
        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            Start(command);
        }
        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            Log(command, interceptionContext);
        }


        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            Start(command);
        }
        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {

            Log(command, interceptionContext);
        }


        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            Start(command);
        }
        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            Log(command, interceptionContext);
        }


        //------------------------------------------
        // 计时
        //------------------------------------------
        static Dictionary<DbCommand, DateTime> _dict = new Dictionary<DbCommand, DateTime>();

        // 记录开始执行时的时间
        private static void Start(DbCommand command)
        {
            if (!_dict.Keys.Contains(command))
                _dict.Add(command, DateTime.Now);
        }

        // 计算花费时间
        static TimeSpan Stop(DbCommand command)
        {
            if (!_dict.Keys.Contains(command))
                return TimeSpan.Zero;

            var startDt = _dict[command];
            _dict.Remove(command);
            return DateTime.Now - startDt;
        }

        // 日志
        private static void Log<T>(DbCommand command, DbCommandInterceptionContext<T> interceptionContext)
        {
            var duration = Stop(command);
            var txt = string.Format("{0}\r\n耗时：{1} ms", command.GetCommandText(), duration.TotalMilliseconds);
            if (duration.TotalSeconds > 1 || interceptionContext.Exception != null)
            {
                // 异常日志
                txt = string.Format("数据库异常：{0} \r\n{1} ", txt, interceptionContext.Exception?.Message);
                CoreConfig.Log("Database", txt, 1);  // 注意不能存储在数据库中，会造成无限递归错误
            }
            else
            {
                // 普通日志
                //Logger.Log(txt);
            }
        }

    }
}