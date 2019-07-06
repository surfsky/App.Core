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
    public class UtilsTests
    {
        [TestMethod()]
        public void IIFTest()
        {
            var score = 2000;
            var result = score.IIF(t => t > 1000, "High", "Low");
        }

        [TestMethod()]
        public void HasBitTest()
        {
            SexType all = SexType.Male | SexType.Female;
            bool isMale = all.HasFlag(SexType.Male);
        }
    }
}