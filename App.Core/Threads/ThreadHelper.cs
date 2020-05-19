using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace App.Utils
{
    /// <summary>
    /// 进程线程相关辅助方法
    /// </summary>
    public class ThreadHelper
    {
        /// <summary>是否已存在进程</summary>
        public static bool ExistProcess(string fileName)
        {
            var name = new FileInfo(fileName).Name;
            int n = name.LastIndexOf(".");
            var processName = name.Substring(0, n);
            Process[] processes = Process.GetProcessesByName(processName);
            foreach (Process process in processes)
                if (process.MainModule.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }


        /// <summary>运行进程</summary>
        public static void Run(string exeFilePath)
        {
            Process.Start(exeFilePath);
        }

        /// <summary>运行进程，并保持运行（自动检测并重启）</summary>
        public static void RunAndKeepLiving(string exeFilePath, int sleep)
        {
            var obj = new { FilePath = exeFilePath, Sleep = sleep };
            new Thread(new ParameterizedThreadStart(RunLoop)).Start(obj);
        }
        static void RunLoop(dynamic p)
        {
            string fileName = p.FilePath;
            int sleep = p.Sleep;
            while (true)
            {
                if (!ExistProcess(fileName))
                    Process.Start(fileName);
                Thread.Sleep(sleep);
            }
        }
    }
}
