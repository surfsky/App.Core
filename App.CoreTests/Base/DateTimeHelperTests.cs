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
    public class DateTimeHelperTests
    {
        [TestMethod()]
        public void ToTimeStampTest()
        {
            var now = DateTime.Now;
            var stamp = now.ToTimeStamp();
            var dt = stamp.ParseTimeStamp();
            Assert.AreEqual(now.ToString(), dt.ToString());
        }

        [TestMethod()]
        public void ToFriendlyTextTest()
        {
            var now = DateTime.Now;
            var d1 = now.AddDays(-5);
            var d2 = now.AddDays(5);
            Assert.AreEqual(d2.ToFriendlyText(), d2.ToString("yyyy-MM-dd"));
            Assert.AreEqual(now.AddDays(2).ToFriendlyText(), "2天后");
            Assert.AreEqual(now.AddHours(5).ToFriendlyText(), "5小时后");
            Assert.AreEqual(now.AddMinutes(15).ToFriendlyText(), "15分钟后");
            Assert.AreEqual(now.AddMinutes(5).ToFriendlyText(), "马上");
            Assert.AreEqual(now.AddMinutes(-5).ToFriendlyText(), "刚刚");
            Assert.AreEqual(now.AddMinutes(-15).ToFriendlyText(), "15分钟前");
            Assert.AreEqual(now.AddHours(-5).ToFriendlyText(), "5小时前");
            Assert.AreEqual(now.AddDays(-2).ToFriendlyText(), "2天前");
            Assert.AreEqual(d1.ToFriendlyText(), d1.ToString("yyyy-MM-dd"));
        }
    }
}