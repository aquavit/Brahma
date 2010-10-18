using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

using Brahma;

namespace Brahma.OpenCL
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    internal sealed class CodeGeneratorAttribute : Attribute
    {
        private readonly string _method = string.Empty;

        public CodeGeneratorAttribute(string method)
        {
            _method = method;
        }

        public string Method
        {
            get
            {
                return _method; 
            }
        }

        public Type[] Signature
        {
            get;
            set;
        }
    }

    internal delegate void CodeGenerator(CLCodeGenerator.Visitor visitor, MethodCallExpression method, ICLKernel kernel);
    
    public static class Loop
    {
        internal static CodeGenerator ForGenerator = (v, e, k) => 
        {
            // Select into the range variable of the enclosing "Select" (TODO: Handle SelectMany later!)
            string loopVar = (v.ExpressionStack.Pop() as ParameterExpression).Name;
                // k.NameGenerator.NewVarName();
            k.Source.AppendLine(string.Format("for (int {0} = {1}({4}); {2}({0}, {4}); {3}({0}, {4}))",
                loopVar, 
                (from l in v.NamedLambdas where l.Value == e.Arguments[0] select l.Key).First(),
                (from l in v.NamedLambdas where l.Value == e.Arguments[1] select l.Key).First(),
                (from l in v.NamedLambdas where l.Value == e.Arguments[2] select l.Key).First(),
                (from c in v.Closures from f in c.GetFields() select f.Name).Join(", ")));
        };
        [KernelUsable(CodeGenerator = "ForGenerator")]
        public static IEnumerable<int> For(Func<int> startValue, Func<int, bool> condition, Func<int, int> increment)
        {
            for (int i = startValue(); condition(i); i = increment(i))
                yield return i;
        }

        public static IEnumerable<bool> While(Func<bool> condition)
        {
            bool conditionValue = condition();
            while (conditionValue)
            {
                conditionValue = condition();
                yield return conditionValue;
            }
        }
    }
}