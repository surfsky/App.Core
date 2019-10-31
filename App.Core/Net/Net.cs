using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// 网络相关
    /// </summary>
    public static class Net
    {
        /// <summary>获取本机IP地址列表（IPV4 及 IPV6）</summary>
        public static List<string> IPs
        {
            get
            {
                var name = Dns.GetHostName();
                var host = Dns.GetHostEntry(name);
                var addrs = host.AddressList.OrderBy(t => t.AddressFamily);
                return addrs.CastString();
            }
        }

    }
}
