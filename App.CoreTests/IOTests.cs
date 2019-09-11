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
    public class IOTests
    {
        [TestMethod()]
        public void GetFileNameTest()
        {
            string url = "http://oa.wzcc.com/oamain.aspx?a=x&b=xx";
            var name = url.GetFileName();
            var ext = url.GetFileExtension();
            var folder = url.GetFileFolder();
            var q = url.GetQuery().ToString();
            var u = url.TrimQuery();
        }

        [TestMethod()]
        public void GetNextNameTest()
        {
            var name1 = "c:\\folder\\filename.doc?x=1";

            //
            var name2 = name1.GetNextName(@"_{0}").GetNextName(@"_{0}");
            Assert.AreEqual(name2, "c:\\folder\\filename_3.doc?x=1");

            //
            var name3 = name1.GetNextName(@"-{0}").GetNextName(@"-{0}");
            Assert.AreEqual(name3, "c:\\folder\\filename-3.doc?x=1");

            //
            var name4 = name1.GetNextName(@"({0})").GetNextName(@"({0})");
            Assert.AreEqual(name4, "c:\\folder\\filename(3).doc?x=1");
        }
    }
}