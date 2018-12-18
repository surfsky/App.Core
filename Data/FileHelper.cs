﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// 文件相关辅助操作
    /// </summary>
    public class FileHelper
    {
        /// <summary>该文件是否是图片（根据扩展名）</summary>
        public static bool IsImageFile(string fileName)
        {
            string ext = GetFileExtension(fileName);
            if (ext.IsEmpty())
                return false;

            string[] exts = new string[] { ".jpg", ".png", ".gif", ".jpeg", ".bmp" };
            return exts.Contains(ext);
        }

        /// <summary>获取文件扩展名</summary>
        public static string GetFileExtension(string filePath)
        {
            if (filePath.IsEmpty()) return "";
            int n = filePath.LastIndexOf('.');
            if (n == -1) return "";
            return filePath.Substring(n).ToLower();
        }

        /// <summary>获取文件扩展名</summary>
        public static string GetFileName(string filePath)
        {
            if (filePath.IsEmpty()) return "";
            int n = filePath.LastIndexOf('.');
            if (n == -1) return "";
            return filePath.Substring(0, n);
        }

        /// <summary>获取文件扩展名</summary>
        public static string GetFileMimeType(string filePath)
        {
            var extension = GetFileExtension(filePath);
            return extension.IsEmpty() ? "" : GetMimeType(extension);
        }

        /// <summary>获取文件 MimeType</summary>
        public static string GetMimeType(string extension)
        {
            if (extension.IsEmpty())
                return "";
            switch (extension)
            {
                case ".jpg": return "image/jpeg";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".doc": return "application/msword";
                case ".xls": return "application/vnd.ms-excel";
                case ".ppt": return "application/vnd.ms-powerpoint";
                case ".exe": return "application/octet-stream";
                case ".pdf": return "application/pdf";
                default: return "";
            }
        }

        /// <summary>获取文件的MD5哈希信息</summary>
        /// <param name="filePath">文件物理路径</param>
        /// <returns>十六进制字符串</returns>
        public static string GetFileMD5(string filePath)
        {
            return EncryptionHelper.ToMD5(filePath);
        }
    }
}
