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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;


namespace App.Core
{
    /// <summary>
    /// 类型转换-Json
    /// </summary>
    public static partial class Convertor
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
        public static void SaveJson(string filePath, object obj, JsonSerializerSettings settings = null)
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
        public static object LoadJson(string filePath, Type type, JsonSerializerSettings settings = null)
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
            settings.Formatting = Newtonsoft.Json.Formatting.Indented;                                  // 递进
            settings.MaxDepth = 10;                                                     // 设置序列化的最大层数  
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;              // 指定如何处理循环引用
            settings.Converters.Add(new StringEnumConverter());                         // 枚举输出为字符串
            return settings;
        }
    }
}