using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils.Tests
{
    [TestClass()]
    public class GZiperTests
    {
        [TestMethod()]
        public void ZipTest()
        {
            //var text = "folder=/a/b/c&mode=select";
            var text = "folder=/a/b/c&mode=selectfolder=/a/b/c&mode=selectfolder=/a/b/c&mode=selectfolder=/a/b/c&mode=selectfolder=/a/b/c&mode=selectfolder=/a/b/c&mode=selectfolder=/a/b/c&mode=selectfolder=/a/b/c&mode=selectfolder=/a/b/c&mode=select";
            var zip = text.Zip();
            var raw = zip.Unzip();
            Console.WriteLine(zip);
            Console.WriteLine(raw);
            Assert.AreEqual(text, raw);
        }
    }
}