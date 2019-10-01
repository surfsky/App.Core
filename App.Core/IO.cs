﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace App.Core
{
    /// <summary>
    /// IO 辅助方法（文件、路径、程序集）
    /// </summary>
    public static partial class IO
    {
        //------------------------------------------------
        // 程序集
        //------------------------------------------------
        /// <summary>获取主入口数据集版本号</summary>
        public static Version AssemblyVersion
        {
            get { return Assembly.GetEntryAssembly().GetName().Version; }
        }

        /// <summary>获取主入口数据集路径</summary>
        public static string AssemblyPath
        {
            get { return Assembly.GetEntryAssembly().Location; }
        }

        /// <summary>获取调用者数据集目录</summary>
        public static string AssemblyDirectory
        {
            get { return new FileInfo(AssemblyPath).DirectoryName; }
        }

        /// <summary>获取某个类型归属的程序集版本号</summary>
        public static Version GetVersion(Type type)
        {
            return type.Assembly.GetName().Version;
        }

        //------------------------------------------------
        // 输出
        //------------------------------------------------
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

        //------------------------------------------------------------
        // 缓存相关
        //------------------------------------------------------------
        /// <summary>清除缓存对象</summary>
        public static void RemoveCache(string key)
        {
            var cache = HttpRuntime.Cache;
            if (cache[key] != null)
            {
                cache.Remove(key);
                System.Diagnostics.Debug.WriteLine("Clear cache : " + key);
            }
        }

        /// <summary>设置缓存对象</summary>
        public static void SetCache<T>(string key, T value, DateTime? expiredTime=null) where T : class
        {
            if (value != null)
            {
                expiredTime = expiredTime ?? Cache.NoAbsoluteExpiration;
                var cache = HttpRuntime.Cache;
                cache.Insert(key, value, null, expiredTime.Value, Cache.NoSlidingExpiration);
                System.Diagnostics.Debug.WriteLine("Create cache : " + key);
            }
        }

        /// <summary>获取缓存对象（缓存有有效期，一旦失效会自动获取）</summary>
        public static T GetCache<T>(string key, Func<T> creator=null, DateTime? expiredTime=null) where T : class
        {
            expiredTime = expiredTime ?? Cache.NoAbsoluteExpiration;
            var cache = HttpRuntime.Cache;    // 可在非Web环境使用
            if (creator == null)
                return cache[key] as T;
            else
            {
                if (cache[key] == null)
                {
                    T o = creator();
                    SetCache(key, o, expiredTime.Value);
                }
                return cache[key] as T;
            }
        }


    }
}
