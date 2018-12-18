using System;
using System.Collections.Generic;
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
    /// ASP.NET 网页相关辅助方法
    /// </summary>
    public class Asp
    {
        /// <summary>是否是网站运行环境</summary>
        public static bool IsWeb()
        {
            return HttpContext.Current != null;
        }

        /// <summary>是否是本网站文件</summary>
        public static bool IsLocalFile(string url)
        {
            url = Asp.ResolveUrl(url);
            Uri uri = new Uri(url);
            return uri.Host.ToLower() == Asp.Host.ToLower();
        }

        // 在页面头部注册CSS
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

        // 在页面头部注册脚本
        public static void RegistScript(string url)
        {
            HtmlGenericControl script = new HtmlGenericControl("script");
            script.Attributes.Add("type", "text/javascript");
            script.Attributes.Add("src", url);
            (HttpContext.Current.Handler as Page).Header.Controls.Add(script);
        }


        /// <summary>
        /// 创建POST表单并跳转页面
        /// </summary>
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

        //-------------------------------------
        // HttpContext
        //-------------------------------------
        public static HttpServerUtility Server { get { return HttpContext.Current.Server; } }
        public static HttpRequest Request { get { return HttpContext.Current.Request; } }
        public static HttpResponse Response { get { return HttpContext.Current.Response; } }
        public static HttpSessionState Session { get { return HttpContext.Current.Session; } }
        public static HttpApplicationState Application { get { return HttpContext.Current.Application; } }
        public static Page Page { get { return HttpContext.Current.Handler as Page; } }
        public static IPrincipal User { get { return HttpContext.Current.User; } }


        /// <summary>获取主机根路径</summary>
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

        /// <summary>获取 Url 路径（如 http://.../）</summary>
        public static string GetUrlPath(string url)
        {
            int n = url.LastIndexOf('/');
            if (n != -1)
                return url.Substring(0, n);
            return "";
        }

        /// <summary>获取 Url 路径和页面（如 http://..../Page1.aspx）</summary>
        public static string GetUrlPathAndPage(string url)
        {
            int n = url.LastIndexOf('?');
            if (n != -1)
                return url.Substring(0, n);
            return "";
        }

        /// <summary>获取 Url 页面（如 Page1.aspx）</summary>
        public static string GetUrlPage(string url)
        {
            int n = url.LastIndexOf('/');
            if (n != -1)
            {
                url = url.Substring(n);
                n = url.IndexOf('?');
                if (n != -1)
                    return url.Substring(0, n);
                return url;
            }
            return "";
        }

        /// <summary>获取 Url 查询页面</summary>
        public static string GetUrlQueryString(Uri uri)
        {
            return uri.Query;
        }


        /// <summary>获取客户端真实IP</summary>
        public static string GetClientIP()
        {
            HttpRequest request = HttpContext.Current.Request;
            return (request.ServerVariables["HTTP_VIA"] != null)
                ? request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString()   // 使用代理，尝试去找原始地址
                : request.ServerVariables["REMOTE_ADDR"].ToString()            // 
                ;
            //return request.UserHostAddress;
        }

        // Session 相关
        public static void SetSession(string name, object value)
        {
            SetSession(name, value, 20);
        }

        public static void SetSession(string name, object value, int expireMinutes)
        {
            HttpContext.Current.Session[name] = value;
            HttpContext.Current.Session.Timeout = expireMinutes;
        }

        public static object GetSession(string name)
        {
            return HttpContext.Current.Session[name];
        }

        public static bool HasSession(string name)
        {
            return HttpContext.Current.Session[name] != null;
        }

        //-------------------------------------------
        // Url 转换辅助函数
        //-------------------------------------------
        /// <summary>
        /// 将 URL 转化为从根目录开始的路径。如:
        /// （1）../default.aspx 转化为 /application1/default.aspx
        /// （2）~/default.aspx 转化为 /application1/default.aspx
        /// </summary>
        public static string ResolveUrl(string relativeUrl)
        {
            return new Control().ResolveUrl(relativeUrl);
        }

        /// <summary>
        /// 将 URL 转化为相对于浏览器当前路径的相对路径。
        /// 如浏览器当前为 /pages/test.aspx，则
        /// （1）/pages/default.aspx 转化为 default.aspx
        /// （2）~/default.aspx      转化为 ../default.aspx
        /// </summary>
        public static string ResolveClientUrl(string relativeUrl)
        {
            return new Control().ResolveClientUrl(relativeUrl);
        }



        //-------------------------------------
        // QueryString
        //-------------------------------------
        /// <summary>获取查询字符串</summary>
        public static string GetQueryString(string queryKey)
        {
            return HttpContext.Current.Request.QueryString[queryKey];
        }

        /// <summary>获取查询字符串中的整型参数值</summary>
        public static int? GetQueryIntValue(string queryKey)
        {
            int result = -1;
            string str = HttpContext.Current.Request.QueryString[queryKey];
            if (!string.IsNullOrEmpty(str) && Int32.TryParse(str, out result))
                return result;
            return null;
        }

        /// <summary>获取查询字符串中的boolean参数值</summary>
        public static bool? GetQueryBoolValue(string queryKey)
        {
            bool result = false;
            string str = HttpContext.Current.Request.QueryString[queryKey];
            if (!string.IsNullOrEmpty(str) && Boolean.TryParse(str, out result))
                return result;
            return null;
        }

        /// <summary>获取查询字符串中的枚举参数值（支持枚举字符串或枚举数字）</summary>
        public static T? GetQueryEnumValue<T>(string queryKey) where T : struct
        {
            string str = HttpContext.Current.Request.QueryString[queryKey];
            return str.ToEnum<T>();
        }



        //------------------------------------------------------------
        // 环境数据获取方法：Cache & Session & HttpContext & Application
        // 若使用泛型方法存储简单值的数据，as 转化会有报错，故还是用非泛型方法，更通用一些。
        // 以下提供的泛型方法只针对类对象
        //------------------------------------------------------------
        /// <summary>获取Session数据（会话期有效）</summary>
        public static object GetSessionData(string key, Func<object> creator = null)
        {
            if (HttpContext.Current.Session == null)
                return null;

            if (creator != null && HttpContext.Current.Session[key] == null)
                HttpContext.Current.Session[key] = creator();
            return HttpContext.Current.Session[key];
        }

        /// <summary>获取上下文数据（在每次请求中有效）</summary>
        public static object GetContextData(string key, Func<object> creator = null)
        {
            if (creator != null && !HttpContext.Current.Items.Contains(key))
                HttpContext.Current.Items[key] = creator();
            return HttpContext.Current.Items[key];
        }

        /// <summary>清除 Application 数据</summary>
        public static void ClearApplicationData(string key)
        {
            var app = HttpContext.Current.Application;
            if (!app.AllKeys.Contains(key))
                app.Remove(key);
        }

        /// <summary>获取 Application 数据（网站开启一直有效）</summary>
        public static object GetApplicationData(string key, Func<object> creator = null)
        {
            if (creator != null && !Application.AllKeys.Contains(key))
            {
                System.Diagnostics.Debug.WriteLine("Create application data : " + key);
                Application[key] = creator();
            }
            return Application[key];
        }

        /// <summary>获取 Application 数据（网站开启一直有效）</summary>
        public static T GetApplicationData<T>(string key, Func<T> creator = null) where T : class
        {
            if (creator != null && !Application.AllKeys.Contains(key))
            {
                System.Diagnostics.Debug.WriteLine("Create application data : " + key);
                Application[key] = creator();
            }
            return Application[key] as T;
        }

        /// <summary>获取 Application 数据（网站开启一直有效）</summary>
        public static T GetApplicationValue<T>(string key, Func<T> creator = null) where T : struct
        {
            if (creator != null && !Application.AllKeys.Contains(key))
            {
                System.Diagnostics.Debug.WriteLine("Create application data : " + key);
                Application[key] = creator();
            }
            return (T)Application[key];
        }



        //------------------------------------------------------------
        // 缓存数据
        //------------------------------------------------------------
        /// <summary>获取缓存对象（缓存有有效期，一旦失效，自动获取）</summary>
        public static T GetCachedData<T>(string key, Func<T> creator) where T : class
        {
            return GetCachedData<T>(key, System.Web.Caching.Cache.NoAbsoluteExpiration, creator);
        }
        public static T GetCachedData<T>(string key, DateTime expiredTime, Func<T> creator) where T : class
        {
            if (HttpContext.Current.Cache[key] == null)
            {
                T o = creator();
                if (o != null)
                {
                    HttpContext.Current.Cache.Insert(key, o, null, expiredTime, System.Web.Caching.Cache.NoSlidingExpiration);
                    System.Diagnostics.Debug.WriteLine("Create cache : " + key);
                }
            }
            return HttpContext.Current.Cache[key] as T;
        }


        /// <summary>获取缓存对象（缓存有有效期，一旦失效，自动获取）</summary>
        public static object GetCacheData(string key, DateTime expiredTime, Func<object> creator = null)
        {
            if (creator != null && HttpContext.Current.Cache[key] == null)
            {
                var o = creator();
                if (o != null)
                {
                    HttpContext.Current.Cache.Insert(key, o, null, expiredTime, System.Web.Caching.Cache.NoSlidingExpiration);  // Cache.Insert若存在会覆盖的
                    System.Diagnostics.Debug.WriteLine("Create cache : " + key);
                }
            }
            return HttpContext.Current.Cache[key];
        }

        /// <summary>清除缓存对象</summary>
        public static void ClearCachedObject(string key)
        {
            if (HttpContext.Current.Cache[key] != null)
            {
                System.Diagnostics.Debug.WriteLine("Clear cache : " + key);
                HttpContext.Current.Cache.Remove(key);
            }
        }

        /// <summary>设置缓存策略（使用context.Response.Cache来缓存输出）</summary>
        public static void SetCachePolicy(HttpContext context, int cacheSeconds, bool varyByParams = true)
        {
            if (cacheSeconds > 0)
            {
                context.Response.Cache.SetCacheability(HttpCacheability.Server);
                context.Response.Cache.SetExpires(DateTime.Now.AddSeconds((double)cacheSeconds));
                context.Response.Cache.SetSlidingExpiration(false);
                context.Response.Cache.SetValidUntilExpires(true);
                if (varyByParams)
                    context.Response.Cache.VaryByParams["*"] = true;
                else
                    context.Response.Cache.VaryByParams.IgnoreParams = true;
            }
            else
            {
                context.Response.Cache.SetNoServerCaching();
                context.Response.Cache.SetMaxAge(TimeSpan.Zero);
            }
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
        // 网站文件处理
        //------------------------------------------------------------
        /// <summary>输出错误调试页面</summary>
        public static void WriteError(Exception ex)
        {
            var txt = GetErrorHtml(ex);
            HttpContext.Current.Server.ClearError();
            HttpContext.Current.Response.Write(txt);
        }

        /// <summary>获取错误信息并组织为 Html</summary>
        public static string GetErrorHtml(Exception ex)
        {
            var request = HttpContext.Current.Request;
            var server = HttpContext.Current.Server;
            var response = HttpContext.Current.Response;
            var sb = new System.Text.StringBuilder();
            var st = new System.Diagnostics.StackTrace(ex, true);

            // 基本信息
            sb.AppendFormat("<h1>错误信息</h1>");
            sb.AppendFormat("<BR/>时间：{0}&nbsp;", DateTime.Now);
            sb.AppendFormat("<BR/>URL：{0}&nbsp;", request.Url);
            sb.AppendFormat("<BR/>来源：{0}&nbsp;", request.UrlReferrer);
            sb.AppendFormat("<BR/>错误：{0}", ex.Message);
            sb.AppendFormat("<BR/>类名：{0}", ex.TargetSite.DeclaringType.FullName);
            sb.AppendFormat("<BR/>方法：{0}", ex.TargetSite.Name);
            sb.AppendFormat("<BR/>堆栈：<pre>{0}</pre>", ex.StackTrace);
            /*
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

            // 服务器信息
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

            // 客户端信息
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

    }
}