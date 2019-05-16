using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    /// <summary>
    /// 可安全访问的字典。对于dict["key"], 如果键不存在则返回null，而不报异常
    /// </summary>
    public class FreeDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        /// <summary>获取或设置查询字符串成员</summary>
        public new TValue this[TKey key]
        {
            get
            {
                if (this.Keys.Contains(key))
                    return base[key];
                return default(TValue);
            }
            set { base[key] = value; }
        }
    }

}
