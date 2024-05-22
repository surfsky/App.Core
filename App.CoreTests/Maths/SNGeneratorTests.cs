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
    public class SNGeneratorTests
    {
        [TestMethod()]
        public void GenerateTest()
        {
            var sn = SNGenerator.Generate("12345678");
            var b  = SNGenerator.Validate(sn, "12345678");
            Assert.IsTrue(b);
        }
    }
}