﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// URL 辅助操作类，可以自由修改 QueryString 各部分。
    /// </summary>
    /// <example>
    ///     var url = new Url("http://www.company.com/a/b/c.aspx?mode=new&parentid=1");
    ///     url["mode"] = "edit";
    ///     url["id"] = "5";
    ///     url["more"] = "8";
    ///     url.Remove("parentid");
    ///     var txt = url.ToString();
    /// </example>
    public class Url
    {
        //---------------------------------------
        // 公开属性
        //---------------------------------------
        /// <summary>查询字符串字典</summary>
        public FreeDictionary<string, string> Dict { get; set; }

        /// <summary>协议。如 https</summary>
        public string Protocol { get; set; }

        /// <summary>主机。如a.b.com</summary>
        public string Host { get; set; }
        
        /// <summary>端口号</summary>
        public string Port { get; set; }

        /// <summary>相对于根目录的绝对路径。如 /Pages/Default.aspx?p=1</summary>
        public string AbsolutePath { get; set; }

        /// <summary>去除查询字符串外的纯路径。如 /Pages/Default.aspx</summary>
        public string PurePath { get; set; } = "";

        /// <summary>文件名称。如 Default.aspx</summary>
        public string FileName { get; set; }

        /// <summary>文件扩展名（小写）。如 .aspx</summary>
        public string FileExtesion { get; set; }

        //---------------------------------------
        // 查询字符串操作
        //---------------------------------------
        /// <summary>查询字符串</summary>
        public string QueryString
        {
            get{ return this.Dict.ToString(); }
        }

        /// <summary>获取或设置查询字符串成员</summary>
        public string this[string key]
        {
            get { return Dict[key];}
            set { Dict[key] = value; }
        }

        /// <summary>删除查询字符串成员</summary>
        public void Remove(string key)
        {
            Dict.Remove(key);
        }

        /// <summary>是否具有查询字符串值</summary>
        public bool Has(string key)
        {
            return Dict.Keys.Contains(key);
        }

        /// <summary>转化为查询字符串。如http://../page.aspx?a=x&amp;b=x</summary>
        public override string ToString()
        {
            return this.PurePath.IsEmpty() 
                ? this.QueryString
                : string.Format("{0}?{1}", this.PurePath, this.QueryString).TrimEnd('?')
                ;
        }


        //---------------------------------------
        // 构造函数
        //---------------------------------------
        /// <summary>创建URL对象</summary>
        public Url(string url)
        {
            if (url.IsEmpty())
                return;

            try
            {
                // 分离路径和查询字符串部分
                var queryString = "";
                int n = url.IndexOf('?');
                if (n == -1)
                {
                    // 无问号但有等于号，认为该字符串就是querystring
                    // 无问号且无等于号，认为该字符串就是purepath
                    if (url.IndexOf('=') != -1)   queryString = url;
                    else                          this.PurePath = url;
                }
                else
                {
                    this.PurePath = url.Substring(0, n);
                    queryString = url.Substring(n + 1);
                }

                // 解析参数部分
                Dict = queryString.ParseDict();

                // 分析前面的路径部分
                int k = PurePath.LastIndexOf('.');
                if (k != -1)
                    this.FileExtesion = PurePath.Substring(k).ToLower();
                k = PurePath.LastIndexOf("/");
                if (k != -1)
                    this.FileName = PurePath.Substring(k+1);

                // 解析协议、主机、端口、请求路径
                Regex r = new Regex(@"^(?<proto>\w+)://(?<host>[^/:]+)(?<port>:\d+)(?<path>[\w\._/]+)", RegexOptions.Compiled);
                Match m = r.Match(PurePath);
                if (m.Success)
                {
                    this.Protocol = m.Result("${proto}");
                    this.Host = m.Result("${host}");
                    this.Port = m.Result("${port}")?.TrimStart(':');
                    this.AbsolutePath = m.Result("${path}");
                }
            }
            catch { }
        }



    }
}
