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
    public class DateTimeHelperTests
    {
        [TestMethod()]
        public void ToTimeStampTest()
        {
            var now = DateTime.Now;
            var stamp = now.ToTimeStamp();
            var dt = stamp.ParseTimeStamp();
            Assert.AreEqual(now, dt);
        }
    }
}