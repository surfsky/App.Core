using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Tests
{
    public class User
    {
        public string Name { get; set; }
        public string Sex { get; set; }
    }

    [TestClass()]
    public class SerializeHelperTests
    {
        [TestMethod()]
        public void ToJsonTest()
        {
            var user = new User() { Name = "kevin", Sex = "Male" };
            var txt = user.ToJson();
            System.Diagnostics.Trace.Write(txt);
        }
    }
}