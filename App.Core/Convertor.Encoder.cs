using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace App.Core
{
    /// <summary>
    /// 编解码辅助
    /// </summary>
    public static partial class Convertor
    {
        //--------------------------------------------------
        // 字节数组和流
        //--------------------------------------------------
        /// <summary>对象转换为字节数组</summary>
        public static byte[] ToBytes(this object o)
        {
            if (o == null)
                return null;
            else
            {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter ser = new BinaryFormatter();
                ser.Serialize(ms, o);
                byte[] bytes = ms.ToArray();
                ms.Close();
                return bytes;
            }
        }

        /// <summary>字符串转换为字节数组</summary>
        public static byte[] ToBytes(this string txt, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return txt.IsEmpty() ? new byte[0] : encoding.GetBytes(txt);
        }

        /// <summary>将图像转换为字节数组</summary>
        public static byte[] ToBytes(this Image img)
        {
            if (img == null)
                return null;
            else
            {
                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Png);
                byte[] bytes = ms.ToArray();
                ms.Close();
                return bytes;
            }
        }

        /// <summary>将文本转化为流</summary>
        public static MemoryStream ToStream(this string text, Encoding encoding = null)
        {
            return text.ToBytes(encoding).ToStream();
        }

        /// <summary>将字节数组转化为流</summary>
        public static MemoryStream ToStream(this byte[] bytes)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }



        //--------------------------------------------------
        // 常用字符串编码转换
        //--------------------------------------------------
        /// <summary> url编码</summary> 
        public static string ToUrlEncode(this string text)
        {
            return HttpUtility.UrlEncode(text);
        }

        /// <summary> url解码</summary> 
        public static string ToUrlDecode(this string text)
        {
            return HttpUtility.UrlDecode(text);
        }

        /// <summary>进行Html编码。格式如：&amp;quot;Name&amp;quot;</summary>
        public static string ToHtmlEncode(this string text)
        {
            return HttpUtility.HtmlEncode(text);
        }

        /// <summary>进行Html反编码。格式如：&quot;Name&quot;</summary>
        public static string ToHtmlDecode(this string text)
        {
            return HttpUtility.HtmlDecode(text);
        }

        /// <summary>组装QueryString。如：a=1&b=2&c=3</summary>
        public static string ToQueryString(this Dictionary<string, string> data)
        {
            if (data == null || data.Count == 0)
                return "";
            StringBuilder sb = new StringBuilder();
            var i = 0;
            foreach (var item in data)
            {
                i++;
                sb.AppendFormat("{0}={1}", item.Key, item.Value);
                if (i < data.Count)
                    sb.Append("&");
            }
            return sb.ToString();
        }

        /// <summary>转化为Base64字符串</summary>
        public static string ToBase64String(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        /// <summary>解析Base64字符串</summary>
        public static byte[] ToBase64Bytes(this string text)
        {
            return Convert.FromBase64String(text);
        }

        /// <summary>将byte数组转化为16进制字符串</summary>
        public static string ToHexString(this byte[] bytes, bool insertSpace = true)
        {
            if ((bytes == null) || (bytes.Length == 0))
                return "";
            else
            {
                string format = insertSpace ? "{0:X2} " : "{0:X2}";
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    sb.Append(string.Format(format, bytes[i]));
                return sb.ToString();
            }
        }

        /// <summary>16进制字符串转化为字符串数组</summary>
        public static byte[] ToHexBytes(this string hex)
        {
            string[] hexs = hex.Trim().Split(' ');
            byte[] bytes = new byte[hexs.Length];
            for (int i = 0; i < hexs.Length; i++)
            {
                bytes[i] = (byte)(int.Parse(hexs[i]));
            }
            return bytes;
        }

    }
}