using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils
{
    /// <summary>
    /// 字节数组辅助方法
    /// </summary>
    public static class ByteHelper
    {
        //--------------------------------------------
        // 字节数组操作
        //--------------------------------------------
        /// <summary>检测标志数据是否匹配</summary>
        public static bool MatchFlag(this byte[] bytes, byte[] flag, int start = 0)
        {
            int length = flag.Length;
            if (bytes.Length < length + start)
                return false;

            for (int i = 0; i < length; i++)
            {
                if (bytes[i + start] != flag[i])
                    return false;
            }
            return true;
        }

        /// <summary>创建字节数组并用指定数据填充</summary>
        public static byte[] NewBytes(long length, byte clearByte = 0x00)
        {
            byte[] bytes = new byte[length];
            ClearBytes(bytes, clearByte);
            return bytes;
        }

        /// <summary>清空字节数组</summary>
        public static void ClearBytes(this byte[] bytes, byte clearByte = 0x00)
        {
            int n = bytes.Length;
            for (int i = 0; i < n; i++)
                bytes[i] = clearByte;
        }

        //--------------------------------------------
        // 从字节数组中解析数据
        //--------------------------------------------
        /// <summary>截取部分字节数组</summary>
        public static byte[] GetBytes(this byte[] bytes, long start=0, long? length=null)
        {
            length = length ?? bytes.Length - start;
            if (length > bytes.Length - start)
                length = bytes.Length - start;

            var bytes2 = new byte[length.Value];
            Array.Copy(bytes, start, bytes2, 0, length.Value);
            return bytes2;
        }

        /// <summary>从字节数组中读取int数据</summary>
        public static int GetInt(this byte[] bytes, long start=0, long? length=null)
        {
            var bytes2 = GetBytes(bytes, start, length);
            return bytes2 == null ? 0 : BitConverter.ToInt32(bytes2, 0);
        }

        /// <summary>从字节数组中读取文本数据</summary>
        public static string GetText(this byte[] bytes, long start=0, long? length=null, Encoding encoding=null)
        {
            encoding = encoding ?? Encoding.UTF8;
            byte[] bytes2 = GetBytes(bytes, start, length);
            return encoding.GetString(bytes2);
        }

        /// <summary>从字节数组中读取文本数据</summary>
        public static string GetHexText(this byte[] bytes, long start=0, long? length=null)
        {
            byte[] bytes2 = GetBytes(bytes, start, length);
            return bytes2.ToHexString(true);
        }



        //--------------------------------------------
        // int <-> bytes
        //--------------------------------------------
        /// <summary>将数字数据转化为字节数组</summary>
        public static byte[] ToBytes(this long data, long length)
        {
            var dataBytes = BitConverter.GetBytes(data);
            var resultBytes = new byte[length];
            ClearBytes(resultBytes);
            var n = resultBytes.Length - dataBytes.Length;
            if (n >= 0)
                Array.Copy(dataBytes, 0, resultBytes, n, dataBytes.Length);
            return resultBytes;
        }

        //--------------------------------------------
        // string <-> bytes
        //--------------------------------------------
        /// <summary>将int数据转化为字符串后，再转化为字节数组。如："000072"</summary>
        public static byte[] ToTextBytes(this int data, long length, bool atStartOrEnd = false, string format = "{0}", byte clearByte = (byte)' ', Encoding encoding = null)
        {
            string txt = string.Format(format, data);
            return ToTextBytes(txt, length, atStartOrEnd, clearByte, encoding);
        }

        /// <summary>将Decimal数据转化为字符串后，再转化为字节数组。如："    78.00"</summary>
        public static byte[] ToTextBytes(this decimal data, long length, bool atStartOrEnd = false, string format = "{0:0.00}", byte clearByte = (byte)' ', Encoding encoding = null)
        {
            string txt = string.Format(format, data);
            return ToTextBytes(txt, length, atStartOrEnd, clearByte, encoding);
        }

        /// <summary>将文本转化为字节数组</summary>
        /// <param name="text">文本</param>
        /// <param name="length">字节数组长度</param>
        /// <param name="atStartOrEnd">文本放在开头还是结尾</param>
        /// <param name="clearByte">默认填充的字节值（默认为空格）</param>
        /// <param name="encoding">文本编码方式</param>
        /// <returns>构建的字节数组</returns>
        public static byte[] ToTextBytes(this string text, long length, bool atStartOrEnd = false, byte clearByte = (byte)' ', Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            byte[] bytes = NewBytes(length, clearByte);

            // 把字符串转化为数组，并拷贝到输出数组
            if (!string.IsNullOrEmpty(text))
            {
                byte[] bytes2 = encoding.GetBytes(text);
                int n = bytes2.Length;
                if (n >= length) Array.Copy(bytes2, bytes, length);
                else if (atStartOrEnd) Array.Copy(bytes2, bytes, n);
                else Array.Copy(bytes2, 0, bytes, length - n, n);
            }
            return bytes;
        }
    }
}
