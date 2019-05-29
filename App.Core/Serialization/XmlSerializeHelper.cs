using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace App.Core
{
    /// <summary>
    /// Xml 序列化反序列化辅助类（使用微软官方的 XmlSerializer，使用时需要给类增加Xml相关attribue）
    /// </summary>
    [Obsolete("推荐使用 App.Core.Xmlizer")]
    public static class XmlSerializeHelper
    {
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