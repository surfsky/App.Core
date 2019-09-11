﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Tests
{
    [TestClass()]
    public class MathHelperTests
    {
        [TestMethod()]
        public void IncTest()
        {
            int? n = null;
            n = n.Inc(1);
            IO.Trace(n.ToString());
        }

        [TestMethod()]
        public void EqualsTest()
        {
            bool b1 = MathHelper.Approx(1.15f, 1.16f, 0.01f);     // success
            bool b2 = MathHelper.Approx(1.153f, 1.152f, 0.001f);  // 
            Assert.IsTrue(b1);
            Assert.IsTrue(b2);
        }
    }
}