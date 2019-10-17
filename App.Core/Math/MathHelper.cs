using System;
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
        //--------------------------------------------
        // 小数的相等判断
        //--------------------------------------------
        /// <summary>约等于</summary>
        /// <param name="precision">精度，如0.01f</param>
        /// <example>bool b = MathHelper.Equals(1.153f, 1.152f, 0.001f);</example>
        /// <remarks>注意: 小数的精度问题会导致例如 1.153 -1.152 大于 0.001 的错误，故该方法实际计算时会将精度乘以1.1(如0.001 变为 0.0011)，以避免逻辑上出错</remarks>
        public static bool Approx(this float n1, float n2, float precision)
        {
            precision = precision * 1.1f;
            var v = n1 - n2;
            return Math.Abs(v) <= precision;
        }
        /// <summary>约等于</summary>
        /// <param name="precision">精度，如0.01f</param>
        /// <example>bool b = MathHelper.Equals(1.153f, 1.152f, 0.001f);</example>
        /// <remarks>注意: 小数的精度问题会导致例如 1.153 -1.152 大于 0.001 的错误，故该方法实际计算时会将精度乘以1.1(如0.001 变为 0.0011)，以避免逻辑上出错</remarks>
        public static bool Approx(this double n1, double n2, double precision)
        {
            precision = precision * 1.1d;
            var v = n1 - n2;
            return Math.Abs(v) <= precision;
        }
        /// <summary>约等于</summary>
        /// <param name="precision">精度，如0.01f</param>
        /// <example>bool b = MathHelper.Equals(1.153f, 1.152f, 0.001f);</example>
        /// <remarks>注意: 小数的精度问题会导致例如 1.153 -1.152 大于 0.001 的错误，故该方法实际计算时会将精度乘以1.1(如0.001 变为 0.0011)，以避免逻辑上出错</remarks>
        public static bool Approx(this decimal n1, decimal n2, decimal precision)
        {
            precision = precision * 1.1m;
            var v = n1 - n2;
            return Math.Abs(v) <= precision;
        }

        //--------------------------------------------------
        // Distance
        //--------------------------------------------------
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
        public static int Inc(this ref int? i, int n, int? max = null)
        {
            if (i == null) i = 0;
            return Inc(ref i, n, max);
        }
        public static int Inc(this ref int i, int n, int? max = null)
        {
            i = i + n;
            if (max != null)
                i = (i > max.Value) ? max.Value : i;
            return i;
        }

        /// <summary>减少数字，不小于指定最小值</summary>
        /// <param name="i">数字。若为空，等价于0</param>
        /// <param name="n">减少值</param>
        /// <param name="max">最小值</param>
        public static int Dec(this ref int? i, int n, int? min = 0)
        {
            if (i == null) i = 0;
            return Dec(ref i, n, min);
        }
        public static int Dec(this ref int i, int n, int? min = 0)
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