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

        internal sealed class Visitor : ExpressionVisitor
        {
            private readonly ICLKernel _kernel;
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
                    
                    _kernel.Source.Append("(");
                    _kernel.Source.Append(Translator<Type>.Translate(this, unary.Type));
                    _kernel.Source.Append(")(");
                    Visit(unary.Operand);
                    _kernel.Source.Append(")");
                }
                
                return unary;
            }

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                _kernel.Source.Append("("); // Always add braces
                Visit(binaryExpression.Left);

                switch (binaryExpression.NodeType)
                {
                    case ExpressionType.Add:
                        _kernel.Source.Append(" + ");
                        break;

                    case ExpressionType.Subtract:
                        _kernel.Source.Append(" - ");
                        break;

                    case ExpressionType.Multiply:
                        _kernel.Source.Append(" * ");
                        break;

                    case ExpressionType.Divide:
                        _kernel.Source.Append(" / ");
                        break;

                    case ExpressionType.GreaterThan:
                        _kernel.Source.Append(" > ");
                        break;

                    case ExpressionType.LessThan:
                        _kernel.Source.Append(" < ");
                        break;

                    case ExpressionType.GreaterThanOrEqual:
                        _kernel.Source.Append(" >= ");
                        break;

                    case ExpressionType.LessThanOrEqual:
                        _kernel.Source.Append(" <= ");
                        break;

                    case ExpressionType.Equal:
                        _kernel.Source.Append(" == ");
                        break;

                    case ExpressionType.NotEqual:
                        _kernel.Source.Append(" != ");
                        break;

                    case ExpressionType.Modulo:
                        _kernel.Source.Append(" % ");
                        break;

                    case ExpressionType.OrElse:
                        _kernel.Source.Append(" || ");
                        break;

                    case ExpressionType.AndAlso:
                        _kernel.Source.Append(" && ");
                        break;

                    default:
                        throw new NotSupportedException(string.Format("Cannot use the operator '{0}' in queries at this time",
                                                                      binaryExpression.NodeType));
                }

                Visit(binaryExpression.Right);
                _kernel.Source.Append(")");

                return binaryExpression;
            }

            protected override Expression VisitMemberAccess(MemberExpression member)
            {
                if (member.Type.Implements(typeof(INDRangeDimension)))
                    return member;

                if (member.Member.DeclaringType.Implements(typeof(IMem)))
                {
                    _kernel.Source.Append(Translator<MemberExpression>.Translate(this, member));
                    Visit(member.Expression);
                    _kernel.Source.Append("." + member.Member.Name);
                    return member;
                }
                
                if (member.Member.DeclaringType.IsAnonymous())
                {
                    _kernel.Source.Append(member.Member.Name);
                    return member;
                }
                
                if (_kernel.Closures.Contains(member, _memberExpressionComparer))
                {
                    _kernel.Source.Append(member.Member.Name);
                    return member;
                }

                _kernel.Source.Append(Translator<MemberExpression>.Translate(this, member));
                Visit(member.Expression);
                
                return member;
            }

            protected override Expression VisitParameter(ParameterExpression parameter)
            {
                if (parameter.Type.Implements(typeof(INDRangeDimension)))
                    return parameter;    
                    
                _kernel.Source.Append(parameter.Name);
                
                return parameter;
            }

            protected override Expression VisitConstant(ConstantExpression constant)
            {
                _kernel.Source.Append(constant.Value.ToString());
                
                return base.VisitConstant(constant);
            }

            protected override Expression VisitMethodCall(MethodCallExpression method)
            {
                if (method.Method.Name == "get_Item")
                {
                    Visit(method.Object);
                    _kernel.Source.Append("[");
                    for (int i = 0; i < method.Arguments.Count; i++)
                    {
                        Visit(method.Arguments[i]);
                        if (i < method.Arguments.Count - 1)
                            _kernel.Source.Append(", ");
                    }
                    _kernel.Source.Append("]");
                    
                    return method;
                }

                return method;
            }

            protected override Expression VisitConditional(ConditionalExpression conditional)
            {
                _kernel.Source.Append("(");
                _kernel.Source.Append("(");
                Visit(conditional.Test);
                _kernel.Source.Append(") ? ");
                Visit(conditional.IfTrue);
                _kernel.Source.Append(" : ");
                Visit(conditional.IfFalse);
                _kernel.Source.Append(")");

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

                _kernel.Source.Append(string.Format("make_{0}(", newExpression.Type.Name));
                for (int i = 0; i < newExpression.Arguments.Count; i++)
                {
                    Visit(newExpression.Arguments[i]);
                    if (i < newExpression.Arguments.Count - 1)
                        _kernel.Source.Append(", ");
                }
                _kernel.Source.Append(")");

                return newExpression;
            }

            internal struct Parameter
            {
                public ParameterExpression RangeVariable
                {
                    get;
                    set;
                }

                public Expression Sequence
                {
                    get;
                    set;
                }
            }

            internal struct Let
            {
                public MemberInfo Member
                {
                    get;
                    set;
                }

                public Expression Value
                {
                    get;
                    set;
                }
            }

            internal struct Result
            {
                public Expression Lhs
                {
                    get;
                    set;
                }

                public Expression Rhs
                {
                    get;
                    set;
                }
            }
            
            public Visitor(ICLKernel kernel, LambdaExpression lambda)
            {
                _kernel = kernel;

                Flattened = lambda.Flatten().ToArray();
                Closures = Flattened.Closures().ToArray();

                _kernel.Parameters = lambda.Parameters.ToArray(); // TODO: Remove if not required
                _kernel.Closures = Closures;

                Parameters = from method in
                                 (from exp in Flattened
                                  let selectMethod = exp as MethodCallExpression
                                  where selectMethod != null &&
                                  selectMethod.Method.Name == "Select"
                                  select selectMethod)
                             let selector = method.Arguments[1] as LambdaExpression
                             let parameter = selector.Parameters[0]
                             let sequence = method.Arguments[0]
                             where !parameter.IsTransparentIdentifier()
                             select new Parameter
                             {
                                 RangeVariable = parameter,
                                 Sequence = sequence
                             };

                Lets = from newExp in
                           (from expression in Flattened
                            let newExp = expression as NewExpression
                            where newExp != null && newExp.Type.IsAnonymous()
                            select newExp)
                       from idx in Enumerable.Range(0, newExp.Arguments.Count)
                       let memberName = newExp.Members[idx].Name
                       let param = (newExp.Arguments[0] as ParameterExpression)
                       let paramName = param != null ? param.Name : string.Empty
                       where (memberName != paramName) &&
                       !newExp.Arguments[idx].Type.IsAnonymous()
                       select new Let
                       {
                           Member = newExp.Members[idx],
                           Value = newExp.Arguments[idx]
                       };

                Results = from newExp in
                              (from expression in Flattened
                               where expression != null &&
                               expression.NodeType == ExpressionType.NewArrayInit &&
                               typeof(Set[]).IsAssignableFrom(expression.Type)
                               select expression as NewArrayExpression)
                          from expression in newExp.Expressions
                          let binExp = expression as BinaryExpression
                          select new Result
                          {
                              Lhs = binExp.Left,
                              Rhs = binExp.Right
                          };
            }

            new public Expression Visit(Expression expression)
            {
                return base.Visit(expression);
            }

            public ICLKernel Kernel
            {
                get
                {
                    return _kernel;
                }
            }

            public IEnumerable<Expression> Flattened
            {
                get;
                private set;
            }

            public IEnumerable<MemberExpression> Closures
            {
                get;
                private set;
            }

            public IEnumerable<Parameter> Parameters
            {
                get;
                private set;
            }

            public IEnumerable<Let> Lets
            {
                get;
                private set;
            }

            public IEnumerable<Result> Results
            {
                get;
                private set;
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
            Translator<Type>.Register(new Dictionary<Func<Type, bool>, Func<CLCodeGenerator.Visitor, Type, string>>
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
                        throw new NotSupportedException(string.Format("Cannot use type {0} inside a kernel, type should be a struct and implement IMem"));
                    //MemberInfo[] members = t.GetMembers();
                    //if ((from member in members
                    //     where member.MemberType != MemberTypes.Field
                    //     select true).Count() != 0)
                    //    throw new NotSupportedException(string.Format("{0} cannot be used in a kernel! Types used inside a kernel must contain only public fields",
                    //        t));

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

            Translator<MemberExpression>.Register(new Dictionary<Func<MemberExpression, bool>, Func<CLCodeGenerator.Visitor, MemberExpression, string>>
            {
                { 
                    m => m.Member.DeclaringType == typeof(_1D.IDs_1D), 
                    (v, m) => 
                    {
                        v.Stack.Push(m.Member.Name == "x" ? "0" : CLCodeGenerator.Error);
                        return string.Empty;
                    }
                },
                {
                    m => m.Member.DeclaringType == typeof(_2D.IDs_2D), 
                    (v, m) => 
                    {
                        v.Stack.Push(m.Member.Name == "x" ? "0" : 
                            m.Member.Name == "y" ? "1" : 
                            CLCodeGenerator.Error);
                        
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
                            CLCodeGenerator.Error);
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
            var visitor = new Visitor(kernel, lambda);

            kernel.Source.Clear();

            var closures = from closure in visitor.Closures
                           select string.Format("{0} {1}", 
                           Translator<Type>.Translate(visitor, closure.Type), closure.Member.Name);

            kernel.Source.Append("__kernel void main(");
            kernel.Source.Append((from parameter in lambda.Parameters
                                  where !parameter.Type.DerivesFrom(typeof(NDRange))
                                  select string.Format("{0} {1}", Translator<Type>.Translate(visitor, parameter.Type), parameter.Name)).Join(", ", 
                                  noTrailingSeparator: closures.Count() == 0));

            kernel.Source.Append(closures.Join(", "));

            kernel.Source.AppendLine(")");
            kernel.Source.AppendLine("{");

            // Write "let"s
            foreach (Visitor.Let let in visitor.Lets)
            {
                Type memberType = let.Member.MemberType == MemberTypes.Property ?
                    (let.Member as PropertyInfo).PropertyType :
                        let.Member.MemberType == MemberTypes.Field ?
                            (let.Member as FieldInfo).FieldType :
                            null;
                
                kernel.Source.Append(string.Format("{0} {1} = ", Translator<Type>.Translate(visitor, memberType), let.Member.Name));
                visitor.Visit(let.Value);
                kernel.Source.AppendLine(";");
            }

            // Write results
            foreach (Visitor.Result result in visitor.Results)
            {
                visitor.Visit(result.Lhs);
                kernel.Source.Append(" = ");
                visitor.Visit(result.Rhs);
                kernel.Source.AppendLine(";");
            }

            kernel.Source.AppendLine("}");
            kernel.Source.Insert(0, "#pragma OPENCL EXTENSION cl_khr_byte_addressable_store: enable\n\n"); // Always (for now)
        }
    }
}