using System;
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
    public static partial class IO
    {
        //------------------------------------------------
        // 路径
        //------------------------------------------------
        /// <summary>准备文件路径（不存在则创建）</summary>
        /// <param name="filePath">文件的物理路径</param>
        public static void PrepareDirectory(string filePath)
        {
            var fi = new FileInfo(filePath);
            if (!Directory.Exists(fi.Directory.FullName))
                Directory.CreateDirectory(fi.Directory.FullName);
        }

        //------------------------------------------------
        // 文件存取
        //------------------------------------------------
        /// <summary>读取文件到字节数组</summary>
        public static byte[] GetBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
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

        /// <summary>写文件（附加）</summary>
        /// <param name="filePath">文件的物理路径</param>
        public static void WriteFile(string filePath, string data)
        {
            var fileinfo = new FileInfo(filePath);
            using (FileStream fs = fileinfo.OpenWrite())
            {
                var sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.Write(data);
                sw.Flush();
                sw.Close();
            }
        }

        /// <summary>合并文件</summary>
        /// <param name="files">源文件路径列表</param>
        /// <param name="mergeFile">合并文件路径</param>
        public static void MergeFiles(List<string> files, string mergeFile, bool deleteRawFiles = true)
        {
            if (File.Exists(mergeFile))
                File.Delete(mergeFile);

            using (FileStream stream = new FileStream(mergeFile, FileMode.OpenOrCreate))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    foreach (string file in files)
                    {
                        // 拷贝合并到新文件（并删除临时文件）
                        using (FileStream fileStream = new FileStream(file, FileMode.Open))
                        {
                            using (BinaryReader fileReader = new BinaryReader(fileStream))
                            {
                                byte[] bytes = fileReader.ReadBytes((int)fileStream.Length);
                                writer.Write(bytes);
                            }
                        }
                        if (deleteRawFiles)
                            File.Delete(file);
                    }
                }
            }
        }

        //------------------------------------------------
        // 文件名处理
        //------------------------------------------------
        /// <summary>获取查询字符串字典</summary>
        public static FreeDictionary<string, string> GetQuery(this string fileName)
        {
            Url url = new Url(fileName);
            return url.Dict;
        }

        /// <summary>去除尾部的查询字符串</summary>
        public static string TrimQuery(this string url)
        {
            if (url.IsEmpty())
                return "";
            int n = url.LastIndexOf('?');
            if (n != -1)
                return url.Substring(0, n);
            return url;
        }


        /// <summary>去除目录部分</summary>
        public static string TrimFolder(this string url)
        {
            if (url.IsEmpty())
                return "";
            int n = url.LastIndexOf('/');
            if (n != -1)
                url = url.Substring(n + 1);
            n = url.LastIndexOf('\\');
            if (n != -1)
                url = url.Substring(n + 1);
            return url;
        }

        /// <summary>获取文件名（去掉路径和查询字符串）</summary>
        public static string GetFileName(this string url)
        {
            if (url.IsEmpty())
                return "";
            return url.TrimQuery().TrimFolder();
        }

        /// <summary>获取文件扩展名</summary>
        public static string GetFileExtension(this string fileName)
        {
            if (fileName.IsEmpty())
                return "";
            fileName = fileName.TrimQuery();
            int n = fileName.LastIndexOf('.');
            if (n != -1)
                return fileName.Substring(n).ToLower();
            return "";
        }

        /// <summary>该文件是否是图片（根据扩展名）</summary>
        public static bool IsImageFile(this string fileName)
        {
            string ext = GetFileExtension(fileName);
            if (ext.IsEmpty())
                return false;

            string[] exts = new string[] { ".jpg", ".png", ".gif", ".jpeg", ".bmp", ".tif" };
            return exts.Contains(ext);
        }

        /// <summary>获取文件 MimeType</summary>
        public static string GetMimeType(this string fileName)
        {
            var ext = GetFileExtension(fileName);
            if (ext.IsEmpty())
                return "";
            switch (ext)
            {
                case ".jpg": return "image/jpeg";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".doc": return "application/msword";
                case ".xls": return "application/vnd.ms-excel";
                case ".ppt": return "application/vnd.ms-powerpoint";
                case ".exe": return "application/octet-stream";
                case ".pdf": return "application/pdf";
                case ".mp3": return "audio/mp3";
                case ".mp4": return "vidio/mp4";
                default: return "";
            }
        }
    }
}
