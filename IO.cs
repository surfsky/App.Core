using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// IO 辅助方法
    /// </summary>
    public static class IO
    {
        /// <summary>准备文件路径（不存在则创建）</summary>
        /// <param name="filePath">文件的物理路径</param>
        public static void PrepareDirectory(string filePath)
        {
            var fi = new FileInfo(filePath);
            if (!Directory.Exists(fi.Directory.FullName))
                Directory.CreateDirectory(fi.Directory.FullName);
        }

        /// <summary>写文件（附加）</summary>
        /// <param name="filePath">文件的物理路径</param>
        public static void WriteFile(string filePath, string data)
        {
            var fileinfo = new FileInfo(filePath);
            using (FileStream fs = fileinfo.OpenWrite())
            {
                var sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.Write(data);
                sw.Flush();
                sw.Close();
            }
        }

        /// <summary>打印到调试窗口</summary>
        public static void Trace(string format, params object[] args)
        {
            System.Diagnostics.Trace.WriteLine(GetText(format, args));
        }


        /// <summary>打印到控制台窗口</summary>
        public static void Console(string format, params object[] args)
        {
            System.Console.WriteLine(GetText(format, args));
        }

        /// <summary>打印到调试窗口</summary>
        public static void Debug(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(GetText(format, args));
        }

        /// <summary>打印到所有输出窗口</summary>
        public static void Write(string format, params object[] args)
        {
            Trace(format, args);
            Console(format, args);
            //Debug(format, args);
        }

        public static string GetText(string format, object[] args)
        {
            return (args.Length == 0) ? format : string.Format(format, args);
        }
    }
}
