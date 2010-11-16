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
        
        public const string Error = "__error__";
        public const string KernelName = "main";

        internal sealed class ExpressionProcessor : Brahma.ExpressionProcessor
        {
            private readonly Stack<object> _stack = new Stack<object>();
            private readonly List<Type> _declaredTypes = new List<Type>();

            protected override Expression VisitUnary(UnaryExpression unary)
            {
                if (unary.NodeType == ExpressionType.Convert)
                {
                    if (unary.Type.IsConcreteGenericOf(typeof(Set<>)))
                    {
                        return unary;
                    }
                    
                    Kernel.Source.Append("(");
                    Kernel.Source.Append(Translator<Type>.Translate(this, unary.Type));
                    Kernel.Source.Append(")(");
                    Visit(unary.Operand);
                    Kernel.Source.Append(")");
                }
                
                return unary;
            }

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                Kernel.Source.Append("("); // Always add braces
                Visit(binaryExpression.Left);

                switch (binaryExpression.NodeType)
                {
                    case ExpressionType.Add:
                        Kernel.Source.Append(" + ");
                        break;

                    case ExpressionType.Subtract:
                        Kernel.Source.Append(" - ");
                        break;

                    case ExpressionType.Multiply:
                        Kernel.Source.Append(" * ");
                        break;

                    case ExpressionType.Divide:
                        Kernel.Source.Append(" / ");
                        break;

                    case ExpressionType.GreaterThan:
                        Kernel.Source.Append(" > ");
                        break;

                    case ExpressionType.LessThan:
                        Kernel.Source.Append(" < ");
                        break;

                    case ExpressionType.GreaterThanOrEqual:
                        Kernel.Source.Append(" >= ");
                        break;

                    case ExpressionType.LessThanOrEqual:
                        Kernel.Source.Append(" <= ");
                        break;

                    case ExpressionType.Equal:
                        Kernel.Source.Append(" == ");
                        break;

                    case ExpressionType.NotEqual:
                        Kernel.Source.Append(" != ");
                        break;

                    case ExpressionType.Modulo:
                        Kernel.Source.Append(" % ");
                        break;

                    case ExpressionType.OrElse:
                        Kernel.Source.Append(" || ");
                        break;

                    case ExpressionType.AndAlso:
                        Kernel.Source.Append(" && ");
                        break;

                    default:
                        throw new NotSupportedException(string.Format("Cannot use the operator '{0}' in queries at this time",
                                                                      binaryExpression.NodeType));
                }

                Visit(binaryExpression.Right);
                Kernel.Source.Append(")");

                return binaryExpression;
            }

            protected override Expression VisitMemberAccess(MemberExpression member)
            {
                if (member.Type.Implements(typeof(INDRangeDimension)))
                    return member;

                if (member.Member.DeclaringType.Implements(typeof(IMem)))
                {
                    Kernel.Source.Append(Translator<MemberExpression>.Translate(this, member));
                    Visit(member.Expression);
                    Kernel.Source.Append("." + member.Member.Name);
                    return member;
                }
                
                if (member.Member.DeclaringType.IsAnonymous())
                {
                    Kernel.Source.Append(member.Member.Name);
                    return member;
                }
                
                if (Kernel.Closures.Contains(member, _memberExpressionComparer))
                {
                    Kernel.Source.Append(member.Member.Name);
                    return member;
                }

                Kernel.Source.Append(Translator<MemberExpression>.Translate(this, member));
                Visit(member.Expression);
                
                return member;
            }

            protected override Expression VisitParameter(ParameterExpression parameter)
            {
                if (parameter.Type.Implements(typeof(INDRangeDimension)))
                    return parameter;    
                    
                Kernel.Source.Append(parameter.Name);
                
                return parameter;
            }

            protected override Expression VisitConstant(ConstantExpression constant)
            {
                Kernel.Source.Append(constant.Value.ToString());
                
                return base.VisitConstant(constant);
            }

            protected override Expression VisitMethodCall(MethodCallExpression method)
            {
                if (method.Method.Name == "get_Item")
                {
                    Visit(method.Object);
                    Kernel.Source.Append("[");
                    for (int i = 0; i < method.Arguments.Count; i++)
                    {
                        Visit(method.Arguments[i]);
                        if (i < method.Arguments.Count - 1)
                            Kernel.Source.Append(", ");
                    }
                    Kernel.Source.Append("]");
                    
                    return method;
                }

                return method;
            }

            protected override Expression VisitConditional(ConditionalExpression conditional)
            {
                Kernel.Source.Append("(");
                Kernel.Source.Append("(");
                Visit(conditional.Test);
                Kernel.Source.Append(") ? ");
                Visit(conditional.IfTrue);
                Kernel.Source.Append(" : ");
                Visit(conditional.IfFalse);
                Kernel.Source.Append(")");

                return conditional;
            }

            protected override NewExpression VisitNew(NewExpression newExpression)
            {
                if (!_declaredTypes.Contains(newExpression.Type))
                    throw new NotSupportedException(string.Format("Cannot new {0} inside a kernel!", newExpression.Type.Name));

                if (newExpression.Arguments.Count == 0)
                {
                    // Initialize everything to default(T). How?
                }

                Kernel.Source.Append(string.Format("make_{0}(", newExpression.Type.Name));
                for (int i = 0; i < newExpression.Arguments.Count; i++)
                {
                    Visit(newExpression.Arguments[i]);
                    if (i < newExpression.Arguments.Count - 1)
                        Kernel.Source.Append(", ");
                }
                Kernel.Source.Append(")");

                return newExpression;
            }

            public ExpressionProcessor(ICLKernel kernel, LambdaExpression lambda)
                : base(kernel as Kernel, lambda)
            {
            }

            new public Expression Visit(Expression expression)
            {
                return base.Visit(expression);
            }

            new public ICLKernel Kernel
            {
                get
                {
                    return base.Kernel as ICLKernel;
                }
            }

            public Stack<object> Stack
            {
                get
                {
                    return _stack;
                }
            }

            public List<Type> DeclaredTypes
            {
                get
                {
                    return _declaredTypes;
                }
            }
        }

        static CLCodeGenerator()
        {
            // Initialize conversions
            Translator<Type>.Register(new Dictionary<Func<Type, bool>, Func<ExpressionProcessor, Type, string>>
            {
                { t => t == typeof(byte), (v, t) => "uchar" },
                { t => t == typeof(int), (v, t) => "int" },
                { t => t == typeof(float), (v, t) => "float" },
                { t => t == typeof(int32), (v, t) => "int" },
                { t => t == typeof(single), (v, t) => "float" },
                { t => t == typeof(Buffer<int32>), (v, t) => "global int*" },
                { t => t == typeof(Buffer<single>), (v, t) => "global float*" },
                { t => t.IsConcreteGenericOf(typeof(Buffer<>)), (v, t) => string.Format("global {0}*", Translator<Type>.Default(v, t.GetGenericArguments()[0])) }
            });

            Translator<Type>.RegisterDefault(
                (v, t) =>
                {
                    if (v.DeclaredTypes.Contains(t))
                        return t.Name;

                    if (!t.IsValueType && !t.IsPrimitive && !t.IsEnum && t.Implements(typeof(IMem)))
                        throw new NotSupportedException(string.Format("Cannot use type \"{0}\" inside a kernel, type should be a struct and implement IMem", t.FullName));

                    var newType = new StringBuilder();
                    newType.AppendLine("typedef struct __attribute__ ((__packed__)) {");
                    FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var field in fields)
                        newType.AppendLine(string.Format("{0} {1};", Translator<Type>.Translate(v, field.FieldType), field.Name));

                    newType.AppendLine(string.Format("}} {0};", t.Name));
                    
                    // Make_XXX function
                    newType.AppendLine(string.Format("{0} make_{0}({1}) {{",
                        t.Name,
                        (from field in fields
                         let fieldType = Translator<Type>.Translate(v, field.FieldType)
                         select string.Format("{0} _{1}", fieldType, field.Name)).Join(", ")));
                    newType.AppendLine(string.Format("{0} result;", t.Name));
                    newType.AppendLine((from field in fields
                                        select string.Format("result.{0} = _{0};", field.Name)).Join("\n", false));
                    newType.AppendLine("return result;");
                    newType.AppendLine("}");


                    v.Kernel.Source.Insert(0, newType.ToString());

                    v.DeclaredTypes.Add(t);

                    return t.Name;
                });

            Translator<MemberExpression>.Register(new Dictionary<Func<MemberExpression, bool>, Func<ExpressionProcessor, MemberExpression, string>>
            {
                { 
                    m => m.Member.DeclaringType == typeof(_1D.IDs_1D), 
                    (v, m) => 
                    {
                        v.Stack.Push(m.Member.Name == "x" ? "0" : Error);
                        return string.Empty;
                    }
                },
                {
                    m => m.Member.DeclaringType == typeof(_2D.IDs_2D), 
                    (v, m) => 
                    {
                        v.Stack.Push(m.Member.Name == "x" ? "0" : 
                            m.Member.Name == "y" ? "1" : 
                            Error);
                        
                        return string.Empty;
                    }
                },
                {
                    m => m.Member.DeclaringType == typeof(_3D.IDs_3D), 
                    (v, m) => 
                    {
                        v.Stack.Push(m.Member.Name == "x" ? "0" :
                            m.Member.Name == "y" ? "1" :
                            m.Member.Name == "z" ? "2" :
                            Error);
                        return string.Empty;
                    }
                },
                {
                    m => m.Member.Name == "GlobalIDs",
                    (v, m) => 
                    {
                        return "get_global_id(" + v.Stack.Pop() + ")";
                    }
                },
                {
                    m => m.Member.Name == "LocalIDs",
                    (v, m) => 
                    {
                        return "get_local_id(" + v.Stack.Pop() + ")";
                    }
                }
            });
        }
        
        public static void GenerateKernel(this LambdaExpression lambda, ICLKernel kernel)
        {
            var expressionProcessor = new ExpressionProcessor(kernel, lambda);

            kernel.Source.Clear();

            var closures = from closure in expressionProcessor.Closures
                           select string.Format("{0} {1}", 
                           Translator<Type>.Translate(expressionProcessor, closure.Type), closure.Member.Name);

            kernel.Source.Append("__kernel void main(");
            kernel.Source.Append((from parameter in lambda.Parameters
                                  where !parameter.Type.DerivesFrom(typeof(NDRange))
                                  select string.Format("{0} {1}", Translator<Type>.Translate(expressionProcessor, parameter.Type), parameter.Name)).Join(", ", 
                                  noTrailingSeparator: closures.Count() == 0));

            kernel.Source.Append(closures.Join(", "));

            kernel.Source.AppendLine(")");
            kernel.Source.AppendLine("{");

            // Write "let"s
            foreach (var let in expressionProcessor.Lets)
            {
                Type memberType = let.Member.MemberType == MemberTypes.Property ?
                    (let.Member as PropertyInfo).PropertyType :
                        let.Member.MemberType == MemberTypes.Field ?
                            (let.Member as FieldInfo).FieldType :
                            null;
                
                kernel.Source.Append(string.Format("{0} {1} = ", Translator<Type>.Translate(expressionProcessor, memberType), let.Member.Name));
                expressionProcessor.Visit(let.Value);
                kernel.Source.AppendLine(";");
            }

            // Write results
            foreach (var result in expressionProcessor.Results)
            {
                expressionProcessor.Visit(result.Lhs);
                kernel.Source.Append(" = ");
                expressionProcessor.Visit(result.Rhs);
                kernel.Source.AppendLine(";");
            }

            kernel.Source.AppendLine("}");
            kernel.Source.Insert(0, "#pragma OPENCL EXTENSION cl_khr_byte_addressable_store: enable\n\n"); // Always (for now)
        }
    }
}