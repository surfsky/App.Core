using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace App.Core
{
    public class CookieHelper
    {
        /// <summary>
        /// 读cookie值
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>cookie值</returns>
        public static string GetCookie(string name)
        {
            if (HttpContext.Current.Request.Cookies != null && HttpContext.Current.Request.Cookies[name] != null)
            {
                return HttpContext.Current.Request.Cookies[name].Value.ToString();                
            }
            return null;
        }

        /// <summary>
        /// 设置cookie
        /// </summary>
        public static void SetCookie(string name,string value)
        {
            SetCookie(name, value, 20);
        }

        /// <summary>
        /// 设置cookie
        /// </summary>
        public static void SetCookie(string name, string value,int expires)
        {
            HttpCookie cookie = new HttpCookie(name, value);
            cookie.Expires = DateTime.Now.AddMinutes(expires);
            HttpContext.Current.Response.SetCookie(cookie);
        }

        public static void InsertCookie(string name, string value)
        {
            InsertCookie(name, value, 20);
        }

        public static void InsertCookie(string name, string value,int expires)
        {
            HttpCookie cookie = new HttpCookie(name);
            cookie.Expires = DateTime.Now.AddMinutes(expires);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static void RemoveCookie(string name)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[name];
            if (cookie != null)
                HttpContext.Current.Response.Cookies.Remove(name);
        }

        public static void ClearCookie()
        {
            HttpContext.Current.Request.Cookies.Clear();
        }
    }
}
