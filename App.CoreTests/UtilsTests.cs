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
        public void IsEmptyTest()
        {
            // string
            string text = null;
            Assert.IsTrue(text.IsEmpty());
            text = "";
            Assert.IsTrue(text.IsEmpty());
            text = "aa";
            Assert.IsTrue(text.IsNotEmpty());

            // list
            List<string> arr = null;
            Assert.IsTrue(arr.IsEmpty());
            arr = new List<string> { };
            Assert.IsTrue(arr.IsEmpty());
            arr = new List<string> { "aa" };
            Assert.IsTrue(arr.IsNotEmpty());

            // object
            Person p = null;
            Assert.IsTrue(p.IsEmpty());
            p = new Person();
            Assert.IsTrue(p.IsNotEmpty());

        }
        [TestMethod()]
        public void IIFTest()
        {
            var score = 2000;
            var result = score.IIF(t => t > 1000, "High", "Low");
        }

        [TestMethod()]
        public void IndexOfTest()
        {
            var items = new string[] { "ID", "Name", "Url" };
            var n = items.IndexOf(t => t == "Name");
            Assert.AreEqual(n, 1);
        }

        [TestMethod()]
        public void GetTextTest()
        {
            Assert.AreEqual(Utils.GetText("hello world"), "hello world");
            Assert.AreEqual(Utils.GetText("hello {0}", "world"), "hello world");
        }

        [TestMethod()]
        public void GetResTextTest()
        {
            // 简单测试
            var key = "Name";
            var resType = typeof(App.CoreTests.Properties.Resources);
            var text = key.GetResText(resType);
            Assert.AreEqual(text, "名称");

            // 全局化开关测试
            AppCoreConfig.Instance.UseGlobal = true;
            AppCoreConfig.Instance.ResType = typeof(App.CoreTests.Properties.Resources);
            Assert.AreEqual(key.GetResText(), "名称");

            AppCoreConfig.Instance.UseGlobal = false;
            Assert.AreEqual(key.GetResText(), "Name");
        }

        [TestMethod()]
        public void AsListTest()
        {
            var a = "test";
            var list =  a.AsList();
            Assert.AreEqual(list.Count, 1);

            bool? b = null;
            var list2 = b.AsList();
            Assert.AreEqual(list2.Count, 0);
        }
    }
}