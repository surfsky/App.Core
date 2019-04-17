using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace App.Core
{
    /// <summary>
    /// 类型转换-XML
    /// </summary>
    public static partial class Convertor
    {
        //------------------------------------------
        // Xml
        //------------------------------------------
        /// <summary>解析XML字符串为对象</summary>
        public static T ParseXml<T>(this string xml) where T : class
        {
            var s = new Xmlizer();
            return s.Parse<T>(xml);
        }

        /// <summary>将对象转化为 XML 字符串</summary>
        public static string ToXml(this object o, string rootName)
        {
            var s = new Xmlizer();
            return s.ToXml(o, rootName);
        }


        //------------------------------------------
        // SimplyXml
        //------------------------------------------
        /// <summary>解析 XML 字符串为简单对象（请自行捕捉解析异常）</summary>
        public static T ParseSimplyXml<T>(this string xml) where T : class, new()
        {
            T o = new T();
            var type = typeof(T);
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var root = doc.DocumentElement;
            foreach (PropertyInfo p in type.GetProperties())
            {
                if (!p.CanWrite)
                    continue;

                var node = root.SelectSingleNode(p.Name);
                if (node == null)
                    continue;

                var text = node.InnerText;
                var value = text.Parse(p.PropertyType);
                try
                {
                    p.SetValue(o, value, null);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                }
            }
            return o;
        }

        /// <summary>构建XML</summary>
        public static string ToSimplyXml(this object o, string root = "xml")
        {
            var sb = new StringBuilder();
            sb.AppendFormat("<{0}>\r\n", root);
            var items = o.GetType().GetProperties();
            foreach (var item in items)
                sb.AppendFormat("<{0}>{1}</{0}>\r\n", item.Name, item.GetValue(o).ToXmlSafeText());
            sb.AppendFormat("</{0}>\r\n", root);
            return sb.ToString();
        }

        /// <summary>构建XML</summary>
        public static string ToSimplyXml(this Dictionary<string, string> dict, string root = "xml")
        {
            var sb = new StringBuilder();
            var items = dict.ToList();
            sb.AppendFormat("<{0}>\r\n", root);
            foreach (var item in items)
                sb.AppendFormat("<{0}>{1}</{0}>\r\n", item.Key, item.Value.ToXmlSafeText());
            sb.AppendFormat("</{0}>\r\n", root);
            return sb.ToString();
        }

        /// <summary>获取Xml安全文本（若有必要，用CDATA封装）</summary>
        public static string ToXmlSafeText(this object obj)
        {
            if (obj == null)
                return "";

            // "<" 字符和"&"字符对于XML来说是严格禁止使用的，此处用CDATA解决
            var txt = obj.ToString();
            if (txt.IndexOfAny(new char[] { '<', '&' }) != -1)
                return string.Format("<![CDATA[ {0} ]]>", txt);
            return txt;
        }




    }
}