using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils
{
    /// <summary>
    /// ID 生成器
    /// </summary>
    public static class IDGenerator
    {
        /// <summary>GUID</summary>
        public static string NewGuid(string format="N")
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>分布式雪花ID</summary>
        public static long NewSnowflakeID(int machine)
        {
            return new SnowflakeID(machine).NewID();
        }

        /// <summary>保留前10位GUID+6位时间戳，便于数据库索引排序</summary>
        public static string NewGuidCombo(int startYear=2000)
        {
            // 时间偏差（日期、毫秒）
            DateTime baseDate = new DateTime(startYear, 1, 1);
            DateTime now = DateTime.Now;
            int days = new TimeSpan(now.Ticks - baseDate.Ticks).Days;
            TimeSpan msecs = now.TimeOfDay;

            // GUID 128bit = 16byte
            byte[] bytes = Guid.NewGuid().ToByteArray();

            // Convert to a byte array
            // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333    
            byte[] daysBytes = BitConverter.GetBytes(days);
            byte[] msBytes   = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

            // Reverse the bytes to match SQL Servers ordering    
            Array.Reverse(daysBytes);
            Array.Reverse(msBytes);

            // 拷贝到GUID数组，天数占2字节，毫秒占4字节
            Array.Copy(daysBytes, daysBytes.Length - 2, bytes, bytes.Length - 6, 2);
            Array.Copy(msBytes,   msBytes.Length - 4,   bytes, bytes.Length - 4, 4);

            return bytes.ToHexString();
            //return new Guid(bytes).ToString("N");
        }
    }
}
