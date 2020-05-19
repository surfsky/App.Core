using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils
{
    /// <summary>
    /// 字体操作辅助方法
    /// </summary>
    public class FontHelper
    {
        /// <summary>从资源中获取字体（注意支持的样式要匹配，否则会报错）</summary>
        public static Font GetFontFromRes(string fontRes, int size, FontStyle style, Assembly assembly = null)
        {
            if (assembly == null)
                assembly = typeof(FontHelper).Assembly;
            var stream = assembly.GetManifestResourceStream(fontRes);
            byte[] fontData = new byte[stream.Length];
            stream.Read(fontData, 0, (int)stream.Length);
            stream.Close();

            return GetFont(fontData, size, style);
        }

        /// <summary>从内存中获取字体</summary>
        public static Font GetFont(byte[] fontData, int size, FontStyle style)
        {
            var fonts = new System.Drawing.Text.PrivateFontCollection();
            unsafe
            {
                fixed (byte* pFontData = fontData)
                {
                    fonts.AddMemoryFont((IntPtr)pFontData, fontData.Length);
                }
            }
            var font = new Font(fonts.Families[0], size, style);
            return font;
        }

        /// <summary>从文件中获取字体</summary>
        public static Font GetFontFromFile(string fontPath, int size, FontStyle style)
        {
            var fonts = new System.Drawing.Text.PrivateFontCollection();
            fonts.AddFontFile(fontPath);
            return new Font(fonts.Families[0], size, style);
        }
    }
}
