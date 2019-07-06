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
    public class IDGeneratorTests
    {
        [TestMethod()]
        public void NewGuidTest()
        {
            var id1 = IDGenerator.NewGuid();
            //var id2 = IDGenerator.NewGuidString();
            var id3 = IDGenerator.NewCombo();
            var id4 = IDGenerator.NewSnowflakeID(1);
        }
    }
}
