using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils.Tests
{
    [TestClass()]
    public class SnowflakeIDTests
    {
        [TestMethod()]
        public void SnowflakeIDTest()
        {
            // 生成
            var snow = new SnowflakeID(1);
            var ids = new List<long>();
            for (int i = 0; i < 1000; i++)
            {
                long id = snow.NewID();
                ids.Add(id);
            }

            // 解析
            foreach (var id in ids)
            {
                IO.Write("{0} : {1}", id.ToString(), id.ToBitString());
                var snowId = SnowflakeID.Parse(1259605479504482304);
                var timestamp = snowId.TimeStamp;
                var machine = snowId.Machine;
                var sequence = snowId.Sequence;
            }
        }

        [TestMethod()]
        public  static void TestShift()
        {
            ulong n1 = 12;
            ulong n2 = 12 << 4;
            IO.Write("{0} {1}", n1.ToBitString(), n2.ToBitString());
        }
    }
}