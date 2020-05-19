using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils
{
    /// <summary>
    /// 16 进制编码器
    /// </summary>
    public class HexEncoding : ASCIIEncoding
    {
        public static HexEncoding Instance = new HexEncoding();

        public override string EncodingName
        {
            get { return "HEX"; }
        }

        public override byte[] GetBytes(string s)
        {
            return s.ToHexBytes();
        }

        public override string GetString(byte[] bytes)
        {
            return bytes.ToHexString(false);
        }
    }
}
