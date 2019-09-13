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
    public class RegexHelperTests
    {
        [TestMethod()]
        public void IsMatchTest()
        {
            Assert.IsTrue(RegexHelper.IsMatch("AABB", RegexHelper.AABB));
            Assert.IsTrue(RegexHelper.IsMatch("开开心心", RegexHelper.AABB));
            Assert.IsTrue(RegexHelper.IsMatch("ABAB", RegexHelper.ABAB));
            Assert.IsTrue(RegexHelper.IsMatch("快活快活", RegexHelper.ABAB));
            Assert.IsTrue(RegexHelper.IsMatch("0", RegexHelper.Int));
            Assert.IsTrue(RegexHelper.IsMatch("90", RegexHelper.Int));
            Assert.IsTrue(RegexHelper.IsMatch("90.8", RegexHelper.Float));
            Assert.IsTrue(RegexHelper.IsMatch("-0", RegexHelper.Int));
            Assert.IsTrue(RegexHelper.IsMatch("-1", RegexHelper.NegativeInt));
        }

        [TestMethod()]
        public void SearchTest()
        {
            var text = @"<html><title>title</title></html>";
            var reg = @"<title>(?<title>.*?)</title>";
            Assert.AreEqual(text.Search(reg, "title"), "title");
            Assert.AreEqual(text.Search(reg, "${title}"), "title");
            Assert.AreEqual(text.Search(reg, "$1"), "title");
            Assert.AreEqual(text.Search(reg), "<title>title</title>");
        }


        [TestMethod()]
        public void ReplaceRegexTest()
        {
            var txt = "03/01/2019";
            var day1 = txt.ReplaceRegex(
                @"\b(\d{1,2})/(\d{1,2})/(\d{2,4})\b",
                "$3-$1-$2"
               );
            var day2 = txt.ReplaceRegex(
                 @"\b(?<month>\d{1,2})/(?<day>\d{1,2})/(?<year>\d{2,4})\b",
                 "${year}-${month}-${day}"
                );
            Assert.AreEqual(day1, "2019-03-01");
            Assert.AreEqual(day2, "2019-03-01");
        }

        [TestMethod()]
        public void FindWordsTest()
        {
            var text = "hello world,你好+世界, '测试+System'";
            var list = RegexHelper.FindWords(text);
            Assert.AreEqual(list.Count, 6);
        }

        [TestMethod()]
        public void FindReplicatedWordTest()
        {
            var txt = "Test  Apple Test Apple,Test Kitty Apple Test Apple";
            var list = RegexHelper.FindReplicatedWord(txt);
            Assert.AreEqual(list.Count, 2);
        }


        [TestMethod()]
        public void ParseUrlTest()
        {
            RegexHelper.ParseUrl("http://www.test.com:8080/", out string proto, out string host, out string port);
            Assert.AreEqual(proto, "http");
            Assert.AreEqual(host, "www.test.com");
            Assert.AreEqual(port, "8080");
            RegexHelper.ParseUrl("http://www.test.com/", out string proto2, out string host2, out string port2);
            Assert.AreEqual(proto2, "http");
            Assert.AreEqual(host2, "www.test.com");
            Assert.AreEqual(port2, "");
        }

        [TestMethod()]
        public void MMDDYY2YYMMDDTest()
        {
            Assert.AreEqual(RegexHelper.MMDDYY2YYMMDD("03/01/2019"), "2019-03-01");
        }

        [TestMethod()]
        public void ParseHyperlinkTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ParseTitleTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ClearScriptTest()
        {
            Assert.Fail();
        }



        [TestMethod()]
        public void ParseFileExtensionTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ParseHtmlUrlTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void LikeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetXmlTagsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetXmlTagTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetXmlTagAttributeTest()
        {
            Assert.Fail();
        }
    }
}