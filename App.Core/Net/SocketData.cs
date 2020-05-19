using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Utils;

namespace App.Utils
{
    /// <summary>
    /// 通讯数据基类
    /// </summary>
    public class SocketData
    {
        // 属性
        public Encoding Encoding { get; set; }      // 使用的文本编码
        public byte[] Bytes { get; set; }           // 字节流
        public string Text { get; set; }            // 字节流对应的文本

        // 构造方法
        public SocketData() { }
        public SocketData(byte[] bytes, Encoding encoding=null) { SetData(bytes, encoding); }
        public SocketData(string txt, Encoding encoding=null) { SetData(txt, encoding); }
        public SocketData(long data, long length=4) { SetData(data, length); }

        // 设置数据
        public void SetData(byte[] bytes, Encoding encoding=null)
        {
            this.Encoding = encoding ?? Encoding.UTF8;
            this.Bytes = bytes;
            this.Text = encoding.GetString(bytes);
        }
        public void SetData(string txt, Encoding encoding=null)
        {
            this.Encoding = encoding ?? Encoding.UTF8;
            this.Text = txt;
            this.Bytes = encoding.GetBytes(txt);
        }
        public void SetData(long data, long length=4)
        {
            this.Encoding = Encoding.UTF8;
            this.Bytes = data.ToBytes(length);
            this.Text = this.Encoding.GetString(this.Bytes);
        }

        // 获取16进制文本
        public string GetHex(int length)
        {
            return string.Format("{0} {1}",
                this.Bytes.GetBytes(0, length).ToHexString(),
                this.Bytes.Length > 128 ? "..." : ""
                );
        }
    }
}
