using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils
{
    /// <summary>
    /// Twitter SnowflakeID 分布式 ID 生成器。
    /// 用于分布式系统中生成全局唯一且递增的Id，可用于取代数据库自增（易被试探）及GUID（性能很差）。
    /// </summary>
    /// <remarks>
    /// 共64位（用 long 容纳），默认分配如下:
    ///   第1部分：01位，始终为0, 表示正数
    ///   第2部分：41位，精确为毫秒的时间戳, 可以使用 2^41 毫秒 = 69.73 年
    ///   第3部分：10位，机器码（0-1023）
    ///   第4部分：12位，序列号（0-4095）
    /// 优点： 
    ///   按ID递增，易于在数据库中插入和检索，比GUID要好（128位且无序）
    ///   不依赖数据库，在内存中生成，性能好
    /// 备注：
    ///   实际使用时要创建单例对象，减少创建开销，加快生成速度。
    ///   经实际测试，毫秒内的序列号可以生成完整（0-4095）
    ///   一般一个系统也不会使用70年，基本够用了。
    ///   实在不够用了怎么办? 时间戳再分配几位，如43位 => 2^43毫秒 = 278.92 年
    /// 参考：
    ///   https://blog.csdn.net/weixin_40990818/article/details/82745567
    /// </remarks>
    /// <example>
    /// var snow = new SnowflakeID(1, 2010);
    /// var id = snow.NewID();
    /// </example>
    public class SnowflakeID
    {
        // 属性
        public int  StartYear { get; set; }     // 开始年份
        public long TimeStamp { get; set; }     // 时间戳毫秒
        public long Machine { get; set; }       // 设备编号
        public long Sequence { get; set; }      // 毫秒内序列

        // 位数
        public int TimeStampBits = 41;           // 时间戳位数
        public int MachineBits = 10;             // 机器编号位数
        public int SequenceBits = 12;            // 序列位数

        // 掩码
        private long _timestampMask;
        private long _machineMask;
        private long _sequenceMask;

        // 静态变量
        static readonly object _lock = new object();     // 互斥锁
        static long _lastTimeStamp = -1L;        // 上次时间截
        static long _lastSequence = 0;           // 上次序列号

        /// <summary>长整形值</summary>
        public long Value
        {
            get
            {
                // 移位并通过或运算拼到一起组成64位的ID
                long span = this.TimeStamp - new DateTime(this.StartYear, 1, 1).Ticks / 10000;
                return  (span << (MachineBits + SequenceBits))
                      | (this.Machine << SequenceBits)
                      | this.Sequence
                      ;
            }
            private set
            {
                // 解析
                this.TimeStamp = (value >> (MachineBits + SequenceBits)) & _timestampMask;
                this.Machine   = (value >> SequenceBits) & _machineMask;
                this.Sequence  = value & _sequenceMask;
            }
        }

        /// <summary>解析ID结构</summary>
        public static SnowflakeID Parse(long id)
        {
            SnowflakeID snow = new SnowflakeID();
            snow.Value = id;
            return snow;
        }

        /// <summary>构造函数</summary>
        /// <param name="machine">机器编码（0-1023）</param>
        public SnowflakeID(int machine=1, int startYear=2010, int timeStampBits=41, int machineBits=10, int sequenceBits=12)
        {
            if (machine > 1023 || machine < 0)
                throw new Exception("machine must less then 1024");
            this.Machine = machine;
            this.StartYear = startYear;
            this.TimeStampBits = timeStampBits;
            this.MachineBits = machineBits;
            this.SequenceBits = sequenceBits;
            // 掩码
            this._timestampMask = (1L << TimeStampBits) - 1;
            this._machineMask   = (1L << MachineBits) - 1;
            this._sequenceMask  = (1L << SequenceBits) - 1;
        }


        /*
        /// <summary>生成</summary>
        public static long NewID(int machine = 1, int startYear = 2010, int timeStampBits = 41, int machineBits = 10, int sequenceBits = 12)
        {
            return new SnowflakeID(machine, startYear, timeStampBits, machineBits, sequenceBits)
                .New().Value;
        }
        */

        /// <summary>生成</summary>
        public long NewID()
        {
            return New().Value;
        }

        /// <summary>生成</summary>
        protected SnowflakeID New()
        {
            lock (_lock)
            {
                var timestamp = GetTimeStamp();
                if (timestamp != _lastTimeStamp)
                    _lastSequence = 0;
                else
                {
                    // 如果是同一毫秒生成的，则进行毫秒内序列（默认12位，值范围为0-4095, 2^12-1）
                    // 如果毫秒内序列溢出, 阻塞到下一个毫秒，获得新的时间戳
                    _lastSequence = (_lastSequence + 1) & _sequenceMask;
                    if (_lastSequence == 0)
                        timestamp = WaitNextMS(_lastTimeStamp);
                }
                _lastTimeStamp = timestamp;

                //
                this.TimeStamp = timestamp;
                this.Sequence = _lastSequence;
                return this;
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

        /// <summary>SnowflakeID 生成器单例</summary>
        public static SnowflakeID Instance
        {
            get
            {
                return IO.GetCache<SnowflakeID>("__SnowFlaker", () => {
                    var machineId = CoreConfig.Instance.MachineId;
                    return new SnowflakeID(machineId, 2010, 41, 10, 12);
                });
            }
        }
    }
}
