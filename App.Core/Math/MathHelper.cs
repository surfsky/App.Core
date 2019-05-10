﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App.Core
{
    /// <summary>
    /// 数学相关辅助方法
    /// </summary>
    public static class MathHelper
    {
        /// <summary>计算两个GPS坐标的距离</summary>
        public static double CalcGPSDistance(double x1, double y1, double x2, double y2)
        {
            // https://blog.csdn.net/java_zhaoyanli/article/details/45973499
            double jd = 102834.74258026089786013677476285;
            double wd = 111712.69150641055729984301412873;
            double b = Math.Abs((x2 - x1) * jd);
            double a = Math.Abs((y2 - y1) * wd);
            return Math.Sqrt((a * a + b * b)) / 1000.0;
        }

        //--------------------------------------------------
        // 数字处理
        //--------------------------------------------------
        /// <summary>增加数字，不超过指定最大值</summary>
        /// <param name="i">数字。若为空，等价于0</param>
        /// <param name="n">增加值</param>
        /// <param name="max">最大值</param>
        public static int Inc(this int? i, int n, int? max = null)
        {
            return Inc(i ?? 0, n, max);
        }
        public static int Inc(this int i, int n, int? max = null)
        {
            int k = i + n;
            if (max != null)
                return (k > max.Value) ? max.Value : k;
            return k;
        }

        /// <summary>减少数字，不小于指定最小值</summary>
        /// <param name="i">数字。若为空，等价于0</param>
        /// <param name="n">减少值</param>
        /// <param name="max">最小值</param>
        public static int Dec(this int? i, int n, int? min = 0)
        {
            return Dec(i ?? 0, n, min);
        }
        public static int Dec(this int i, int n, int? min = 0)
        {
            int k = i - n;
            if (min != null)
                return (k < min.Value) ? min.Value : k;
            return k;
        }


        /// <summary>限制数字大小在一个区间内</summary>
        public static int Limit(this int n, int min, int max)
        {
            if (n > max) return max;
            if (n < min) return min;
            return n;
        }

        /// <summary>限制数字大小在一个区间内（泛型实现）</summary>
        public static T Limit<T>(this T n, T min, T max) where T : IComparable<T>
        {
            if (n.CompareTo(max) > 0) return max;
            if (n.CompareTo(min) < 0) return min;
            return n;
        }

        /// <summary>限制数字大小在一个区间内（Dynamic实现）</summary>
        public static dynamic Limit(dynamic n, dynamic min, dynamic max)
        {
            if (n > max) return max;
            if (n < min) return min;
            return n;
        }

        /// <summary>找到数组中最小的数</summary>
        public static int Min(params int[] data)
        {
            if (data.Length == 1) return data[0];
            if (data.Length == 2) return Math.Min(data[0], data[1]);
            int result = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] < result)
                    result = data[i];
            }
            return result;
        }
    }
}