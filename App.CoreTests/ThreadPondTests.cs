using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace App.Utils.Tests
{
    [TestClass()]
    public class ThreadPondTests
    {
        //[TestMethod()]
        public void ThreadPondTest()
        {
            // 使用线程池（容量10），开启100个线程
            var pool = new ThreadPond(10);
            pool.Log += (msg) =>
            {
                IO.Write("[{0} {1}] {2}", DateTime.Now.ToString("HH:mm:ss"), Thread.CurrentThread.ManagedThreadId, msg);
            };
            for (int i=0; i<100; i++)
            {
                var now = DateTime.Now;
                var exp = now.AddSeconds(2);
                pool.Start(new Action<object>(Dummy), i, exp);
                Thread.Sleep(10);
            }
            
            void Dummy(object o)
            {
               IO.Write("[{0} {1}] 开启任务 {2}", DateTime.Now.ToString("HH:mm:ss"), Thread.CurrentThread.ManagedThreadId, o);
               Thread.Sleep(1000*10);
            }
        }
    }
}