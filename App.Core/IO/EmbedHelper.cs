using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// 嵌入资源读取辅助类
    /// 注意：如果资源目录以数字打头，编译成资源时会加一个下划线 _，如/res/2.9.3 会编译成  /res/_2._9._3
    /// 所以要么完善该类，要么资源目录不要以数字打头
    /// </summary>
    public class EmbedHelper
    {
        /// <summary>获取数据集中的资源名称列表</summary>
        public static string[] GetResNames(Assembly assembly)
        {
            return assembly.GetManifestResourceNames();
        }

        /// <summary>获取数据集中的资源流</summary>
        /// <param name="assembly">数据集</param>
        /// <param name="resName">资源名称</param>
        /// <param name="caseSensitive">是否大小写敏感</param>
        public static Stream GetStream(Assembly assembly, string resName, bool caseSensitive = false)
        {
            if (caseSensitive)
                return assembly.GetManifestResourceStream(resName);
            else
            {
                resName = resName.ToLower();
                var names = assembly.GetManifestResourceNames();  // 考虑用 cache 加速
                foreach (string name in names)
                    if (resName == name.ToLower())
                        return assembly.GetManifestResourceStream(name);
                return null;
            }
        }


        /// <summary>获取数据集中的字节数组</summary>
        public static byte[] GetBytes(Assembly assembly, string resName, bool caseSensitive = false)
        {
            Stream stream = GetStream(assembly, resName, caseSensitive);
            if (stream == null) 
                return new byte[] { };
            else
            {
                byte[] buffer = new byte[stream.Length];
                int len = stream.Read(buffer, 0, (int)stream.Length);
                return buffer;
            }
        }


        /// <summary>获取数据集中的文本文件</summary>
        public static string GetText(Assembly assembly, string resName, bool caseSensitive = false)
        {
            Stream stream = GetStream(assembly, resName, caseSensitive);
            if (stream == null) 
                return "";
            else
            {
                byte[] buffer = new byte[stream.Length];
                int len = stream.Read(buffer, 0, (int)stream.Length);
                string temp = Encoding.UTF8.GetString(buffer, 0, len);
                return temp;
            }
        }

        /// <summary>获取数据集中的图片文件</summary>
        public static Image GetImage(Assembly assembly, string resName, bool caseSensitive = false)
        {
            Stream s = GetStream(assembly, resName, caseSensitive);
            return Image.FromStream(s);
        }

    }
}
