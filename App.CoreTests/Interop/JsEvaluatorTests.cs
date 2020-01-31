using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Interop.Tests
{
    [TestClass()]
    public class JsEvaluatorTests
    {
        [TestMethod()]
        public void EvalTest()
        {
            var eval = new JsEvaluator();
            var b = eval.EvalBool("5 > 4");
            var d = eval.EvalDecimal("2.5");
            var o = eval.Eval("new Date()");
            var t = eval.EvalDateTime("new Date('2018/01/01 12:00:00')");
        }
    }
}