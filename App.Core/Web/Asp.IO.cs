using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace App.Core
{
    /// <summary>
    /// ASP.NET 网页相关辅助方法（IO相关）
    /// </summary>
    public static partial class Asp
    {
        /// <summary>将物理路径转化为虚拟路径</summary>
        public static string ToVirtualPath(this string physicalPath)
        {
            return physicalPath.ToRelativePath(Asp.MapPath("/")).Replace("\\", "/");
        }

        /// <summary>将 Window 路径转化为 Web 路径（替换反斜杠)</summary>
        public static string ToWebPath(this string winPath)
        {
            return winPath.Replace("\\", "/");
        }



        //------------------------------------------------------------
        // 网站文件处理
        //------------------------------------------------------------
        /// <summary>安全删除文件（不报异常，且忽略静态资源目录文件）</summary>
        public static void DeleteWebFile(string fileUrl, string ignoreFolder = "/res/")
        {
            try
            {
                fileUrl = fileUrl.ToLower();
                if (!fileUrl.Contains(ignoreFolder))
                    File.Delete(HttpContext.Current.Server.MapPath(fileUrl));
            }
            catch { }
        }

        /// <summary>拷贝网站文件。若文件名2未填写，则用guid替代。</summary>
        public static string CopyWebFile(string url1, string url2 = "")
        {
            string path1 = Asp.Server.MapPath(url1);
            string path2 = Asp.Server.MapPath(url2);
            if (url2.IsEmpty())
            {
                int n = url1.LastIndexOf("/");
                var path = url1.Substring(0, n);
                var fileInfo = new FileInfo(path1);
                var folder = fileInfo.Directory.FullName;
                var name = Guid.NewGuid().ToString("N");
                string extension = fileInfo.Extension;
                path2 = string.Format("{0}\\{1}{2}", folder, name, extension);
                url2 = string.Format("{0}/{1}{2}", path, name, extension);
            }
            try
            {
                File.Copy(path1, path2);
            }
            catch { }
            return url2;
        }


        //------------------------------------------------------------
        // 输出文本
        //------------------------------------------------------------
        /// <summary>输出一段文本</summary>
        public static void Write(string format, params object[] args)
        {
            var response = HttpContext.Current.Response;
            response.Write(Utils.GetText(format, args));
        }

        /// <summary>输出文本</summary>
        public static void WriteText(string text, string mimeType = @"text/html", string fileName = "", Encoding encoding = null, bool addMobileMeta = false)
        {
            var response = HttpContext.Current.Response;
            response.ContentEncoding = encoding ?? HttpContext.Current.Request.ContentEncoding;
            response.ContentType = mimeType;
            if (!string.IsNullOrEmpty(fileName))
                response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            if (addMobileMeta)
                response.Write("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"/>");
            response.Write(text);
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



        //------------------------------------------------------------
        // 输出二进制文件
        //------------------------------------------------------------
        /// <summary>输出附件</summary>
        public static void WriteAttach(string filePath, string name)
        {
            // 附件名称
            var attachName = filePath.GetFileName();
            if (name.IsNotEmpty() && name.GetFileExtension().IsEmpty())
            {
                var ext = filePath.GetFileExtension();
                attachName = $"{name}{ext}";
            }

            // 下载文件
            WriteFile(filePath, attachName);
        }

        /// <summary>输出文件（小于2G）</summary>
        /// <param name="filePath">文件物理路径</param>
        /// <param name="attachName">附件名。若不为空，则启动附件下载方式。</param>
        /// <param name="mimeType">文件Mime类型。若为空，则尝试根据文件名扩展名解析。</param>
        public static void WriteFile(string filePath, string attachName = "", string mimeType = "")
        {
            if (mimeType.IsEmpty())
                mimeType = filePath.GetMimeType();
            WriteBinary(File.ReadAllBytes(filePath), attachName, mimeType);
            /*
            // 以下代码本机ok。部署到服务器后，输出异常，无法显示图片
            var response = HttpContext.Current.Response;
            if (mimeType.IsNotEmpty())
                response.ContentType = mimeType;
            if (attachName.IsNotEmpty())
                response.AppendHeader("Content-Disposition", "attachment;filename=" + attachName);
            response.Clear();
            response.Buffer = true;
            response.WriteFile(filePath);   // 在服务器端（IIS）上缓存。当下载文件比较大时，服务器压力会很大，iis支持2G大小的文件下载，
            response.Flush();
            response.Close();
            */
        }

        

        /// <summary>输出图像文件</summary>
        /// <param name="attachName">附件名。若不为空，则启动附件下载方式。</param>
        public static void WriteImage(Image image, string attachName = "", string mimeType = @"image/png")
        {
            WriteBinary(image.ToBytes(), attachName, mimeType);
        }


        /// <summary>输出二进制文件</summary>
        /// <param name="mimeType">文件Mime类型。若为空，则尝试根据文件名扩展名解析。</param>
        /// <param name="attachName">附件名。若不为空，则启动附件下载方式。</param>
        public static void WriteBinary(byte[] bytes, string attachName, string mimeType)
        {
            var response = HttpContext.Current.Response;
            if (mimeType.IsNotEmpty())
                response.ContentType = mimeType;
            if (attachName.IsNotEmpty())
                response.AddHeader("Content-Disposition", "attachment; filename=" + attachName);
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.BinaryWrite(bytes);
            response.End();  // 结束请求，跳到ApplicationEndRequest事件
        }

        /// <summary>输出超大文件（未测试）</summary>
        /// <param name="filePath">文件物理路径</param>
        /// <param name="attachName">附件名。若不为空，则启动附件下载方式。</param>
        /// <param name="mimeType">文件Mime类型。若为空，则尝试根据文件名扩展名解析。</param>
        public static void WriteBigFile(string filePath, string attachName = "", string mimeType = "")
        {
            if (mimeType.IsEmpty())
                mimeType = filePath.GetMimeType();
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                WriteStream(fs, attachName, mimeType);
            }
        }

        /// <summary>输出流（未测试）</summary>
        /// <param name="mimeType">文件Mime类型。若为空，则尝试根据文件名扩展名解析。</param>
        /// <param name="attachName">附件名。若不为空，则启动附件下载方式。</param>
        public static void WriteStream(Stream stream, string attachName, string mimeType)
        {
            long contentLength = stream.Length;

            // 头部
            var response = HttpContext.Current.Response;
            response.ClearContent();
            if (mimeType.IsNotEmpty())
                response.ContentType = mimeType;
            if (attachName.IsNotEmpty())
                response.AddHeader("Content-Disposition", "attachment; filename=" + attachName);
            response.AddHeader("Content-Length", contentLength.ToString());

            // 循环输出流
            int bufferSize = 1024;
            byte[] buffer = new byte[bufferSize];
            long send = 0;
            while (send < contentLength)
            {
                int n = stream.Read(buffer, 0, bufferSize);
                response.OutputStream.Write(buffer, 0, n);
                send += n;
            }
            response.OutputStream.Flush(); // 释放内存
            response.OutputStream.Close();
        }


        //------------------------------------------------------------
        // 输出错误
        //------------------------------------------------------------
        /// <summary>输出 HTTP 错误</summary>
        public static void WriteError(int errorCode, string info)
        {
            HttpContext context = HttpContext.Current;
            context.Response.StatusCode = errorCode;
            context.Response.StatusDescription = info.SubText(0, 512);
            context.Response.End();
        }


        /// <summary>输出错误调试页面</summary>
        public static void WriteErrorHtml(Exception ex)
        {
            var txt = BuildRequestHtml(ex);
            HttpContext.Current.Server.ClearError();
            HttpContext.Current.Response.Write(txt);
        }



        //------------------------------------------------------------
        // 收集客户端和服务器的信息
        //------------------------------------------------------------
        /// <summary>获取web请求信息，并组织成html</summary>
        public static string BuildRequestHtml(Exception ex=null)
        {
            if (!Asp.IsWeb)
                return "";

            var sb = new StringBuilder();
            try
            {
                sb.Append(BuildExceptionInfo(ex));
                sb.Append(BuildRequestInfo());
                sb.Append(BuildRequestParamsInfo());
                sb.Append(BuildClientInfo());
                //sb.Append(BuildServerInfo());
            }
            catch { }
            return sb.ToString();
        }

        /// <summary>打印异常信息</summary>
        public static string BuildExceptionInfo(Exception ex)
        {
            if (ex == null)
                return "";

            var sb = new StringBuilder();
            sb.AppendFormat("<h1>{0}</h1>", ex.GetType().FullName);
            sb.AppendFormat("<BR/>时间：{0}&nbsp;", DateTime.Now);
            sb.AppendFormat("<BR/>URL：{0}&nbsp;", Request?.Url);
            sb.AppendFormat("<BR/>来源：{0}&nbsp;", Request?.UrlReferrer);
            sb.AppendFormat("<BR/>错误：{0}", ex.Message);
            sb.AppendFormat("<BR/>类名：{0}", ex.TargetSite?.DeclaringType?.FullName);
            sb.AppendFormat("<BR/>方法：{0}", ex.TargetSite?.Name);
            sb.AppendFormat("<BR/>堆栈：<pre>{0}</pre>", ex.StackTrace);
            /*
            var st = new System.Diagnostics.StackTrace(ex, true);
            foreach (var frame in st.GetFrames())
            {
                sb.AppendFormat("<BR/>{0}:{1}({2},{3})", 
                    frame.GetFileName(), 
                    frame.GetMethod().Name, 
                    frame.GetFileLineNumber(), 
                    frame.GetFileColumnNumber()
                    );
            }
            */

            // 内部异常信息
            if (ex.InnerException != null)
                sb.Append(BuildExceptionInfo(ex.InnerException));

            return sb.ToString();
        }

        /// <summary>打印请求基础信息</summary>
        public static string BuildRequestInfo(string title="请求信息")
        {
            var sb = new StringBuilder();
            if (title.IsNotEmpty())
                sb.AppendFormat("<h1>{0}</h1>", title);
            sb.AppendFormat("<table class='table table-sm table-hover'>");
            sb.AppendFormat("<thead><tr><td width=300>名称</td><td>内容</td></tr></thead>");
            var heads = Request.Headers;
            foreach (string key in Request.QueryString.Keys)
                sb.AppendFormat("<tr><td>QueryString[{0}]</td><td>{1}&nbsp;</td></tr>", key, Request.QueryString[key]);
            foreach (var pair in CookieHelper.GetCookies())
                sb.AppendFormat("<tr><td>Cookie[{0}]</td><td>{1}&nbsp;</td></tr>", pair.Key, pair.Value);
            foreach (string key in heads.Keys)
                sb.AppendFormat("<tr><td>Header[{0}]</td><td>{1}&nbsp;</td></tr>", key, heads[key]);
            sb.AppendFormat("</table>");
            return sb.ToString();
        }

        /// <summary>打印请求参数</summary>
        public static string BuildRequestParamsInfo(string title= "请求详细参数")
        {
            var sb = new StringBuilder();
            if (title.IsNotEmpty())
                sb.AppendFormat("<h1>{0}</h1>", title);
            sb.AppendFormat("<table class='table table-sm table-hover'>");
            sb.AppendFormat("<thead><tr><td width=300>名称</td><td>内容</td></tr></thead>");
            var ps = Request.Params;
            foreach (string key in ps.Keys)
            {
                var value = ps[key];
                if (value.IsNotEmpty())
                    sb.AppendFormat("<tr><td>Param[{0}]</td><td>{1}&nbsp;</td></tr>", key, ps[key]);
            }
            sb.AppendFormat("</table>");
            return sb.ToString();
        }

        /// <summary>打印客户端信息</summary>
        public static string BuildClientInfo(string title="客户端信息")
        {
            var sb = new StringBuilder();
            var request = Request;
            HttpBrowserCapabilities bc = request.Browser;
            if (title.IsNotEmpty())
                sb.AppendFormat("<h1>{0}</h1>", title);
            sb.AppendFormat("<table class='table table-sm table-hover'>");
            sb.AppendFormat("<thead><tr><td width=300>名称</td><td>内容</td></tr></thead>");
            sb.AppendFormat("<tr><td>客户器IP</td><td>{0}&nbsp;</td></tr>", request.UserHostAddress);
            sb.AppendFormat("<tr><td>客户机OS</td><td>{0}&nbsp;</td></tr>", bc.Platform);
            sb.AppendFormat("<tr><td>浏览器类型</td><td>{0}&nbsp;</td></tr>", bc.Type);
            sb.AppendFormat("<tr><td>支持Cookie</td><td>{0}&nbsp;</td></tr>", bc.Cookies);
            sb.AppendFormat("<tr><td>支持Frames</td><td>{0}&nbsp;</td></tr>", bc.Frames);
            sb.AppendFormat("<tr><td>支持Javascript</td><td>{0}&nbsp;</td></tr>", bc.EcmaScriptVersion);
            sb.AppendFormat("<tr><td>支持VBScript</td><td>{0}&nbsp;</td></tr>", bc.VBScript);
            sb.AppendFormat("</table>");
            return sb.ToString();
        }

        /// <summary>打印服务器端信息</summary>
        public static string BuildServerInfo(string title= "服务器参数")
        {
            var sb = new StringBuilder();
            var request = Request;
            if (title.IsNotEmpty())
                sb.AppendFormat("<h1>{0}</h1>", title);
            //sb.AppendFormat("<table border=1 style='border-collapse: collapse' width='100%' cellpadding='2' cellspacing='0'>");
            sb.AppendFormat("<table class='table table-sm table-hover'>");
            sb.AppendFormat("<thead><tr><td width=300>名称</td><td>内容</td></tr></thead>");
            sb.AppendFormat("<tr><td>服务器IP</td><td>{0}&nbsp;</td></tr>", request.ServerVariables["LOCAL_ADDR"]);
            sb.AppendFormat("<tr><td>服务器端口</td><td>{0}&nbsp;</td></tr>", request.ServerVariables["SERVER_PORT"]);
            sb.AppendFormat("<tr><td>IIS版本</td><td>{0}&nbsp;</td></tr>", request.ServerVariables["SERVER_SOFTWARE"]);
            sb.AppendFormat("<tr><td>服务器操作系统</td><td>{0}&nbsp;</td></tr>", Environment.OSVersion);
            sb.AppendFormat("<tr><td>文件</td><td>{0}&nbsp;</td></tr>", request.ServerVariables["SCRIPT_NAME"]);
            foreach (object obj in request.ServerVariables)
            {
                var name = obj.ToString();
                if (request.ServerVariables[name].Length > 0)
                    sb.AppendFormat("<tr><td>ServerVariables[{0}]</td><td>{1}&nbsp;</td></tr>", name, request.ServerVariables[name]);
            }
            sb.AppendFormat("</table>");
            return sb.ToString();
        }
    }
}