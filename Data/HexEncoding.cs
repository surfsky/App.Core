using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// 16 进制编码器
    /// </summary>
    public class HexEncoding : ASCIIEncoding
    {
        public static HexEncoding Instance = new HexEncoding();

        public override byte[] GetBytes(string s)
        {
            return ToHexBytes(s);
        }

        public override string GetString(byte[] bytes)
        {
            return ToHexString(bytes);
        }

        public override string EncodingName
        {
            get { return "HEX"; }
        }

        public override string ToString()
        {
            return "HEX";
        }

        //-----------------------------------
        // 编码
        //-----------------------------------
        /// <summary>
        /// 将byte数组转化为16进制字符串
        /// </summary>
        public static string ToHexString(byte[] bytes, bool insertSpace = true)
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

        /// <summary>
        /// 16进制字符串转化为字符串数组
        /// </summary>
        public static byte[] ToHexBytes(string hex)
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
