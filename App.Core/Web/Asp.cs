using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web;
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
        public static HttpServerUtility Server { get { return HttpContext.Current.Server; } }
        public static HttpRequest Request
        {
            get
            {
                try   { return HttpContext.Current.Request; }
                catch { return null; }
            }
        }
        public static HttpResponse Response { get { return HttpContext.Current.Response; } }
        public static HttpSessionState Session { get { return HttpContext.Current.Session; } }
        public static HttpApplicationState Application { get { return HttpContext.Current.Application; } }
        public static Page Page { get { return HttpContext.Current.Handler as Page; } }
        public static IPrincipal User { get { return HttpContext.Current.User; } }
        public static string Url { get { return HttpContext.Current.Request.Url.ToString(); } }


        /// <summary>获取主机根路径</summary>
        public static string Root
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

        /// <summary>是否是网站运行环境</summary>
        public static bool IsWeb
        {
            get {return HttpContext.Current != null;}
        }

        /// <summary>请求是否有效（避免触发“HttpRequest在上下文中不可用”的异常）</summary>
        public static bool IsRequestValid
        {
            get { return Request != null; }
        }


        /// <summary>获取客户端真实IP</summary>
        public static string ClientIP
        {
            get
            {
                try
                {
                    HttpRequest request = HttpContext.Current.Request;
                    return (request.ServerVariables["HTTP_VIA"] != null)
                        ? request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString()   // 使用代理，尝试去找原始地址
                        : request.ServerVariables["REMOTE_ADDR"].ToString()            // 
                        ;
                    //return request.UserHostAddress;
                }
                catch
                {
                    return "";
                }
            }
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
        /// <summary>是否是本网站文件</summary>
        public static bool IsSiteFile(this string url)
        {
            url = Asp.ResolveUrl(url);
            Uri uri = new Uri(url);
            return uri.Host.ToLower() == Asp.Request.Url.Host.ToLower();
        }

        /// <summary>将虚拟路径转化为物理路径。等同于Server.MapPath()</summary>
        public static string MapPath(this string virtualPath)
        {
            return Server.MapPath(virtualPath);
        }


        /// <summary>
        /// 将 URL 转化为完整路径。如:
        /// （1）../default.aspx 转化为 http://..../application1/default.aspx
        /// （2）~/default.aspx 转化为 http://..../application1/default.aspx
        /// </summary>
        public static string ResolveFullUrl(this string relativeUrl)
        {
            if (relativeUrl.IsEmpty())
                return "";
            if (relativeUrl.ToLower().StartsWith("http"))
                return relativeUrl;
            var url = new Control().ResolveUrl(relativeUrl);
            return Asp.Root + url;
        }

        /// <summary>
        /// 将 URL 转化为从根目录开始的路径。如:
        /// （1）../default.aspx 转化为 /application1/default.aspx
        /// （2）~/default.aspx 转化为 /application1/default.aspx
        /// </summary>
        public static string ResolveUrl(this string relativeUrl)
        {
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
            return relativeUrl.IsEmpty() ? "" : new Control().ResolveClientUrl(relativeUrl);
        }



        //-------------------------------------
        // QueryString
        //-------------------------------------
        /// <summary>获取查询字符串</summary>
        public static string GetQueryString(string queryKey)
        {
            return HttpContext.Current.Request.QueryString[queryKey];
        }

        /// <summary>获取查询字符串</summary>
        public static T GetQuery<T>(string queryKey)
        {
            var txt = HttpContext.Current.Request.QueryString[queryKey];
            return txt.Parse<T>();
        }

        /// <summary>获取查询字符串中的整型参数值</summary>
        public static int? GetQueryInt(string queryKey)
        {
            int result = -1;
            string str = HttpContext.Current.Request.QueryString[queryKey];
            if (!string.IsNullOrEmpty(str) && Int32.TryParse(str, out result))
                return result;
            return null;
        }

        /// <summary>获取查询字符串中的整型参数值</summary>
        public static Int64? GetQueryLong(string queryKey)
        {
            Int64 result = -1;
            string str = HttpContext.Current.Request.QueryString[queryKey];
            if (!string.IsNullOrEmpty(str) && Int64.TryParse(str, out result))
                return result;
            return null;
        }

        /// <summary>获取查询字符串中的小数参数值</summary>
        public static double? GetQueryDouble(string queryKey)
        {
            double result = -1;
            string str = HttpContext.Current.Request.QueryString[queryKey];
            if (str.IsNotEmpty() && double.TryParse(str, out result))
                return result;
            return null;
        }

        /// <summary>获取查询字符串中的boolean参数值</summary>
        public static bool? GetQueryBool(string queryKey)
        {
            bool result = false;
            string str = HttpContext.Current.Request.QueryString[queryKey];
            if (str.IsNotEmpty() && Boolean.TryParse(str, out result))
                return result;
            return null;
        }

        /// <summary>获取查询字符串中的日期时间参数值</summary>
        public static DateTime? GetQueryDate(string queryKey)
        {
            DateTime result;
            string str = HttpContext.Current.Request.QueryString[queryKey];
            if (!string.IsNullOrEmpty(str) && DateTime.TryParse(str, out result))
                return result;
            return null;
        }

        /// <summary>获取查询字符串中的枚举参数值（支持枚举字符串或枚举数字）</summary>
        public static T? GetQueryEnum<T>(string queryKey) where T : struct
        {
            string str = HttpContext.Current.Request.QueryString[queryKey];
            return str.ParseEnum<T>();
        }

        /// <summary>获取 URL 对应的处理器类</summary>
        public static Type GetHandler(string url, HttpContext context=null)
        {
            if (context == null)
                context = HttpContext.Current;
            try
            {
                var type = WebHandlerParser.GetCompiledType(url, url, context);
                if (type.FullName.StartsWith("ASP.") && type.BaseType != null)
                    type = type.BaseType;
                return type;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }

    /// <summary>
    /// 页面请求处理器解析器。可获取页面请求对应的处理类。
    /// （抄的 Asp.net 源码）
    /// </summary>
    internal class WebHandlerParser : SimpleWebHandlerParser
    {
        private WebHandlerParser(HttpContext context, string virtualPath, string physicalPath)
            : base(context, virtualPath, physicalPath)
        {
        }
        internal static Type GetCompiledType(string virtualPath, string physicalPath, HttpContext context)
        {
            var parser = new WebHandlerParser(context, virtualPath, physicalPath);
            return parser.GetCompiledTypeFromCache();
        }
        protected override string DefaultDirectiveName
        {
            get { return "webhandler"; }
        }
    }
}