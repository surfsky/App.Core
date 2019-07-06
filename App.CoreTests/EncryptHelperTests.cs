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
    }
}