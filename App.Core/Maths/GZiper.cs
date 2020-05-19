using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils
{
    /// <summary>
    /// GZip压缩解压辅助类
    /// </summary>
    public static class GZiper
    {
        /// <summary>GZip压缩</summary>
        public static string Zip(this string str)
        {
            var rawBytes = Encoding.GetEncoding("UTF-8").GetBytes(str);
            var zipBytes = Zip(rawBytes);
            return zipBytes.ToBase64String();
        }

        /// <summary>解压缩字符串</summary>
        public static string Unzip(this string str)
        {
            var zipBytes = str.ToBase64Bytes();
            var unzipBytes = Unzip(zipBytes);
            return Encoding.GetEncoding("UTF-8").GetString(unzipBytes);
        }

        /// <summary>GZip解压缩</summary>
        private static byte[] Zip(this byte[] data)
        {
            var ms = new MemoryStream();
            var zip = new GZipStream(ms, CompressionMode.Compress, true);
            zip.Write(data, 0, data.Length);
            zip.Close();
            var buffer = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buffer, 0, buffer.Length);
            ms.Close();
            return buffer;
        }

        /// <summary>GZip解压缩</summary>
        private static byte[] Unzip(this byte[] data)
        {
            var ms = new MemoryStream(data);
            var zip = new GZipStream(ms, CompressionMode.Decompress, true);
            var msreader = new MemoryStream();
            var buffer = new byte[0x1000];
            while (true)
            {
                var reader = zip.Read(buffer, 0, buffer.Length);
                if (reader <= 0)
                    break;
                msreader.Write(buffer, 0, reader);
            }
            zip.Close();
            ms.Close();
            msreader.Position = 0;
            buffer = msreader.ToArray();
            msreader.Close();
            return buffer;
        }
    }
}
