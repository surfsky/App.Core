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
    }
}