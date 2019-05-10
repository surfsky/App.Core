using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace App.Core
{
    /// <summary>
    /// 序列化辅助方法
    /// </summary>
    public static class SerializeHelper
    {
        //------------------------------------------------
        // JSON
        //------------------------------------------------
        /// <summary>OBJECT -> JSON</summary>
        public static string ToJson(this object obj, JsonSerializerSettings settings = null)
        {
            if (obj == null)
                return "";
            settings = settings ?? GetDefaultJsonSettings();
            return JsonConvert.SerializeObject(obj, settings);
        }

        /// <summary>OBJECT -> JSON</summary>
        public static void SaveJson(string filePath, object obj, JsonSerializerSettings settings=null)
        {
            Type type = obj.GetType();
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                string txt = obj.ToJson(settings);
                writer.Write(txt);
                writer.Close();
            }
        }

        /// <summary>JSON -> OBJECT</summary>
        public static object LoadJson(string filePath, Type type, JsonSerializerSettings settings=null)
        {
            if (!File.Exists(filePath))
                return null;
            using (StreamReader reader = new StreamReader(filePath))
            {
                string txt = reader.ReadToEnd();
                reader.Close();
                settings = settings ?? GetDefaultJsonSettings();
                return JsonConvert.DeserializeObject(txt, type, settings);
            }
        }

        /// <summary>JSON -> OBJECT</summary>
        public static object ParseJson(this string txt, Type type, JsonSerializerSettings settings = null)
        {
            settings = settings ?? GetDefaultJsonSettings();
            return JsonConvert.DeserializeObject(txt, type, settings);
        }

        /// <summary>JSON -> OBJECT</summary>
        public static T ParseJson<T>(this string txt, JsonSerializerSettings settings = null)
        {
            settings = settings ?? GetDefaultJsonSettings();
            return JsonConvert.DeserializeObject<T>(txt, settings);
        }

        /// <summary>Json 字符串转换为 JObject 对象。可用 JObject["Name1"]["Name2"].ToString() 来获取节点值</summary>
        public static JObject ParseJObject(this string json)
        {
            return JObject.Parse(json);
        }

        //
        public static JsonSerializerSettings GetDefaultJsonSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            //settings.ContractResolver = new CamelCasePropertyNamesContractResolver(); // 属性为小写开头驼峰式
            settings.NullValueHandling = NullValueHandling.Ignore;                      // 忽略null值
            settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;       // 日期格式
            settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";                          // 
            settings.Formatting = Formatting.Indented;                                  // 递进
            settings.MaxDepth = 10;                                                     // 设置序列化的最大层数  
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;              // 指定如何处理循环引用
            settings.Converters.Add(new StringEnumConverter());                         // 枚举输出为字符串
            return settings;
        }

        //------------------------------------------------
        // XML
        //------------------------------------------------
        /// <summary>OBJECT -> XML</summary>
        public static string ToXml(object obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer xs = new XmlSerializer(obj.GetType());
                xs.Serialize(stream, obj);
                return stream.ToString();
            }
        }

        /// <summary>OBJECT -> XML</summary>
        public static void SaveXml(string filePath, object obj)
        {
            Type type = obj.GetType();
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                XmlSerializer xs = new XmlSerializer(type);
                xs.Serialize(writer, obj);
                writer.Close();
            }
        }

        /// <summary>XML -> OBJECT</summary>
        public static object LoadXml(string filePath, Type type)
        {
            if (!File.Exists(filePath))
                return null;
            using (StreamReader reader = new StreamReader(filePath))
            {
                XmlSerializer xs = new XmlSerializer(type);
                object obj = xs.Deserialize(reader);
                reader.Close();
                return obj;
            }
        }

        /// <summary>XML -> OBJECT（不推荐，要写一堆的Xml特性标签，且有些平台不支持。请用ParseXml()方法]]>）</summary>
        public static object XmlToObject(this string txt, Type type)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer xs = new XmlSerializer(type);
                object obj = xs.Deserialize(stream);
                stream.Close();
                return obj;
            }
        }
    }
}