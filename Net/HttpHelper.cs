using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;

namespace App.Core
{
    /// <summary>
    /// 要上传的文件信息
    /// </summary>
    public class PostFile
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public PostFile(byte[] file, string fileName = null, string contentType = null)
        {
            this.File = file;
            this.FileName = fileName;
            this.ContentType = contentType;
        }
    }

    /// <summary>
    /// HTTP 操作相关（GET/POST/...)
    /// </summary>
    public static class HttpHelper
    {
        //---------------------------------------------------------
        // 辅助方法
        //---------------------------------------------------------
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

        /// <summary>获取Http响应文本</summary>
        public static string GetResponseText(HttpWebResponse response)
        {
            string encoding = response.ContentEncoding;
            if (encoding == null || encoding.Length < 1)
                encoding = "UTF-8";
            var reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(encoding));
            return reader.ReadToEnd();
        }

        //---------------------------------------------------------
        // Get 方法
        //---------------------------------------------------------
        /// <summary>Get</summary>
        public static string Get(string url, CookieContainer cookieContainer = null)
        {
            // 请求
            cookieContainer = cookieContainer ?? new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.CookieContainer = cookieContainer;

            // 返回
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            return GetResponseText(response);
        }


        //---------------------------------------------------------
        // Post方法
        //---------------------------------------------------------
        /// <summary>Post 查询字符串</summary>
        public static string Post(string url, string queryString, Encoding encoding = null, string contentType = null, CookieContainer cookieContainer = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            byte[] bytes = queryString.IsEmpty() ? new byte[0] : encoding.GetBytes(queryString);
            return Post(url, bytes, contentType, cookieContainer);
        }

        /// <summary>Post Json 字符串</summary>
        public static string PostJson(string url, string jsonString, Encoding encoding = null, CookieContainer cookieContainer = null)
        {
            return Post(url, jsonString, encoding, "application/json", cookieContainer);
        }

        /// <summary>Post 字典</summary>
        public static string Post(string url, Dictionary<string, string> data, Encoding encoding = null, string contentType = null, CookieContainer cookieContainer = null)
        {
            return Post(url, ToQueryString(data), encoding, contentType, cookieContainer);
        }

        /// <summary>POST 字节数组</summary>
        public static string Post(string url, byte[] bytes, string contentType = null, CookieContainer cookieContainer = null)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return Post(url, stream, contentType, cookieContainer);
        }

        /// <summary>Post 文件</summary>
        public static string PostFile(string url, string filePath, CookieContainer cookieContainer = null)
        {
            var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read);
            return Post(url, fileStream, null, cookieContainer);
        }

        /// <summary>Post 字节流</summary>
        public static string Post(string url, Stream stream = null, string contentType = null, CookieContainer cookieContainer = null)
        {
            // 参数
            cookieContainer = cookieContainer ?? new CookieContainer();
            contentType = contentType ?? "application/x-www-form-urlencoded";

            // 请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = contentType;
            request.ContentLength = stream != null ? stream.Length : 0;
            request.CookieContainer = cookieContainer;
            //request.SendChunked = false;
            //request.KeepAlive = true;
            //request.Proxy = null;
            //request.Timeout = Timeout.Infinite;
            //request.ReadWriteTimeout = Timeout.Infinite;
            //request.AllowWriteStreamBuffering = false;
            //request.ProtocolVersion = HttpVersion.Version11;

            // 上传
            if (stream != null)
            {
                Stream requestStream = request.GetRequestStream();
                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                    requestStream.Write(buffer, 0, bytesRead);
                stream.Close();
            }

            // 返回
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            response.Cookies = cookieContainer.GetCookies(response.ResponseUri);
            return GetResponseText(response);
        }


        ///------------------------------------------------------------
        /// Post MultipartForm Http 协议 (multipart/form-data) 辅助方法。
        /// 普通post是简单的name=value值连接，而multipart/form-data则是添加了分隔符的内容组合。
        /// 一次性可以传多个数据，如：文本、图片、文件。
        /// 和普通POST的区别: http://blog.csdn.net/five3/article/details/7181521
        /// 来自Face++ 代码示例：https://console.faceplusplus.com.cn/documents/6329752
        ///------------------------------------------------------------
        /// <summary>Post MultipartForm</summary>
        public static string PostMultipartForm(string url, Dictionary<string, object> data, Encoding encoding = null)
        {
            string boundary = string.Format("----------{0:N}", Guid.NewGuid()); // 分隔字符串
            string contentType = "multipart/form-data; boundary=" + boundary;   // multipart 请求类型
            byte[] bytes = BuildMultipartFormData(data, boundary, encoding);    // 将参数转化为字节数组
            return Post(url, bytes, contentType, null);
        }

        /// <summary>组装参数字典</summary>
        /// <param name="data">参数字典</param>
        /// <param name="boundary">分隔字符串</param>
        private static byte[] BuildMultipartFormData(Dictionary<string, object> data, string boundary, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            bool flag = false;
            Stream stream = new MemoryStream();
            foreach (var item in data)
            {
                if (flag)
                    stream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                flag = true;
                if (item.Value is PostFile)
                {
                    PostFile fileParameter = (PostFile)item.Value;
                    string s = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n", new object[]
                    {
                        boundary,
                        item.Key,
                        fileParameter.FileName ?? item.Key,
                        fileParameter.ContentType ?? "application/octet-stream"
                    });
                    stream.Write(encoding.GetBytes(s), 0, encoding.GetByteCount(s));
                    stream.Write(fileParameter.File, 0, fileParameter.File.Length);
                }
                else
                {
                    string s2 = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}", boundary, item.Key, item.Value);
                    stream.Write(encoding.GetBytes(s2), 0, encoding.GetByteCount(s2));
                }
            }
            string s3 = "\r\n--" + boundary + "--\r\n";
            stream.Write(encoding.GetBytes(s3), 0, encoding.GetByteCount(s3));
            stream.Position = 0L;
            byte[] array = new byte[stream.Length];
            stream.Read(array, 0, array.Length);
            stream.Close();
            return array;
        }


        ///------------------------------------------------------------
        /// 服务器端处理
        ///------------------------------------------------------------
        /// <summary>获取post上来的数据</summary>
        public static string GetPostString(HttpRequest request, Encoding encoding=null)
        {
            Stream stream = request.InputStream;
            int n = (int)stream.Length;
            byte[] bytes = new byte[n];
            stream.Read(bytes, 0, n);
            stream.Close();

            encoding = encoding ?? Encoding.UTF8;
            return encoding.GetString(bytes);
        }


        /// <summary>设置页面缓存</summary>
        /// <param name="context">网页上下文</param>
        /// <param name="cacheSeconds">缓存秒数</param>
        /// <param name="varyByParam">缓存参数名称</param>
        /// <remarks>
        /// ashx 的页面缓存不允许写语句：<%@ OutputCache Duration="60" VaryByParam="*" %>  
        /// 故可直接调用本方法实现缓存。
        /// 参考：https://stackoverflow.com/questions/1109768/how-to-use-output-caching-on-ashx-handler
        /// </remarks>
        public static void SetCache(HttpContext context, int cacheSeconds = 60, HttpCacheability cacheLocation = HttpCacheability.ServerAndPrivate, string varyByParam = "*")
        {
            TimeSpan ts = new TimeSpan(0, 0, 0, cacheSeconds);
            HttpCachePolicy cachePolicy = context.Response.Cache;
            cachePolicy.SetCacheability(cacheLocation);
            cachePolicy.VaryByParams[varyByParam] = true;
            cachePolicy.SetExpires(DateTime.Now.Add(ts));
            cachePolicy.SetMaxAge(ts);
            cachePolicy.SetValidUntilExpires(true);
        }

        //------------------------------------------------------------
        // 获取网络图片
        //------------------------------------------------------------
        /// <summary>获取网络图片的缩略图</summary>
        public static Image GetThumbnail(string url, int w, int h = -1)
        {
            Image img = HttpHelper.GetServerOrNetworkImage(url);
            return DrawHelper.CreateThumbnail(img, w, h);
        }

        /// <summary>获取服务器或网络图片</summary>
        /// <param name="url">可用~/，也可以用完整的http地址</param>
        public static Image GetServerOrNetworkImage(string url)
        {
            if (url.StartsWith("~/") || url.StartsWith(".") || url.StartsWith("/"))
            {
                if (Asp.IsWeb() && Asp.IsLocalFile(url))
                    return DrawHelper.LoadImage(Asp.Server.MapPath(url));
            }
            else
                return HttpHelper.GetNetworkImage(url);
            return null;
        }

        /// <summary>获取网络图片</summary>
        public static Bitmap GetNetworkImage(string url)
        {
            try
            {
                var req = (HttpWebRequest)(WebRequest.Create(new Uri(url)));
                req.Timeout = 180000;
                req.Method = "GET";
                var res = (HttpWebResponse)(req.GetResponse());
                return new Bitmap(res.GetResponseStream());
            }
            catch
            {
                return null;
            }
        }

        //------------------------------------------------------------
        // 输出
        //------------------------------------------------------------
        /// <summary>输出 Html 错误</summary>
        public static void WriteHtmlError(int errorCode, string info)
        {
            var response = HttpContext.Current.Response;
            response.StatusCode = errorCode;
            response.StatusDescription = info?.Substring(512);
            response.End();
        }

        /// <summary>输出文本</summary>
        public static void WriteHtml(string text, Encoding encoding = null)
        {
            WriteText(text, "text/html", null);
        }

        /// <summary>输出Json</summary>
        public static void WriteJson(string text, Encoding encoding = null)
        {
            WriteText(text, "text/json", null);
        }

        /// <summary>输出xml</summary>
        public static void WriteXml(string text, Encoding encoding = null)
        {
            WriteText(text, "text/xml", null);
        }

        /// <summary>输出文本</summary>
        public static void WriteText(string text, string mimeType = "text/text", Encoding encoding = null)
        {
            var response = HttpContext.Current.Response;
            response.ContentEncoding = encoding ?? HttpContext.Current.Request.ContentEncoding;
            response.ContentType = mimeType;
            response.Write(text);
        }

        /// <summary>输出图片</summary>
        /// <remarks> ashx不支持语句：OutputCache Duration = "60" VaryByParam = "*", 以后再想办法加上缓存</remarks>
        public static void WriteImage(Image img, string fileName="")
        {
            var bytes = ToBytes(img);
            WriteBinary(bytes, "image/png", fileName);
        }


        /// <summary>输出二进制文件</summary>
        public static void WriteBinary(byte[] bytes, string mimeType, string fileName)
        {
            var response = HttpContext.Current.Response;
            response.ClearContent();
            response.ContentType = mimeType;
            if (!string.IsNullOrEmpty(fileName))
                response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            response.BinaryWrite(bytes);
        }

        // 转化为二进制图像字节数组
        static byte[] ToBytes(Image img)
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

    }
}