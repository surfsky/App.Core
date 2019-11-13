﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace App.Core.Tests
{
    [TestClass()]
    public class ParamAttributeTests
    {
        [TestMethod()]
        [Param("p1", "description1", true)]
        [Param("p1", "description12", typeof(string), false)]
        public void ParamAttributeTest()
        {
            var method = ReflectionHelper.GetCurrentMethod() as MethodInfo;
            var attrs = method.GetAttributes<ParamAttribute>();
            Assert.AreEqual(attrs.Count, 2);

            var attr = method.GetAttribute<ParamAttribute>();
            Assert.AreEqual(attr.Name, "p1");
        }
    }
}