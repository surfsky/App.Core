using System;
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
    public class Url
    {
        /// <summary>查询字符串字典</summary>
        private Dictionary<string, string> _dict = new Dictionary<string, string>();
        public Dictionary<string,string> Dict
        {
            get { return _dict; }
        }

        //---------------------------------------
        // 公开属性
        //---------------------------------------
        /// <summary>基路径。如 http://.../Page.aspx</summary>
        public string BaseUrl { get; set; } = "";

        /// <summary>获取查询字符串</summary>
        public string QueryString
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var key in _dict.Keys)
                    sb.AppendFormat("{0}={1}&", key, _dict[key]);
                return sb.ToString().TrimEnd('&');
            }
        }

        /// <summary>获取或设置查询字符串成员</summary>
        public string this[string key]
        {
            get { return _dict[key]; }
            set { _dict[key] = value; }
        }

        /// <summary>删除查询字符串成员</summary>
        public void Remove(string key)
        {
            if (_dict.Keys.Contains(key))
                _dict.Remove(key);
        }


        //---------------------------------------
        // 构造函数
        //---------------------------------------
        public Url(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            try
            {
                // 分离路径和查询字符串部分
                int n = url.IndexOf('?');
                if (n == -1)
                    this.BaseUrl = url;
                else
                    this.BaseUrl = url.Substring(0, n);
                if (n == url.Length - 1)
                    return;

                // 分析参数对
                var queryString = url.Substring(n + 1);
                var regex = new Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", RegexOptions.Compiled);
                var matches = regex.Matches(queryString);
                foreach (Match m in matches)
                {
                    var key = m.Result("$2");
                    var value = m.Result("$3");
                    _dict.Add(key, value);
                }
            }
            catch { }
        }

        //---------------------------------------
        //---------------------------------------
        /// <summary>转化为查询字符串。如http://../page.aspx?a=x&b=x</summary>
        public override string ToString()
        {
            return string.Format("{0}?{1}", this.BaseUrl, this.QueryString).TrimEnd('?');
        }


    }
}
