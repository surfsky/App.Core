using System;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.JScript;

namespace App.Utils
{
    /// <summary>
    /// 使用动态编译＋Js.Eval＋Invoke技术实现Eval函数
    /// </summary>
    public class JsEvaluator : Evaluator
    {
        // 私有变量
        private static object _evaluator = null;
        private static Type _evaluatorType = null;
        private static readonly string _jscriptSource =
            @"
            package Evaluator
            {
                class Evaluator
                {
                    public function Eval(expr: String) : String
                    {
                        return eval(expr);
                    }
                    // 由于js和c#格式不一致，时间类型必须用这个函数，否则c#无法解析
                    public function EvalDate(expr: String) : String
                    {
                        return eval(expr).toLocaleString();
                    }
                }
            }";

        // 静态构造函数。编译js模块并放于内存中。
        static JsEvaluator()
        {
            //ICodeCompiler compiler = new JScriptCodeProvider().CreateCompiler();  // 该接口已废弃
            CodeDomProvider compiler = CodeDomProvider.CreateProvider("js");
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            CompilerResults results = compiler.CompileAssemblyFromSource(parameters, _jscriptSource);
            Assembly assembly = results.CompiledAssembly;
            _evaluatorType = assembly.GetType("Evaluator.Evaluator");
            _evaluator = Activator.CreateInstance(_evaluatorType);
        }

        /// <summary>解析表达式值</summary>
        public override object Eval(string expression)
        {
            return _evaluatorType.InvokeMember(
                "Eval",
                BindingFlags.InvokeMethod,
                null,
                _evaluator,
                new object[] { expression }
            );
        }

        /// <summary>
        /// 转化为日期时间必须用这个函数。格式如：new Date('2018/01/01 12:00:00')
        /// </summary>
        public override DateTime EvalDateTime(string expression)
        {
            object o = _evaluatorType.InvokeMember(
                "EvalDate",
                BindingFlags.InvokeMethod,
                null,
                _evaluator,
                new object[] { expression }
            );
            return System.Convert.ToDateTime(o);
        }

    }
}
