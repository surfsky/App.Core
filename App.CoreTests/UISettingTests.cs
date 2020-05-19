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
    public class UISettingTests
    {
        [TestMethod()]
        public void UISettingTest()
        {
            var setting = new UISetting(typeof(Person));

        }
    }
}