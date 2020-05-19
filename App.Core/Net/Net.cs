using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils
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

        /// <summary>Ping</summary>
        public static bool Ping(string ip)
        {
            try
            {
                var p = new Ping();
                var options = new PingOptions();
                options.DontFragment = true;
                var data = "Test Data!";
                var buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 1000;
                var reply = p.Send(ip, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
