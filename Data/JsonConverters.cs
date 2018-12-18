﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace App.Core
{
    //-------------------------------------------------
    // Json 序列化转换器
    //-------------------------------------------------
    /// <summary>
    /// 简单的日期时间格式化
    /// </summary>
    public class DateTimeConverter : IsoDateTimeConverter
    {
        public DateTimeConverter() : base()
        {
            base.DateTimeFormat = "yyyy/MM/dd HH:mm";
        }
    }

    /// <summary>  
    /// DateTime序列化为时间戳  
    /// </summary>  
    public class TimestampConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            int seconds = int.Parse(reader.Value.ToString());
            return new DateTime(1970, 1, 1).AddSeconds(seconds);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dt = (DateTime)value;
            int n = (int)(dt - new DateTime(1970, 1, 1)).TotalSeconds;
            writer.WriteValue(n);
        }
    }

    /// <summary>  
    /// String Unicode 序列化, 输出为Unicode编码字符）
    /// </summary>  
    public class UnicodeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(ToUnicode(value.ToString()));
        }

        public static string ToUnicode(string str)
        {
            byte[] bts = Encoding.Unicode.GetBytes(str);
            string r = "";
            for (int i = 0; i < bts.Length; i += 2)
            {
                r += "\\u" + bts[i + 1].ToString("X").PadLeft(2, '0') + bts[i].ToString("X").PadLeft(2, '0');
            }
            return r;
        }
    }
}
