using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// 配置信息
    /// </summary>
    public class AppCoreConfig
    {
        /// <summary>是否启用本地化字符串</summary>
        public bool UseGlobal { get; set; } = false;

        /// <summary>资源名称</summary>
        public Type ResType { get; set; }

        // 单例
        private static AppCoreConfig _cfg;
        public static AppCoreConfig Instance
        {
            get
            {
                if (_cfg == null)
                    _cfg = new AppCoreConfig();
                return _cfg;
            }
        }
    }
}
