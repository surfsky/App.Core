using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
 
namespace App.Utils
{
    /// <summary>
    /// 通用易用的线程池管理器
    /// （1）简易的创建线程并启动
    /// （2）若线程池已满，则等待
    /// （3）若线程运行时间过久，则自动关闭，释放线程资源
    /// PS.为了避免与 System.Threading.ThreadPool 重名，故改为 ThreadPond
    /// </summary>
    /// <example>
    /// // 使用线程池（容量10）开启100个线程
    /// ThreadPond pool = new ThreadPond(10);
    /// for (int i=0; i&lt;100; i++)
    /// {
    ///     var now = DateTime.Now;
    ///     pool.Start(new Action(object)(Dummy), now, now.AddHours(1));
    ///     pool.Start((T)=>Console.WriteLine(T.ToString()), now, now.AddHours(1));
    ///     Thread.Sleep(100):
    /// }
    /// 
    /// void Dummy(object o)
    /// {
    ///    Console.WriteLine(o.ToString());
    ///    Thread.Sleep(100);
    /// }
    /// </example>
    public class ThreadPond
    {
        /// <summary>
        /// 线程信息
        /// </summary>
        protected class ThreadInfo
        {
            /// <summary>需在线程中运行的函数（带一个输入参数）</summary>
            public Action<object> Action;

            /// <summary>线程函数对应的参数</summary>
            public object Parameter;

            //
            // 线程相关的信息
            //
            public Thread Thread;
            public int ThreadId;
            public DateTime StartTime;
            public DateTime ExpiredTime;
            public bool Running = false;
        }

        // 私有成员
        int _searchSleep = 5;                                   // 寻找空闲线程的时间间隔
        int _closeSleep  = 10;                                  // 寻找空闲线程的时间间隔
        List<ThreadInfo> _threadInfos = new List<ThreadInfo>(); // 线程信息队列
        public event Action<string> Log;

        //--------------------------------------------
        /// <summary>构造函数</summary>
        /// <param name="maxThreads">允许同时开启的最大线程数目</param>
        public ThreadPond(int maxThreads=20)
        {
            // 初始化队列
            _threadInfos.Clear();
            for (int i = 0; i < maxThreads; i++)
                _threadInfos.Add(new ThreadInfo());

            // 开启后台清理线程，用于清理过期线程
            new Thread(new ThreadStart(CloseExpiredThreads)).Start();
        }

        /// <summary>启动任务（阻塞式），若无可用线程则等待至有为止。</summary>
        /// <param name="action">任务函数（一个输入参数、无输出参数）</param>
        /// <param name="parameter">任务函数对应的参数</param>
        /// <param name="maxProcessSeconds">每个线程最大处理时间（秒）</param>
        public void Start(Action<object> action, object parameter, DateTime expiredTime)
        {
            // 搜索空闲的线程id（阻塞式）
            int id = SearchIdleThreadId();
            while (id == -1)
            {
                Thread.Sleep(_searchSleep);
                id = SearchIdleThreadId();
            }

            // 开启线程
            ThreadInfo info = _threadInfos[id] = new ThreadInfo();
            info.Action = action;
            info.Parameter = parameter;
            info.ThreadId = id;
            info.StartTime = DateTime.Now;
            info.ExpiredTime = expiredTime;
            info.Thread = new Thread(new ParameterizedThreadStart(StartThread));
            info.Thread.Start(info);
        }


        //--------------------------------------------
        // 线程处理主体
        void StartThread(object o)
        {
            ThreadInfo info = o as ThreadInfo;
            SetThreadStatus(info, true);
            info.Action(info.Parameter);
            SetThreadStatus(info, false);
        }

        // 搜索空闲线程
        int SearchIdleThreadId()
        {
            for (int i = 0; i < _threadInfos.Count; i++)
                if (!_threadInfos[i].Running)
                    return i;
            return -1;
        }

        // 关闭过期的线程
        void CloseExpiredThreads()
        {
            Log.Invoke("ThreadPool 监控线程开启");
            while (true)
            {
                for (int i=0; i<_threadInfos.Count; i++)
                {
                    ThreadInfo info = _threadInfos[i];
                    if (info.Running && info.ExpiredTime <= DateTime.Now)
                    {
                        var txt = string.Format("线程 {0} 超时关闭", info.ThreadId);
                        CloseThread(info, txt);
                    }
                }

                Thread.Sleep(_closeSleep);
            }
        }

        /// <summary>关闭线程（并记录日志）</summary>
        private void CloseThread(ThreadInfo info, string txt)
        {
            Log.Invoke(txt);
            SetThreadStatus(info, false);
            if (info.Thread != null && info.Thread.ThreadState != ThreadState.Aborted)
            {
                try
                {
                    info.Thread.Abort();
                }
                catch 
                {

                }
            }
        }

        // 设置线程状态
        void SetThreadStatus(ThreadInfo info, bool running)
        {
            var txt = string.Format("线程 {0} {1}", info.ThreadId, info.Running ? "开启" : "关闭");
            Log.Invoke(txt);
            lock (info)
            {
                info.Running = running;
            }
        }
    }
}
