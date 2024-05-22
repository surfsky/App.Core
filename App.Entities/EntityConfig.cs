using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Entities
{
    /// <summary>
    /// App.Core 类库配置信息
    /// </summary>
    public class EntityConfig
    {
        //---------------------------------------------
        // 单例
        //---------------------------------------------
        private static EntityConfig _cfg;
        public static EntityConfig Instance
        {
            get
            {
                if (_cfg == null)
                    _cfg = new EntityConfig();
                return _cfg;
            }
        }


        //---------------------------------------------
        // 事件
        //---------------------------------------------
        /// <summary>获取数据库上下文事件</summary>
        public event Func<DbContext> OnGetDb;

        /// <summary>数据库上下文（需配置 OnGetDb事件）</summary>
        public static DbContext Db => Instance.OnGetDb();
    }
}
