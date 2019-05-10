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
    public class EncryptHelperTests
    {
        [TestMethod()]
        public void ToMD5Test()
        {
            var txt = "Hello world!";
            var m = txt.ToMD5();  // "86fb269d190d2c85f6e0468ceca42a20"
            var s = txt.ToSHA1(); // "d3486ae9136e7856bc42212385ea797094475802"
        }
    }
}