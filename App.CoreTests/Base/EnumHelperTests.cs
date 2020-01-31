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
    public class EnumHelperTests
    {
        [TestMethod()]
        public void GetEnumInfoTest()
        {
            var info = SexType.Male.GetEnumInfo();
        }
    }
}