using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core
{
    /// <summary>
    /// Twitter flake id 生成器。用于分布式系统中生成全局唯一且递增的Id，可用于取代GUID。
    /// </summary>
    /// <remarks>
    /// 共64位（用long就可以容纳）
    ///   第1部分：01位，始终为0, 表示正数
    ///   第2部分：41位，精确为毫秒的时间戳, 从2000-01-01到2078-?
    ///   第3部分：10位，机器码（0-1023）
    ///   第4部分：12位，序列号（0-4095）
    ///   本算法颠倒了原算法二三部分的位置，使得生成的号码更为连续。
    /// 优点： 
    ///   按ID递增，易于在数据库中插入和检索，比GUI要好（128位且无序）
    ///   不依赖数据库，在内存中生成，性能好
    /// 参考：https://blog.csdn.net/weixin_40990818/article/details/82745567
    /// </remarks>
    public class SnowflakeID
    {
        //
        const int timestampBits = 41;           // 时间戳位数
        const int machineBits = 10;             // 机器编号位数
        const int sequenceBits = 12;            // 序列位数

        //
        const long timestampMask = (1L << timestampBits) - 1; // 掩码
        const long machineMask   = (1L << machineBits  ) - 1; // 掩码
        const long sequenceMask  = (1L << sequenceBits ) - 1; // 序列号掩码(0B1111_11111111)

        //
        static readonly object _lock = 0;       // 互斥锁
        static long lastTimestamp = -1L;        // 上次时间截
        static long lastSequence = 0;           // 上次序列号

        //
        public long TimeStamp { get; set; }     // 毫秒内序列(0~4095)
        public long Machine { get; set; }       // 毫秒内序列(0~4095)
        public long Sequence { get; set; }      // 毫秒内序列(0~4095)
        public long Value
        {
            get
            {
                // 移位并通过或运算拼到一起组成64位的ID
                long span = this.TimeStamp - new DateTime(2010, 1, 1).Ticks / 10000;
                return  (span << (machineBits + sequenceBits))
                      | (this.Machine << sequenceBits)
                      | this.Sequence
                      ;
            }
            private set
            {
                this.TimeStamp = (value >> (machineBits + sequenceBits)) & timestampMask;
                this.Machine   = (value >> sequenceBits) & machineMask;
                this.Sequence  = value & sequenceMask;
            }
        }

        /// <summary>解析ID结构</summary>
        public static SnowflakeID Parse(long id)
        {
            SnowflakeID snow = new SnowflakeID();
            snow.Value = id;
            return snow;
        }

        private SnowflakeID(){}

        /// <summary>构造函数</summary>
        /// <param name="machine">机器编码（0-1023）</param>
        public SnowflakeID(int machine)
        {
            if (machine > 1023 || machine < 0)
                throw new Exception("machine must less then 1024");
            this.Machine = machine;

            lock (_lock)
            {
                var timestamp = GetTimeStamp();
                if (timestamp != lastTimestamp)
                    lastSequence = 0;
                else
                {
                    // 如果是同一时间生成的，则进行毫秒内序列（性能要很好的机子才能跑的出来）
                    // 如果毫秒内序列溢出, 阻塞到下一个毫秒，获得新的时间戳
                    lastSequence = (lastSequence + 1) & sequenceMask;
                    if (lastSequence == 0)
                        timestamp = WaitNextMS(lastTimestamp);
                }
                lastTimestamp = timestamp;
                //System.Diagnostics.Trace.WriteLine(lastSequence);

                //
                this.TimeStamp = timestamp;
                this.Sequence = lastSequence;
            }
        }


        // 阻塞到下一个毫秒，直到获得新的时间戳
        static long WaitNextMS(long lastTimestamp)
        {
            long timestamp = GetTimeStamp();
            while (timestamp <= lastTimestamp)
                timestamp = GetTimeStamp();
            return timestamp;
        }

        //  获取时间戳（毫秒）
        static long GetTimeStamp()
        {
            return DateTime.Now.Ticks / 10000;
        }
    }
}
