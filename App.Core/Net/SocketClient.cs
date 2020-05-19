using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using System.IO;

namespace App.Utils
{
    /// <summary>
    /// Socket 客户端
    /// 请自行捕捉异常
    /// 请设置sendTimeout、receiveTimeOut属性以实现同步+超时逻辑
    /// </summary>
    public class SocketClient : Socket
    {
        public string Host { get; set; }
        public int Port { get; set; }

        //
        public SocketClient(string host, int port, int sendTimeout = 0, int receiveTimeout = 0)
           : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            this.SendTimeout = sendTimeout;       // 限时未发送成功会触发异常
            this.ReceiveTimeout = receiveTimeout; // 限时未接收成功会触发异常
            this.Host = host;
            this.Port = port;
        }

        // 连接
        public void Connect()
        {
            this.Connect(this.Host, this.Port);
        }

        //------------------------------------------------
        // 发送
        //------------------------------------------------
        /// <summary>
        /// 发送文本（默认用UTF-8编码发送）
        /// </summary>
        public int Send(string text, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return Send(encoding.GetBytes(text));
        }
        public new int Send(byte[] bytes)
        {
            OnSend?.Invoke(new SocketData(bytes));
            return Send(bytes, bytes.Length, 0);
        }

        public event Action<SocketData> OnSend;
        public event Action<SocketData> OnReceive;

        //------------------------------------------------
        // 接收
        //------------------------------------------------
        // 接收文本
        public string ReceiveText(Encoding encoding, int size = 10240)
        {
            byte[] bytes = ReceiveBytes(size);
            return encoding.GetString(bytes);
        }

        /// <summary>接收字节数据。循环读取数据，到读满数据或超时为止</summary>
        public byte[] ReceiveBytes(int size = 10240, bool loopRead=false)
        {
            int position = 0;
            byte[] buffer = new byte[size];
            NetworkStream stream = new NetworkStream(this);
            while (position < size)
            {
                int readSize = Math.Min(buffer.Length - position, 10240);
                int n = stream.Read(buffer, position, readSize);
                if (n <= 0)
                    break;
                position += n;
                if (!loopRead)
                    break;
            }

            // 裁剪真实数据部分
            byte[] result = new byte[position];
            Array.Copy(buffer, 0, result, 0, position);
            OnReceive?.Invoke(new SocketData(result));
            return result;
        }


        /// <summary>接收文件（用流边读边写）</summary>
        public int ReceiveFile(int fileSize, string localFilePath)
        {
            FileInfo file = new FileInfo(localFilePath);
            using (FileStream writer = file.Open(FileMode.Create, FileAccess.Write, FileShare.None))
            {
                int len = 0;
                byte[] buffer = new byte[10240];
                NetworkStream ns = new NetworkStream(this);
                while (len < fileSize)
                {
                    int n = ns.Read(buffer, 0, buffer.Length);
                    if (n <= 0)
                        break;
                    writer.Write(buffer, 0, n);
                    len += n;
                }
                writer.Close();
                return len;
            }
        }


        //------------------------------------------------
        // 示例
        //------------------------------------------------
        public static void Demo()
        {
            try
            {
                Encoding encoding = Encoding.GetEncoding("GB2312");
                SocketClient s = new SocketClient("127.0.0.1", 9000);
                s.OnSend += (d) => Console.WriteLine("Send: {0}", d.GetHex(128));
                s.OnReceive += (d) => Console.WriteLine("Receive: {0}", d.GetHex(128));
                s.Send("hello world", encoding);
                string text = s.ReceiveText(encoding);
                Console.WriteLine(text);
                s.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:{0}", e);
            }
        }
    }
}