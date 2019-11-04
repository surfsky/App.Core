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
    }
}