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
    public class StringHelperTests
    {
        [TestMethod()]
        public void TrimEndTest()
        {
            var t1 = "ProductNameID";
            var t2 = t1.ReplaceRegex("Name", "Key");
            var t3 = t2.TrimEnd("ID");
        }

        [TestMethod()]
        public void RemoveBlankTest()
        {
            var t1 = @"
                <script>ProductNameID</script>
                <!- sfdsfdsfdsfdremark ->
                Text \r\n Enter \v\f ProductNameIDProductNameIDProductNameIDProductNameID
                etc 
                ";
            var t2 = t1.RemoveHtml();
            var t3 = t1.RemoveBlank();
            var t4 = t1.RemoveBlankTranslator();
            var t5 = t1.Slim();
            var t6 = t1.RemoveTag();

            var t7 = t1.RemoveHtml().Slim().RemoveBlankTranslator().Summary(20);
            var t10 = t1.RemoveTag().RemoveBlankTranslator().Slim().Summary(20);
        }

        [TestMethod()]
        public void QuoteTest()
        {
            var t1 = "\"Text is 'text' \"  is \r\n 'Enter' \v\f \t Prtempl";
            var t2 = t1.Quote();
            var t3 = t2.Unquote();
            Assert.AreEqual(t3, t1);
            var t4 = t1.Escape('t');
            var t5 = t4.Unescape();
            Assert.AreEqual(t5, t1);
        }

        [TestMethod()]
        public void RemoveHtmlTest()
        {
            var t1 = @"
hello world
<script>
function do() {
    console.write('hello world');
}
</script>
<style>
@font-face {
	font-family: 宋体;
}
</style>
";
            var t2 = t1.RemoveHtml();
            var t3 = t1.RemoveStyleBlock();
            var t4 = t1.RemoveStyleBlock();
            Assert.AreEqual(t2, "hello world");
        }
    }
}