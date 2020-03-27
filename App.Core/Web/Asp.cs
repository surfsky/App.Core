using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace App.Core
{
    /// <summary>
    /// ASP.NET 网页相关辅助方法
    /// </summary>
    public static partial class Asp
    {
        //-------------------------------------
        // HttpContext
        //-------------------------------------
        public static HttpServerUtility Server          => HttpContext.Current.Server;
        public static HttpResponse Response             => HttpContext.Current.Response;
        public static HttpSessionState Session          => HttpContext.Current.Session;
        public static HttpApplicationState Application  => HttpContext.Current.Application;
        public static Page Page                         => HttpContext.Current.Handler as Page;
        public static IPrincipal User                   => HttpContext.Current.User;
        public static string Url                        => HttpContext.Current.Request.Url.ToString(); 
        public static string RawUrl                     => HttpContext.Current.Request.RawUrl;
        public static string QueryString                => Url.GetQueryString();
        public static HttpRequest Request
        {
            get
            {
                try { return HttpContext.Current.Request; }
                catch { return null; }
            }
        }

        /// <summary>是否是网站运行环境</summary>
        public static bool IsWeb                        => HttpContext.Current != null;

        /// <summary>请求是否有效（避免触发“HttpRequest在上下文中不可用”的异常）</summary>
        public static bool IsRequestOk                  => Request != null;

        /// <summary>主机根路径（如http://www.abc.com/）</summary>
        public static string Host
        {
            get
            {
                Uri url = HttpContext.Current.Request.Url;
                return url.Port == 80
                    ? string.Format("{0}://{1}", url.Scheme, url.Host)
                    : string.Format("{0}://{1}:{2}", url.Scheme, url.Host, url.Port)
                    ;
            }
        }

        /// <summary>主机根物理路径</summary>
        public static string HostFolder => HttpRuntime.AppDomainAppPath;


        /// <summary>获取客户端真实IP</summary>
        public static string ClientIP
        {
            get
            {
                try
                {
                    //return request.UserHostAddress;
                    HttpRequest request = HttpContext.Current.Request;
                    return (request.ServerVariables["HTTP_VIA"] != null)
                        ? request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString()   // 使用代理，尝试去找原始地址
                        : request.ServerVariables["REMOTE_ADDR"].ToString()            // 
                        ;
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// 结束对客户端的输出。
        /// 由于.NET 设计原因，Response.End()在WebForm框架下可以终止代码执行，不再处理End()之后的代码。
        /// 在MVC框架下则只是返回响应流，不会中止代码执行。
        /// </summary>
        public static void End()
        {
            Response.End();
        }

        /// <summary>
        /// 强行断开与客户端的socket连接。
        /// 只有代码发生错误（恶意的攻击），希望终止对于客户端的响应/连接时才可以使用Response.Close()
        /// </summary>
        public static void Close()
        {
            Response.Close();
        }
        

        //-------------------------------------
        // Html
        //-------------------------------------
        /// <summary>在页面头部注册移动端适配的meta语句</summary>
        public static void RegistMobileMeta()
        {
            HtmlHead head = Page.Header;
            HtmlMeta meta = new HtmlMeta();
            meta.Name = "viewport";
            meta.Content = "width=device-width, initial-scale=1.0";
            head.Controls.AddAt(0, meta);
        }

        /// <summary>在页面头部注册CSS</summary>
        public static void RegistCSS(string url, bool appendOrInsert=true)
        {
            url = ResolveUrl(url);
            HtmlLink css = new HtmlLink();
            css.Href = url;
            css.Attributes.Add("rel", "stylesheet");
            css.Attributes.Add("type", "text/css");
            var header = (HttpContext.Current.Handler as Page).Header;
            if (appendOrInsert)
                header.Controls.Add(css);
            else
                header.Controls.AddAt(0, css);
        }

        /// <summary>在页面头部注册脚本</summary>
        public static void RegistScript(string url)
        {
            HtmlGenericControl script = new HtmlGenericControl("script");
            script.Attributes.Add("type", "text/javascript");
            script.Attributes.Add("src", url);
            (HttpContext.Current.Handler as Page).Header.Controls.Add(script);
        }

        /// <summary>创建POST表单并跳转页面</summary>
        public static void CreateFormAndPost(Page page, string url, Dictionary<string, string> data)
        {
            // 构建表单
            string formID = "PostForm";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"<form id=""{0}"" name=""{0}"" action=""{1}"" method=""POST"">", formID, url);
            foreach (var item in data)
                sb.AppendFormat(@"<input type=""hidden"" name=""{0}"" value='{1}'>", item.Key, item.Value);
            sb.Append("</form>");

            // 创建js执行Form
            sb.Append(@"<script type=""text/javascript"">");
            sb.AppendFormat("var postForm = document.{0};", formID);
            sb.Append("postForm.submit();");
            sb.Append("</script>");
            page.Controls.Add(new LiteralControl(sb.ToString()));
        }


        //-------------------------------------------
        // Url & Path
        //-------------------------------------------
        /// <summary>是否是本网站文件（如果以.~/开头或host相同是本站图片）</summary>
        public static bool IsSiteFile(this string url)
        {
            if (url.IsEmpty())
                return false;
            if (url.StartsWith("/") || url.StartsWith("~/") || url.StartsWith("."))
                return true;
            url = Asp.ResolveUrl(url);
            Uri uri = new Uri(url);
            return uri.Host.ToLower() == Asp.Request.Url.Host.ToLower();
        }

        /// <summary>将虚拟路径转化为物理路径。等同于Server.MapPath()</summary>
        public static string MapPath(this string virtualPath)
        {
            if (virtualPath.IsEmpty())
                return virtualPath;
            if (virtualPath.Contains("/"))
                return Request.MapPath(virtualPath);
            return virtualPath;
        }


        /// <summary>
        /// 将 URL 转化为完整路径。如:
        /// （1）../default.aspx 转化为 http://..../application1/default.aspx
        /// （2）~/default.aspx 转化为 http://..../application1/default.aspx
        /// </summary>
        public static string ResolveFullUrl(this string relativeUrl)
        {
            relativeUrl = relativeUrl.TrimStart("~");
            if (relativeUrl.IsEmpty())
                return "";
            if (relativeUrl.ToLower().StartsWith("http"))
                return relativeUrl;
            var url = new Control().ResolveUrl(relativeUrl);
            return Asp.Host + url;
        }

        /// <summary>
        /// 将 URL 转化为从根目录开始的路径。如:
        /// （1）../default.aspx 转化为 /application1/default.aspx
        /// （2）~/default.aspx 转化为 /application1/default.aspx
        /// </summary>
        public static string ResolveUrl(this string relativeUrl)
        {
            relativeUrl = relativeUrl.TrimStart("~");
            return relativeUrl.IsEmpty() ? "" : new Control().ResolveUrl(relativeUrl);
        }

        /// <summary>
        /// 将 URL 转化为相对于浏览器当前路径的相对路径。
        /// 如浏览器当前为 /pages/test.aspx，则
        /// （1）/pages/default.aspx 转化为 default.aspx
        /// （2）~/default.aspx      转化为 ../default.aspx
        /// </summary>
        public static string ResolveClientUrl(this string relativeUrl)
        {
            relativeUrl = relativeUrl.TrimStart("~");
            return relativeUrl.IsEmpty() ? "" : new Control().ResolveClientUrl(relativeUrl);
        }



        //-------------------------------------
        // QueryString
        //-------------------------------------
        /// <summary>获取请求参数</summary>
        public static T? GetParam<T>(string queryKey) where T : struct
        {
            return Request.Params[queryKey].Parse<T?>();
        }

        /// <summary>获取请求参数</summary>
        public static string GetParam(string queryKey)
        {
            return Request.Params[queryKey];
        }

        /// <summary>获取查询字符串</summary>
        public static T? GetQuery<T>(string queryKey) where T : struct
        {
            return GetQueryString(queryKey).Parse<T?>();
        }

        /// <summary>获取查询字符串</summary>
        public static string GetQueryString(string queryKey, bool ignoreCase=true)
        {
            if (ignoreCase)
                return Request.QueryString[queryKey];
            var url = new Url(Request.RawUrl);
            return url[queryKey];
        }

        /// <summary>获取查询字符串中的整型参数值</summary>
        public static int? GetQueryInt(string queryKey)
        {
            return GetQueryString(queryKey).ParseInt();
        }

        /// <summary>获取查询字符串中的整型参数值</summary>
        public static Int64? GetQueryLong(string queryKey)
        {
            return GetQueryString(queryKey).ParseLong();
        }

        /// <summary>获取查询字符串中的boolean参数值</summary>
        public static bool? GetQueryBool(string queryKey)
        {
            return GetQueryString(queryKey).ParseBool();
        }


        /// <summary>获取 URL 对应的处理器类</summary>
        /// 
        public static Type GetHandler(string url)
        {
            if (url.IsEmpty()) 
                return null;
            var u = new Url(url);
            url = u.PurePath.ToLower();  // 只保留绝对路径，且去除查询字符串
            var key = url.MD5();
            return IO.GetDict<Type>(key, () =>
            {
                Type type = null;
                try { type = BuildManager.GetCompiledType(url); }
                catch { }
                if (type != null && type.FullName.StartsWith("ASP.") && type.BaseType != null)
                    type = type.BaseType;
                return type;
            });
        }

        /// <summary>允许跨域（未测试）</summary>
        public static void EnableCros()
        {
            var origin = HttpContext.Current.Request.Headers["Origin"];
            HttpContext.Current.Response.AppendHeader("Access-Control-Allow-Origin", origin);
        }
    }

}