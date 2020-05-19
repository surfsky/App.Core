using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Utils;

namespace App.Core.Tests
{
    [TestClass()]
    public class CsEvaluatorTests
    {
        [TestMethod()]
        public void EvalTest()
        {
            var eval = new CsEvaluator();
            var b = eval.EvalBool("5 > 4");
            var d = eval.EvalDecimal("2.5");
            var o = eval.Eval("new DateTime(2018,1,1)");
            var t = eval.EvalDateTime("new DateTime(2018,1,1)");
        }
    }
}