using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace App.Core
{
    /// <summary>
    /// ������
    /// </summary>
    public static class EncryptionHelper
    {
        /// <summary>��ȡ�ַ��� MD5 ��ϣֵ��32λ��</summary>
        /// <param name="text"></param>
        /// <returns>�ַ���MD5��ϣֵ��ʮ�������ַ���</returns>
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

        /// <summary>��ȡ�ļ���MD5��ϣ��Ϣ</summary>
        /// <param name="filePath"></param>
        /// <returns>ʮ�������ַ���</returns>
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
