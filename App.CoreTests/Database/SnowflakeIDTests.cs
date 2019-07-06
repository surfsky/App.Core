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
            ulong n1 = 12;
            ulong n2 = 12 << 4;
            IO.Write("{0} {1}", n1.ToBitString(), n2.ToBitString());


            for (int i = 0; i < 1000; i++)
            {
                long id = new SnowflakeID(1).Value;
                IO.Write("{0} : {1}", id.ToString(), id.ToBitString());
            }
        }
    }
}