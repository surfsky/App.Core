using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.Caching;

namespace App.Utils
{
    /// <summary>
    /// ASP.NET 网页相关辅助方法（数据存储）
    /// </summary>
    public static partial class Asp
    {
        //-------------------------------------
        // Session 相关
        //-------------------------------------
        /// <summary>设置 Session 对象（含过期时间）</summary>
        public static void SetSession(string name, object value, int? expireMinutes=null)
        {
            HttpContext.Current.Session[name] = value;
            if (expireMinutes != null)  
                HttpContext.Current.Session.Timeout = expireMinutes.Value;
        }

        /// <summary>获取 Session 对象</summary>
        public static T GetSession<T>(string name) where T : class
        {
            if (HasSession(name))
                return HttpContext.Current.Session[name] as T;
            return null;
        }

        /// <summary>获取 Session 对象</summary>
        public static object GetSession(string name)
        {
            return HttpContext.Current.Session[name];
        }

        /// <summary>是否有 Session 值</summary>
        public static bool HasSession(string name)
        {
            return HttpContext.Current.Session[name] != null;
        }

        //------------------------------------------------------------
        // 环境数据获取方法：Cache & Session & HttpContext & Application
        // 以下提供的泛型方法只针对类对象
        //------------------------------------------------------------
        //
        // Session
        //
        /// <summary>获取Session数据（会话期有效）</summary>
        public static object GetSessionData(string key, Func<object> creator = null)
        {
            if (HttpContext.Current.Session == null)
                return null;

            if (creator != null && HttpContext.Current.Session[key] == null)
                HttpContext.Current.Session[key] = creator();
            return HttpContext.Current.Session[key];
        }

        /// <summary>获取Session数据（会话期有效）</summary>
        public static T GetSessionData<T>(string key, Func<object> creator = null) where T : class
        {
            if (HttpContext.Current.Session == null)
                return null;

            if (creator != null && HttpContext.Current.Session[key] == null)
                HttpContext.Current.Session[key] = creator();
            return HttpContext.Current.Session[key] as T;
        }

        //
        //  Context
        //
        /// <summary>获取上下文数据（在每次请求中有效）</summary>
        public static object GetContextData(string key, Func<object> creator = null)
        {
            if (creator != null && !HttpContext.Current.Items.Contains(key))
                HttpContext.Current.Items[key] = creator();
            return HttpContext.Current.Items[key];
        }

        /// <summary>获取上下文数据（在每次请求中有效）</summary>
        public static T GetContextData<T>(string key, Func<object> creator = null) where T : class
        {
            if (creator != null && !HttpContext.Current.Items.Contains(key))
                HttpContext.Current.Items[key] = creator();
            return HttpContext.Current.Items[key] as T;
        }

        //
        // Application
        //
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


        

        /// <summary>设置缓存策略（使用context.Response.Cache来缓存输出）</summary>
        /// <remarks>
        /// ashx 的页面缓存不允许写语句：<%@ OutputCache Duration="60" VaryByParam="*" %>  
        /// 故可直接调用本方法实现缓存。
        /// 参考：https://stackoverflow.com/questions/1109768/how-to-use-output-caching-on-ashx-handler
        /// </remarks>
        public static void SetCachePolicy(this HttpResponse response, int cacheSeconds, string varyByParam = "*", HttpCacheability cacheLocation = HttpCacheability.ServerAndPrivate)
        {
            HttpCachePolicy cachePolicy = response.Cache;
            if (cacheSeconds > 0)
            {
                cachePolicy.SetCacheability(cacheLocation);
                cachePolicy.SetExpires(DateTime.Now.AddSeconds((double)cacheSeconds));
                cachePolicy.SetSlidingExpiration(false);
                cachePolicy.SetValidUntilExpires(true);
                if (varyByParam.IsNotEmpty())
                    cachePolicy.VaryByParams[varyByParam] = true;
                else
                    cachePolicy.VaryByParams.IgnoreParams = true;
            }
            else
            {
                cachePolicy.SetCacheability(HttpCacheability.NoCache);
                cachePolicy.SetMaxAge(TimeSpan.Zero);
            }
        }
    }
}