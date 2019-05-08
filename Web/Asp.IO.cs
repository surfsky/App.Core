using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        // 网站文件处理
        //------------------------------------------------------------
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

        /// <summary>输出文件</summary>
        public static void WriteFile(string filePath, string mimeType = "", string fileName = "")
        {
            Asp.WriteBinary(File.ReadAllBytes(filePath), mimeType, fileName);
        }

        /// <summary>输出图像文件</summary>
        public static void WriteImage(Image image, string mimeType = @"image/png", string fileName = "")
        {
            WriteBinary(image.ToBytes(), mimeType, fileName);
        }

        /// <summary>输出二进制文件</summary>
        public static void WriteBinary(byte[] bytes, string mimeType = "", string fileName = "")
        {
            var response = HttpContext.Current.Response;
            response.ClearContent();
            response.ContentType = mimeType;
            if (!string.IsNullOrEmpty(fileName))
                response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            response.BinaryWrite(bytes);
        }

        /// <summary>输出 HTTP 错误</summary>
        public static void WriteError(int errorCode, string info)
        {
            HttpContext context = HttpContext.Current;
            context.Response.StatusCode = errorCode;
            context.Response.StatusDescription = info;
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
        /// <summary>获取错误信息并组织为 Html</summary>
        public static string BuildRequestHtml(Exception ex=null)
        {
            var sb = new StringBuilder();
            sb.Append(BuildExceptionInfo(ex));
            sb.Append(BuildRequestCoreInfo());
            sb.Append(BuildRequestParamsInfo());
            sb.Append(BuildServerInfo());
            sb.Append(BuildClientInfo());
            return sb.ToString();
        }

        /// <summary>打印异常信息</summary>
        public static string BuildExceptionInfo(Exception ex)
        {
            if (ex == null)
                return "";
            if (ex.InnerException != null)
                ex = ex.InnerException;

            var sb = new StringBuilder();
            sb.AppendFormat("<h1>错误信息</h1>");
            sb.AppendFormat("<BR/>时间：{0}&nbsp;", DateTime.Now);
            sb.AppendFormat("<BR/>URL：{0}&nbsp;", Request.Url);
            sb.AppendFormat("<BR/>来源：{0}&nbsp;", Request.UrlReferrer);
            sb.AppendFormat("<BR/>错误：{0}", ex.Message);
            sb.AppendFormat("<BR/>类名：{0}", ex.TargetSite.DeclaringType.FullName);
            sb.AppendFormat("<BR/>方法：{0}", ex.TargetSite.Name);
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
            /*
            if (ex.InnerException != null)
            {
                sb.AppendFormat("<h1>Inner Exception</h1>");
                sb.Append(BuildExceptionInfo(ex.InnerException));
            }
            */
            return sb.ToString();
        }

        /// <summary>打印请求基础信息</summary>
        public static string BuildRequestCoreInfo()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("<h1>请求信息</h1>");
            sb.AppendFormat("<table border=1 style='border-collapse: collapse' width='100%' cellpadding='2' cellspacing='0'>");
            sb.AppendFormat("<tr><td width=300>名称</td><td>内容</td></tr>");
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
        public static string BuildRequestParamsInfo()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("<h1>请求详细参数</h1>");
            sb.AppendFormat("<table border=1 style='border-collapse: collapse' width='100%' cellpadding='2' cellspacing='0'>");
            sb.AppendFormat("<tr><td width=300>名称</td><td>内容</td></tr>");
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
        public static string BuildClientInfo()
        {
            var sb = new StringBuilder();
            var request = Request;
            HttpBrowserCapabilities bc = request.Browser;
            sb.AppendFormat("<h1>客户端信息</h1>");
            sb.AppendFormat("<table border=1 style='border-collapse: collapse' width='100%' cellpadding='2' cellspacing='0'>");
            sb.AppendFormat("<tr><td width=300>名称</td><td>内容</td></tr>");
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
        public static string BuildServerInfo()
        {
            var sb = new StringBuilder();
            var request = Request;
            sb.AppendFormat("<h1>服务器信息</h1>");
            sb.AppendFormat("<table border=1 style='border-collapse: collapse' width='100%' cellpadding='2' cellspacing='0'>");
            sb.AppendFormat("<tr><td width=300>名称</td><td>内容</td></tr>");
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