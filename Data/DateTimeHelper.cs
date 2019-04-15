using App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App.Core
{
    /// <summary>
    /// 星期几。数据同DayOfWeek，添加了中文注释。
    /// </summary>
    public enum WeekDay
    {
        [UI("星期一")] Monday = 1,
        [UI("星期二")] Tuesday = 2,
        [UI("星期三")] Wednesday = 3,
        [UI("星期四")] Thursday = 4,
        [UI("星期五")] Friday = 5,
        [UI("星期六")] Saturday = 6,
        [UI("星期日")] Sunday = 0
    }

    /// <summary>
    /// 日期时间扩展方法
    /// </summary>
    public static class DateTimeExtension
    {
        /// <summary>截取年月日信息</summary>
        public static DateTime TrimDay(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day);
        }
    }

    /// <summary>
    /// 日期时间辅助方法
    /// </summary>
    public static class DateTimeHelper
    {
        //------------------------------------------------
        // 批量安排日期
        //------------------------------------------------
        /// <summary>安排日期</summary>
        /// <param name="startDay">开始日期</param>
        /// <param name="endDay">结束日期</param>
        /// <param name="weekdays">每周几（注意星期日为0）</param>
        public static List<DateTime> ArrangeDays(DateTime startDay, DateTime endDay, List<int> weekdays)
        {
            List<DateTime> dts = new List<DateTime>();
            var days = (endDay - startDay).Days;
            for (int i = 0; i < days; i++)
            {
                var dt = startDay.AddDays(i);
                if (weekdays.Contains((int)dt.DayOfWeek))
                    dts.Add(dt);
            }
            return dts;
        }

        /// <summary>安排日期</summary>
        /// <param name="startDay">开始日期</param>
        /// <param name="times">排几次</param>
        /// <param name="weekdays">每周几（注意星期日为0）</param>
        public static List<DateTime> ArrangeDays(DateTime startDay, int times, List<int> weekdays)
        {
            List<DateTime> dts = new List<DateTime>();
            for (int i = 0; i < times; i++)
            {
                var dt = startDay.AddDays(i);
                if (weekdays.Contains((int)dt.DayOfWeek))
                    dts.Add(dt);
            }
            return dts;
        }


        //------------------------------------------------
        // 星期几相关
        //------------------------------------------------
        /// <summary>得到同周星期几的日期</summary>
        /// <param name="someDate">参照日期</param>
        /// <param name="weekday">星期几</param>
        public static DateTime GetWeekdayDt(DateTime someDate, DayOfWeek weekday)
        {
            int i = weekday - someDate.DayOfWeek;
            return someDate.AddDays(i);
        }

        /// <summary>获取日期对应的星期几字符串</summary>
        public static string GetWeekday(DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:    return "星期一";
                case DayOfWeek.Tuesday:   return "星期二";
                case DayOfWeek.Wednesday: return "星期三";
                case DayOfWeek.Thursday:  return "星期四";
                case DayOfWeek.Friday:    return "星期五";
                case DayOfWeek.Saturday:  return "星期六";
                case DayOfWeek.Sunday:    return "星期日";
                default:                  return "星期一";
            }
        }


        //------------------------------------------------
        // 时间戳
        //------------------------------------------------
        /// <summary>日期格式转成时间戳字符串（从1970年到现在的秒数）</summary>
        public static string ToTimeStamp(this DateTime dt)
        {
            TimeSpan ts = dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }


        /// <summary>解析时间戳为时间</summary>
        /// <param name="timeStamp">Unix时间戳格式</param>
        /// <returns>C#格式时间</returns>
        public static DateTime ParseTimeStamp(this string timeStamp)
        {
            DateTime dt = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dt.Add(toNow);
        }


    }
}