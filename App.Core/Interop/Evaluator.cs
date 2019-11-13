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
        /// <summary>表达式求值</summary>
        public virtual object Eval(string expression)
        {
            throw new NotImplementedException();
        }

        /// <summary>字符串表达式求值</summary>
        public virtual DateTime EvalDateTime(string code)
        {
            object o = Eval(code);
            return Convert.ToDateTime(o);
        }

        #region 辅助函数
        /// <summary>整型值表达式求值</summary>
        public int EvalInteger(string code)
        {
            return Convert.ToInt32(Eval(code));
        }

        /// <summary>Double表达式求值</summary>
        public double EvalDouble(string code)
        {
            return Convert.ToDouble(Eval(code));
        }

        /// <summary>Decimal表达式求值</summary>
        public decimal EvalDecimal(string code)
        {
            return Convert.ToDecimal(Eval(code));
        }

        /// <summary>字符串表达式求值</summary>
        public string EvalString(string code)
        {
            return Convert.ToString(Eval(code));
        }

        /// <summary>Bool表达式求值</summary>
        public bool EvalBool(string code)
        {
            return Convert.ToBoolean(Eval(code));
        }
        #endregion
    }
}
