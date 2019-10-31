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
    public class IOTests
    {
        [TestMethod()]
        public void GetFileNameTest()
        {
            string url = "http://oa.wzcc.com/oamain.aspx?a=x&b=xx";
            var name = url.GetFileName();
            var ext = url.GetFileExtension();
            var folder = url.GetFileFolder();
            var q = url.GetQuery().ToString();
            var u = url.TrimQuery();
        }

        [TestMethod()]
        public void GetNextNameTest()
        {
            var name1 = "c:\\folder\\filename.doc?x=1";

            //
            var name2 = name1.GetNextName(@"_{0}").GetNextName(@"_{0}");
            Assert.AreEqual(name2, "c:\\folder\\filename_3.doc?x=1");

            //
            var name3 = name1.GetNextName(@"-{0}").GetNextName(@"-{0}");
            Assert.AreEqual(name3, "c:\\folder\\filename-3.doc?x=1");

            //
            var name4 = name1.GetNextName(@"({0})").GetNextName(@"({0})");
            Assert.AreEqual(name4, "c:\\folder\\filename(3).doc?x=1");
        }

        [TestMethod()]
        public void GetCacheTest()
        {
            var key = "Name";
            var name1 = IO.GetCache(key, () => "Kevin");
            var name2 = IO.GetCache<string>(key);
            Assert.AreEqual(name1, name2);
            IO.SetCache(key, "John");
            var name3 = IO.GetCache<string>(key);
            Assert.AreEqual(name3, "John");
        }

        [TestMethod()]
        public void GetMimeTypeTest()
        {
            var url1 = "xxx.jpg?test=x";
            var url2 = "/a/b/xxx.doc?test=x";
            var url3 = "/a/b/xxx.xxx?test=x";
            var url4 = "/a/b/xxx?test=x";

            Assert.AreEqual(url1.GetMimeType(), @"image/jpeg");
            Assert.AreEqual(url2.GetMimeType(), @"application/msword");
            Assert.AreEqual(url3.GetMimeType(), "");
            Assert.AreEqual(url4.GetMimeType(), "");
        }

        [TestMethod()]
        public void GetFileFolderTest()
        {
            var url1 = @"xxx.jpg?test=x";
            var url2 = @"a/b/xxx.jpg?test=x";
            var url3 = @"a\b\xxx.jpg?test=x";
            Assert.AreEqual(url1.GetFileFolder(), "");
            Assert.AreEqual(url2.GetFileFolder(), @"a/b");
            Assert.AreEqual(url3.GetFileFolder(), @"a\b");
        }

        [TestMethod()]
        public void GetFileExtensionTest()
        {
            var url1 = @"xxx.jpg?test=x";
            var url2 = @"a/b/xxx.Jpg?test=x";
            var url3 = @"a\b\xxx?test=x";
            Assert.AreEqual(url1.GetFileExtension(), @".jpg");
            Assert.AreEqual(url2.GetFileExtension(), @".jpg");
            Assert.AreEqual(url3.GetFileExtension(), @"");
        }

        [TestMethod()]
        public void GetAppSettingTest()
        {
            var o1 = IO.GetAppSetting<int?>("MachineID");
            var o2 = IO.GetAppSetting<int?>("machineID");
            var o3 = IO.GetAppSetting<int?>("NotExist");
            Assert.AreEqual(o1, o2);
            Assert.AreEqual(o3, null);
        }

        [TestMethod()]
        public void IPsTest()
        {
            var ips = Net.IPs;
            ips.ForEach(t => IO.Trace(t));
        }

    }
}