﻿using System;
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
    /// ASP.NET 网页相关辅助方法（数据存储）
    /// </summary>
    public static partial class Asp
    {
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

        /// <summary>获取Session数据（会话期有效）</summary>
        public static T GetSessionData<T>(string key, Func<object> creator = null) where T : class
        {
            if (HttpContext.Current.Session == null)
                return null;

            if (creator != null && HttpContext.Current.Session[key] == null)
                HttpContext.Current.Session[key] = creator();
            return HttpContext.Current.Session[key] as T;
        }

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



        //-------------------------------------
        // Session 相关
        //-------------------------------------
        /// <summary>设置 Session 对象（含过期时间）</summary>
        public static void SetSession(string name, object value, int expireMinutes)
        {
            HttpContext.Current.Session[name] = value;
            HttpContext.Current.Session.Timeout = expireMinutes;
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
        // 缓存相关
        //------------------------------------------------------------
        /// <summary>获取缓存对象（缓存有有效期，一旦失效，自动获取）</summary>
        public static T GetCacheData<T>(string key, Func<T> creator) where T : class
        {
            return GetCacheData<T>(key, System.Web.Caching.Cache.NoAbsoluteExpiration, creator);
        }
        public static T GetCacheData<T>(string key, DateTime expiredTime, Func<T> creator) where T : class
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


    }
}