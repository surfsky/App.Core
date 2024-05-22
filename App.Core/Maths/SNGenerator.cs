using System;
using System.Security.Cryptography;
using System.Text;

namespace App.Core
{
    /// <summary>
    /// 序列号生成器
    /// </summary>
    public class SNGenerator
    {
        /// <summary>生成序列号。结果是30位的字符串，如：arW5OyJKgqz6DfQKB/A0uQ==C6D999</summary>
        public static string Generate(string key)
        {
            string random = Guid.NewGuid().ToString().Substring(0, 10);
            string text = random.DesEncrypt(key);   // 24 个字符
            string hash = random.MD5().Substring(0, 6).ToLower();  // 6 个字符
            return $"{text}{hash}";
        }

        /// <summary>校验序列号</summary>
        public static bool Validate(string sn, string key)
        {
            if (string.IsNullOrEmpty(sn))
                return false;

            string text = sn.Substring(0, 24);    // 24 个字符
            string hash = sn.Substring(24);       // 6 个字符
            string hash2 = text.DesDecrypt(key).MD5().Substring(0, 6).ToLower();
            return hash == hash2;
        }
    }
}
