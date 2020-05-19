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
    public class PinYinTests
    {
        [TestMethod()]
        public void ToPinYinTest()
        {
            var txt = "你好,中国";
            Assert.AreEqual(txt.ToPinYin(),    "NiHao,ZhongGuo");
            Assert.AreEqual(txt.ToPinYinCap(), "NHZG");
        }
    }
}