using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace App.Core
{
    /// <summary>
    /// 加密类
    /// </summary>
    public static class EncryptionHelper
    {
        /// <summary>获取字符串 MD5 哈希值（32位）</summary>
        /// <param name="text"></param>
        /// <returns>字符串MD5哈希值的十六进制字符串</returns>
        public static string ToMD5(this string text, Encoding encoding=null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(encoding.GetBytes(text));

            var sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
                sb.AppendFormat("{0:x2}", bytes[i]);
            return sb.ToString();
        }

        /// <summary>获取文件的MD5哈希信息</summary>
        /// <param name="filePath"></param>
        /// <returns>十六进制字符串</returns>
        public static string GetFileMD5(string filePath)
        {
            var file = new FileStream(filePath, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(file);
            file.Close();

            var sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
                sb.AppendFormat("{0:x2}", bytes[i]);
            return sb.ToString();
        }
    }
}
