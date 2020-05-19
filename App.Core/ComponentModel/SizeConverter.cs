using App.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.BLL
{
    /// <summary>
    /// Size 和 String 互转
    /// </summary>
    public class SizeConverter : TypeConverter
    {
        //public class EnumToStringConverter<TEnum> : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<TEnum, string> where TEnum : struct

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var txt = value as string;
            return ParseSize(txt);

        }

        /// <summary>将文本解析为 Size 对象</summary>
        /// <param name="txt">格式如：20,20</param>
        public Size ParseSize(string txt)
        {
            var items = txt.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length >= 2)
                return new Size(items[0].ParseInt().Value, items[1].ParseInt().Value);
            return new Size(0, 0);
        }
    }

    /// <summary>
    /// 辅助方法
    /// </summary>
    public static class SizeExtensions
    {
        /// <summary>将文本解析为 Size 对象</summary>
        /// <param name="txt">格式如：20,20</param>
        public static Size ParseSize(this string txt)
        {
            var items = txt.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length >= 2)
                return new Size(items[0].ParseInt().Value, items[1].ParseInt().Value);
            return new Size(0, 0);
        }

        /// <summary>将 Size 对象转化为文本</summary>
        /// <returns>格式如：20,20</returns>
        public static string ToText(this Size size)
        {
            return string.Format("{0},{1}", size.Width, size.Height);
        }
    }
}
