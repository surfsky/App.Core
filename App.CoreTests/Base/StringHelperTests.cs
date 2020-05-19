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
    public class StringHelperTests
    {
        [TestMethod()]
        public void TrimEndTest()
        {
            Assert.AreEqual("ProductNameID".TrimEnd("ID"), "ProductName");
            Assert.AreEqual("ProductNameID".TrimEnd("Name"), "Product");
            Assert.AreEqual("Product$".TrimEnd("$"), "Product");
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


        [TestMethod()]
        public void SplitTest()
        {
            var t1 = "1,2,3,4,5";
            var t2 = "1 2 3 4 5";
            var a1 = t1.SplitLong();
            var a2 = t2.SplitString();
        }

        [TestMethod()]
        public void SubTextTest()
        {
            var text = "0123456789";
            Assert.AreEqual(text.SubText(0, 8), "01234567");
            Assert.AreEqual(text.SubText(0, 10), "0123456789");
            Assert.AreEqual(text.SubText(0, 12), "0123456789");
            Assert.AreEqual(text.SubText(0, 5), "01234");
            Assert.AreEqual(text.SubText(5, 12), "56789");
            Assert.AreEqual(text.SubText(0, 12), "0123456789");
        }

        [TestMethod()]
        public void ContainsTest()
        {
            var str = "Hello world";
            Assert.IsTrue(str.Contains("Hello", true));
            Assert.IsTrue(str.Contains("hello", true));
            Assert.IsFalse(str.Contains("hello", false));
            Assert.IsFalse(str.Contains("", true));
        }

        [TestMethod()]
        public void ToSizeTextTest()
        {
            long size1 = 786;
            long size2 = (long)(15.78 * 1024);
            long size3 = (long)(15.70 * 1024 * 1024);
            long size4 = (long)(15.782 * 1024 * 1024 * 1024);
            long size5 = (long)(15.786 * 1024 * 1024 * 1024 * 1024);
            Assert.AreEqual(size1.ToSizeText(), "786 bytes");
            Assert.AreEqual(size2.ToSizeText(), "15.78 KB");
            Assert.AreEqual(size3.ToSizeText(), "15.7 MB");
            Assert.AreEqual(size4.ToSizeText(), "15.78 GB");
            Assert.AreEqual(size5.ToSizeText(), "15.79 TB");

            Assert.AreEqual(size3.ToSizeText("{0:0.00}"), "15.70 MB");
        }

        [TestMethod()]
        public void AddQueryStringTest()
        {
            var url = "a.aspx?a=1";
            var q = "a=2&b=3";
            Assert.AreEqual(url.AddQueryString(q), "a.aspx?a=2&b=3");


            var url2 = "a=1";
            var q2 = "a=2&b=3";
            Assert.AreEqual(url2.AddQueryString(q2), "a=2&b=3");
        }

        [TestMethod()]
        public void SetQueryStringTest()
        {
            Assert.AreEqual("a.aspx?a=1".SetQueryString("a", "2"), "a.aspx?a=2");
            Assert.AreEqual("a.aspx?a=1".SetQueryString("b", "2"), "a.aspx?a=1&b=2");
            Assert.AreEqual("a=1".SetQueryString("b", "2"), "a=1&b=2");
        }


        [TestMethod()]
        public void TrimStartToTest()
        {
            Assert.AreEqual("a.asp".TrimStart(".", false), "asp");
            Assert.AreEqual("a.asp".TrimStart(".", true), ".asp");
            Assert.AreEqual("a.".TrimStart("."), "");
            Assert.AreEqual("asp".TrimStart("."), "asp");
        }

        [TestMethod()]
        public void TrimEndToTest()
        {
            Assert.AreEqual("a.asp".TrimEnd(".").TrimEnd(".", false), "a");
            Assert.AreEqual("a.asp".TrimEnd(".", true), "a.");
        }

        [TestMethod()]
        public void GetStartTest()
        {
            Assert.AreEqual("a.aspx?a=1".GetStart(".", false), "a");
            Assert.AreEqual("a.aspx?a=1".GetStart(".", true), "a.");
        }

        [TestMethod()]
        public void GetEndTest()
        {
            Assert.AreEqual("a.aspx?a=1".GetEnd(".", false), "aspx?a=1");
            Assert.AreEqual("a.aspx?a=1".GetEnd(".", true), ".aspx?a=1");
        }

        [TestMethod()]
        public void TrimLinesTest()
        {
            var t = @"
                <head>
                <meta charset=""utf-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
                <script src=""{JqueryJs}""></script>
                <script src=""{PopperJs}""></script>
                <script src=""{BootStrapJs}""></script>
                <head>
                ";
            t = t.TrimLines();
        }
    }
}