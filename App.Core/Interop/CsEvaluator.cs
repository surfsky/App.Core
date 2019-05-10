using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.CSharp;

namespace App.Interop
{
    /// <summary>
    /// 表达式项
    /// </summary>
    public class EvaluatorItem
    {
        public EvaluatorItem(Type returnType, string expression, string name)
        {
            ReturnType = returnType;
            Expression = expression;
            Name = name;
        }

        public Type ReturnType;
        public string Name;
        public string Expression;
    }


    /// <summary>
    /// C# 表达式计算器
    /// From: http://www.codeproject.com/csharp/runtime_eval.asp
    /// C# 4.0 中提供了该类叫 CSharpEvaluator
    /// </summary>
    /// <example>
    ///		Console.WriteLine("{0}", Evaluator.EvaluateToObject("System.DateTime.Now"));
	///		Console.WriteLine("Test0: {0}", Evaluator.EvaluateToInteger("(30 + 4) * 2"));
	///		Console.WriteLine("Test1: {0}", Evaluator.EvaluateToString("\"Hello \" + \"There\""));
	///		Console.WriteLine("Test2: {0}", Evaluator.EvaluateToBool("30 == 40"));
	///		Console.WriteLine("Test3: {0}", Evaluator.EvaluateToObject("new DataSet()"));
    ///
	///		EvaluatorItem[] items = {
	///			new EvaluatorItem(typeof(int), "(30 + 4) * 2", "GetNumber"),
	///			new EvaluatorItem(typeof(string), "\"Hello \" + \"There\"", "GetString"),
	///			new EvaluatorItem(typeof(bool), "30 == 40", "GetBool"),
	///			new EvaluatorItem(typeof(object), "new DataSet()", "GetDataSet")
	///		};
    ///  
    ///		CSharpEvaluator eval = new CSharpEvaluator(items);
	///		Console.WriteLine("TestStatic0: {0}", eval.EvaluateInt("GetNumber"));
	///		Console.WriteLine("TestStatic1: {0}", eval.EvaluateString("GetString"));
	///		Console.WriteLine("TestStatic2: {0}", eval.EvaluateBool("GetBool"));
    ///		Console.WriteLine("TestStatic3: {0}", eval.Evaluate("GetDataSet"));
    /// </example>
    public class CsEvaluator : Evaluator
    {
        //---------------------------------------
        // Private
        //---------------------------------------
        const string _defaultMethodName = "__foo";
        private object _instance = null;
        public List<string> Usings = new List<string>();
        public List<string> References = new List<string>();


        //---------------------------------------
        // Construction
        //---------------------------------------
        public CsEvaluator() { }
        public CsEvaluator(string expression)
        {
            EvaluatorItem[] items = { new EvaluatorItem(typeof(object), expression, _defaultMethodName) };
            ConstructEvaluator(items);
        }

        public CsEvaluator(Type returnType, string expression, string methodName)
        {
            EvaluatorItem[] items = { new EvaluatorItem(returnType, expression, methodName) };
            ConstructEvaluator(items);
        }

        public CsEvaluator(EvaluatorItem item)
        {
            EvaluatorItem[] items = { item };
            ConstructEvaluator(items);
        }

        public CsEvaluator(EvaluatorItem[] items)
        {
            ConstructEvaluator(items);
        }

        //---------------------------------------
        // Core
        //---------------------------------------
        private void ConstructEvaluator(EvaluatorItem[] items)
        {
            // C#代码块
            StringBuilder code = new StringBuilder();
            AddUsings(ref code);
            code.Append("namespace MyNameSpace { \n");
            code.Append("  public class _Evaluator { \n");
            foreach (EvaluatorItem item in items)
            {
                code.AppendFormat("    public {0} {1}() ", item.ReturnType.Name, item.Name);
                code.AppendFormat("    {{ ");
                code.AppendFormat("      return ({0}); ", item.Expression);
                code.AppendFormat("    }}\n");
            }
            code.Append("} }");

            // 编译器
            //ICodeCompiler comp = (new CSharpCodeProvider().CreateCompiler());  // 该接口已经废除
            CodeDomProvider cdp = CodeDomProvider.CreateProvider("C#");
            CompilerParameters cp = new CompilerParameters();
            AddReferences(ref cp);
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;

            // 编译到内存，并创建实例
            CompilerResults cr = cdp.CompileAssemblyFromSource(cp, code.ToString());
            if (cr.Errors.HasErrors)
            {
                StringBuilder error = new StringBuilder();
                error.Append("Error Compiling Expression: ");
                foreach (CompilerError err in cr.Errors)
                    error.AppendFormat("{0}\n", err.ErrorText);
                throw new Exception("Error Compiling Expression: " + error.ToString());
            }
            Assembly a = cr.CompiledAssembly;
            _instance = a.CreateInstance("MyNameSpace._Evaluator");
        }

        void AddUsings(ref StringBuilder code)
        {
            code.Append("using System; \n");
            code.Append("using System.Data; \n");
            code.Append("using System.Data.SqlClient; \n");
            code.Append("using System.Data.OleDb; \n");
            code.Append("using System.Xml; \n");
            foreach (string txt in Usings)
                code.AppendLine(txt);
        }

        void AddReferences(ref CompilerParameters cp)
        {
            cp.ReferencedAssemblies.Add("system.dll");
            cp.ReferencedAssemblies.Add("system.data.dll");
            cp.ReferencedAssemblies.Add("system.xml.dll");
        }

        //---------------------------------------
        // Public Members
        //---------------------------------------
        public object GetValue(string methodName)
        {
            MethodInfo mi = _instance.GetType().GetMethod(methodName);
            return mi.Invoke(_instance, null);
        }

        public override object Eval(string expression)
        {
            EvaluatorItem[] items = { new EvaluatorItem(typeof(object), expression, _defaultMethodName) };
            ConstructEvaluator(items);
            return GetValue(_defaultMethodName);
        }

    }
}

