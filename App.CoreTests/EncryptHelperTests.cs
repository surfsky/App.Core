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
    public class EncryptHelperTests
    {
        [TestMethod()]
        public void DesEncryptTest()
        {
            var key = "12345678";
            var msg = "Hello world";
            var encrypt = msg.DesEncrypt(key);
            var decrypt = encrypt.DesDecrypt(key);
        }

        [TestMethod()]
        public void RSACreateKeyPairTest()
        {
            var msg = "hello world";
            var pair = EncryptHelper.RSACreateKeyPair();
            string encrytedMsg = EncryptHelper.RSAEncrypt(msg, pair.Key);
            string decrytedMsg = EncryptHelper.RSADecrypt(encrytedMsg, pair.Value);
        }

        [TestMethod()]
        public void MD5Test()
        {
            var txt = "Hello world!";
            var m3 = txt.MD5();        // "86FB269D190D2C85F6E0468CECA42A20"
            var m4 = txt.MD5();        // "86FB269D190D2C85F6E0468CECA42A20"
            var s = txt.SHA1();        // "D3486AE9136E7856BC42212385EA797094475802"
            var s2 = txt.HmacSHA256(); // "852D2FEC4BDA6ADD8F12C5C1DFF8420510AC5B85EF432140C7097AAEE3C270CA"

            var t2 = "expireDt=1571896546&name=190929.%E7%94%B5%E4%BF%A1%E7%9F%A5%E8%AF%86%E5%BA%93.sketch&nonceStr=6727658767&url=%2fFiles%2fArticles%2f191011-fb27021dfe9145adac4bde7f6f4b17b5.sketch&key=SignKey";
            var m1 = t2.MD5();         // "C6CEBD9247AAB3A6EDAA7629F404CC50"
            var m2 = t2.MD5();         // "C6CEBD9247AAB3A6EDAA7629F404CC50"
            Assert.AreEqual(m1, m2);
        }
    }
}