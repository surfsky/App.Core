using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.Interop
{
    /// <summary>
    /// 表达式计算器基类
    /// </summary>
    public class Evaluator
    {
        // 虚函数
        public virtual object Eval(string expression)
        {
            throw new NotImplementedException();
        }

        public virtual DateTime EvalDateTime(string code)
        {
            object o = Eval(code);
            return Convert.ToDateTime(o);
        }

        #region 辅助函数
        public int EvalInteger(string code)
        {
            return Convert.ToInt32(Eval(code));
        }

        public double EvalDouble(string code)
        {
            return Convert.ToDouble(Eval(code));
        }

        public decimal EvalDecimal(string code)
        {
            return Convert.ToDecimal(Eval(code));
        }

        public string EvalString(string code)
        {
            return Convert.ToString(Eval(code));
        }

        public bool EvalBool(string code)
        {
            return Convert.ToBoolean(Eval(code));
        }
        #endregion
    }
}
