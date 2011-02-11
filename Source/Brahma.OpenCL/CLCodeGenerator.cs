#region License and Copyright Notice
// Copyright (c) 2010 Ananth B.
// All rights reserved.
// 
// The contents of this file are made available under the terms of the
// Eclipse Public License v1.0 (the "License") which accompanies this
// distribution, and is available at the following URL:
// http://www.opensource.org/licenses/eclipse-1.0.php
// 
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
// the specific language governing rights and limitations under the License.
// 
// By using this software in any fashion, you are agreeing to be bound by the
// terms of the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Brahma.Types;

namespace Brahma.OpenCL
{
    internal static class CLCodeGenerator
    {
        private static readonly ExpressionExtensions.MemberExpressionComparer _memberExpressionComparer =
            new ExpressionExtensions.MemberExpressionComparer();
        
        public const string KernelName = "brahmaKernel";

        private sealed class CodeGenerator : ExpressionVisitor
        {
            private readonly ComputeProvider _provider;
            private readonly LambdaExpression _lambda;

            private readonly List<MemberExpression> _closures = new List<MemberExpression>();
            private readonly StringBuilder _code = new StringBuilder();

            private readonly List<string> _declaredMembers = new List<string>();

            private static string TranslateType(Type type)
            {
                if ((type == typeof(int32)) || (type == typeof(int)))
                    return "int";
                if ((type == typeof(uint)))
                    return "uint";
                if ((type == typeof(float32)) || (type == typeof(float)))
                    return "float";
                
                if ((type.IsConcreteGenericOf(typeof(Brahma.Buffer<>))) ||
                    (type.IsConcreteGenericOf(typeof(Buffer<>))))
                {
                    string genericParameterType = TranslateType(type.GetGenericArguments()[0]);
                    return "__global " + genericParameterType + "*"; // TODO: Take into account different kinds of variables later
                }

                throw new InvalidOperationException(string.Format("Cannot use the type {0} inside a kernel", type));
            }

            private static void UnwindMemberAccess(Expression expression, StringBuilder builder)
            {
                if ((expression == null) || (expression.NodeType == ExpressionType.Constant))
                    return;

                if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    var member = expression as MemberExpression;
                    if (!(member.Member is FieldInfo))
                        throw new InvalidOperationException("Cannot access methods/properties inside a kernel!");

                    builder.Append(member.Member.Name);

                    UnwindMemberAccess(member.Expression, builder);
                }
            }

            private static ParameterExpression GetLoopVar(LambdaExpression expression)
            {
                ParameterExpression loopRange = expression.Parameters[0];
                var call = expression.Body as MethodCallExpression;
                do
                {
                    if (call.Arguments[0] == loopRange)
                        return ((LambdaExpression)call.Arguments[1]).Parameters[0];

                    call = call.Arguments[0] as MethodCallExpression;
                } while (call != null);

                // TODO: Will this ever happen?
                throw new InvalidExpressionTreeException("Don't know how to get the loop variable from the given expression", expression);
            }

            private static string GetClosureAccessName(MemberExpression member)
            {
                var result = new StringBuilder();
                UnwindMemberAccess(member, result);

                return result.ToString();
            }

            private string NativeMethodPrefix
            {
                get 
                {
                   return (_provider.CompileOptions & CompileOptions.UseNativeFunctions) == CompileOptions.UseNativeFunctions ? "native_" : string.Empty;
                }
            }

            protected override Expression VisitConstant(ConstantExpression constant)
            {
                _code.Append("((" + TranslateType(constant.Type) + ")");
                _code.Append(constant + ")");

                return constant;
            }

            protected override Expression VisitMemberAccess(MemberExpression member)
            {
                switch (member.Member.Name)
                {
                    case "GlobalID0":
                        _code.Append("get_global_id(0)");
                        return member;
                    case "GlobalID1":
                        _code.Append("get_global_id(1)");
                        return member;
                    case "GlobalID2":
                        _code.Append("get_global_id(2)");
                        return member;
                    case "LocalID0":
                        _code.Append("get_local_id(0)");
                        return member;
                    case "LocalID1":
                        _code.Append("get_local_id(1)");
                        return member;
                    case "LocalID2":
                        _code.Append("get_local_id(2)");
                        return member;
                }

                if (member.Member.DeclaringType.IsAnonymous())
                {
                    _code.Append(member.Member.Name);

                    return member;
                }

                if (member.IsClosureAccess() || member.IsConstantAccess())
                {
                    _code.Append(GetClosureAccessName(member));

                    if (!_closures.Contains(member, _memberExpressionComparer))
                        _closures.Add(member);

                    return member;
                }

                return member;
            }

            protected override Expression VisitUnary(UnaryExpression unary)
            {
                if (unary.NodeType == ExpressionType.Convert)
                    Visit(unary.Operand);

                return unary;
            }

            protected override Expression VisitConditional(ConditionalExpression conditional)
            {
                _code.Append("(");
                Visit(conditional.Test);
                _code.Append(" ? ");
                Visit(conditional.IfTrue);
                _code.Append(" : ");
                Visit(conditional.IfFalse);
                
                _code.Append(")");
                
                return conditional;
            }

            protected override Expression VisitParameter(ParameterExpression parameter)
            {
                _code.Append(parameter.Name);

                return parameter;
            }

            protected override NewExpression VisitNew(NewExpression newExpression)
            {
                if (newExpression.Type.IsAnonymous())
                {
                    for (int i = 0; i < newExpression.Arguments.Count; i++)
                    {
                        if (newExpression.Members[i].Name == newExpression.Arguments[i].ToString())
                            continue; // Trivial assignment

                        // Loop
                        if ((newExpression.Arguments[i].Type == typeof(Func<int, IEnumerable<Set[]>>)) ||
                            (newExpression.Arguments[i].Type == typeof(Func<IEnumerable<int>, IEnumerable<Set[]>>)))
                        {
                            Visit(newExpression.Arguments[i]);
                            continue;
                        }

                        if (!_declaredMembers.Contains(newExpression.Members[i].Name))
                        {
                            _code.Append(TranslateType(((PropertyInfo) newExpression.Members[i]).PropertyType) + " ");
                            _declaredMembers.Add(newExpression.Members[i].Name);
                        }
                        _code.Append(newExpression.Members[i].Name + " = ");

                        Visit(newExpression.Arguments[i]);
                    }
                }

                return newExpression;
            }

            protected override Expression VisitBinary(BinaryExpression binary)
            {
                bool isResultAssignment = false; // TODO: Remove this and move it one level up
                
                _code.Append("(");
                Visit(binary.Left);

                switch (binary.NodeType)
                {
                    case ExpressionType.LessThanOrEqual:
                        if (binary.Type.IsConcreteGenericOf(typeof(Set<>)))
                        {
                            _code.Append(" = ");
                            isResultAssignment = true;
                        }
                        else
                            _code.Append(" <= ");
                        break;

                    case ExpressionType.Add:
                        _code.Append(" + ");
                        break;
                    case ExpressionType.Subtract:
                        _code.Append(" - ");
                        break;
                    case ExpressionType.Multiply:
                        _code.Append(" * ");
                        break;
                    case ExpressionType.Divide:
                        _code.Append(" / ");
                        break;
                    case ExpressionType.Modulo:
                        _code.Append(" % ");
                        break;

                    case ExpressionType.NotEqual:
                        _code.Append(" != ");
                        break;

                    case ExpressionType.Equal:
                        _code.Append(" == ");
                        break;

                    case ExpressionType.LessThan:
                        _code.Append(" < ");
                        break;
                    
                    case ExpressionType.GreaterThan:
                        _code.Append(" > ");
                        break;

                    case ExpressionType.And:
                        _code.Append(" & ");
                        break;
                    
                    case ExpressionType.Or:
                        _code.Append(" | ");
                        break;

                    case ExpressionType.LeftShift:
                        _code.Append(" << ");
                        break;

                    case ExpressionType.RightShift:
                        _code.Append(" >> ");
                        break;
                }

                Visit(binary.Right);
                _code.Append(")");

                if (isResultAssignment)
                    _code.Append(";");

                return binary;
            }

            protected override Expression VisitMethodCall(MethodCallExpression method)
            {
                switch (method.Method.Name)
                {
                    case "get_Item":
                        Visit(method.Object);
                        _code.Append("[");
                        Visit(method.Arguments[0]);
                        for (int i = 1; i < method.Arguments.Count; i++)
                        {
                            _code.Append(", ");
                            Visit(method.Arguments[i]);
                        }
                        _code.Append("]");

                        break;

                    case "Loop":
                        ParameterExpression loopVar = null;

                        // Figure out what kind of loop body this is
                        var loopBody = method.Arguments[2] as LambdaExpression;

                        if (loopBody.Parameters[0].Type == typeof(IEnumerable<int>))
                            loopVar = GetLoopVar(loopBody);
                        if (loopBody.Parameters[0].Type == typeof(int))
                            loopVar = loopBody.Parameters[0];

                        _code.Append(string.Format("for (int {0} = ", loopVar.Name));
                        Visit(method.Arguments[0]);
                        _code.Append(string.Format("; {0} < ", loopVar.Name));
                        Visit(method.Arguments[1]);
                        _code.AppendLine(string.Format("; {0}++) {{", loopVar.Name));

                        Visit(method.Arguments[2]);
                        _code.AppendLine(";");

                        _code.AppendLine("}");

                        break;

                    case "Select":
                        if (!(method.Arguments[0] is ParameterExpression))
                        {
                            Visit(method.Arguments[0]);
                        }
                        Visit(method.Arguments[1]);
                        _code.AppendLine(";");

                        break;
                    
                    case "Fabs":
                        _code.Append("fabs(");
                        Visit(method.Arguments[0]);
                        _code.Append(")");

                        break;

                    case "Log10":
                        _code.Append(string.Format("{0}log10(", NativeMethodPrefix));
                        Visit(method.Arguments[0]);
                        _code.Append(")");

                        break;

                    case "Log2":
                        _code.Append(string.Format("{0}log2(", NativeMethodPrefix));
                        Visit(method.Arguments[0]);
                        _code.Append(")");

                        break;

                    case "Powr":
                        _code.Append(string.Format("{0}powr(", NativeMethodPrefix));
                        Visit(method.Arguments[0]);
                        _code.Append(", ");
                        Visit(method.Arguments[1]);
                        _code.Append(")");

                        break;

                    case "Min":
                        _code.Append("min(");
                        Visit(method.Arguments[0]);
                        _code.Append(", ");
                        Visit(method.Arguments[1]);
                        _code.Append(")");

                        break;
                    
                    case "Max":
                        _code.Append("max(");
                        Visit(method.Arguments[0]);
                        _code.Append(", ");
                        Visit(method.Arguments[1]);
                        _code.Append(")");

                        break;

                    case "reinterpretAsFloat32":
                        _code.Append("as_float(");
                        Visit(method.Arguments[0]);
                        _code.Append(")");

                        break;
                }

                return method;
            }

            public CodeGenerator(ComputeProvider provider, LambdaExpression lambda)
            {
                _provider = provider;
                _lambda = lambda;
            }

            public string Generate()
            {
                Visit(_lambda.Body);

                var parameters = new StringBuilder();
                foreach (var parameter in _lambda.Parameters)
                {
                    if (parameter.Type.DerivesFrom(typeof(NDRange)))
                        continue;

                    parameters.Append(string.Format("{0} {1},", TranslateType(parameter.Type), parameter.Name));
                }

                foreach (var closure in _closures)
                    parameters.Append(string.Format("{0} {1},", TranslateType(closure.Type), GetClosureAccessName(closure)));

                if (parameters[parameters.Length - 1] == ',')
                    parameters.Remove(parameters.Length - 1, 1);

                var kernelSource = new StringBuilder();
                kernelSource.AppendLine(string.Format("__kernel void {0}({1}) {{", KernelName, parameters));
                kernelSource.Append(_code.ToString());
                kernelSource.AppendLine("}");

                return kernelSource.ToString();
            }

            public IEnumerable<MemberExpression> Closures
            {
                get { return _closures; }
            }
        }

        public static void GenerateKernel(this LambdaExpression lambda, ComputeProvider provider, ICLKernel kernel)
        {
            var codeGenerator = new CodeGenerator(provider, lambda);
            kernel.Source.Append(codeGenerator.Generate());
            kernel.Closures = codeGenerator.Closures;
            kernel.Parameters = lambda.Parameters;
        }
    }
}