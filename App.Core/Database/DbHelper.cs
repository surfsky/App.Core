using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils
{
    /// <summary>
    /// 数据库操作相关类
    /// </summary>
    public static class DbHelper
    {
        /// <summary>获取命令文本（自动拼装参数）</summary>
        public static string GetCommandText(this DbCommand command)
        {
            //var sb = new StringBuilder();
            //foreach (DbParameter param in command.Parameters)
            //    sb.AppendFormat("{1} {0} = {2};\r\n", param.ParameterName, param.DbType, param.Value);

            string sql = command.CommandText;
            return command.Parameters
                .Cast<DbParameter>()
                .Aggregate(sql, (name, p) => name.Replace(p.ParameterName, p.Value.ToString()))
                ;
        }
    }
}
