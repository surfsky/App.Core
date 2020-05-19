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
    public class ListHelperTests
    {
        [TestMethod()]
        public void GetItemTest()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>
            {
                { "a", "va"  },
                { "b", "vb"  },
                { "c", "vc"  },
            };
            Assert.AreEqual(dict.GetItem("A", true), "va");
            Assert.AreEqual(dict.GetItem("A", false), null);
        }
    }
}