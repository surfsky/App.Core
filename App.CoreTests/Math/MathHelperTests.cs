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
    public class MathHelperTests
    {
        [TestMethod()]
        public void IncTest()
        {
            int? n = null;
            n = n.Inc(1);
            Assert.AreEqual(n, 1);

            int? i = 5;
            i = i.Inc(1);
            Assert.AreEqual(i, 6);

            i = 5;
            i = i.Inc(5, 8);
            Assert.AreEqual(i, 8);

            var p = new Person("A");
            p.Age = 9;
            //p.Age.Inc(1);  // 属性或索引不能作为 out 或 ref 参数传递
        }

        [TestMethod()]
        public void EqualsTest()
        {
            bool b1 = MathHelper.Approx(1.15f, 1.16f, 0.01f);     // success
            bool b2 = MathHelper.Approx(1.153f, 1.152f, 0.001f);  // 
            Assert.IsTrue(b1);
            Assert.IsTrue(b2);
        }


        [TestMethod()]
        public void ToChinaNumberTest()
        {
            decimal d = 1_2345_6789.456M;
            var t = d.ToChinaNumber();
        }
    }
}