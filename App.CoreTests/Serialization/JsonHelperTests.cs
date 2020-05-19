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
    public class JsonHelperTests
    {
        [TestMethod()]
        public void ParseJsonTest()
        {
            Person p = new Person("Kevin");
            var txt = p.ToJson();
            var o = txt.ParseJson<Person>();
            Assert.AreEqual(p.Name, o.Name);

            txt = "blabla";
            o = txt.ParseJson<Person>(true, null);
            Assert.AreEqual(o, null);
        }
    }
}