using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Tests
{
    [TestClass()]
    public class SnowflakeIDTests
    {
        [TestMethod()]
        public void SnowflakeIDTest()
        {
            // 生成
            for (int i = 0; i < 1000; i++)
            {
                long id = SnowflakeID.NewID();
                IO.Write("{0} : {1}", id.ToString(), id.ToBitString());
            }

            // 解析
            var snowId = SnowflakeID.Parse(1259605479504482304);
            var timestamp = snowId.TimeStamp;
            var machine = snowId.Machine;
            var sequence = snowId.Sequence;
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