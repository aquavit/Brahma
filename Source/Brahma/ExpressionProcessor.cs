using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Brahma
{
    public abstract class ExpressionProcessor: ExpressionVisitor
    {
        public struct Parameter
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

        public struct Let
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

        public struct Result
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

        protected ExpressionProcessor(Kernel kernel, LambdaExpression lambda)
        {
            Flattened = lambda.Flatten().ToArray();
            Closures = Flattened.Closures().ToArray();

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

            Kernel = kernel;

            Kernel.Parameters = lambda.Parameters.ToArray(); // TODO: Remove if not required
            Kernel.Closures = Closures;
        }

        public Kernel Kernel
        {
            get;
            private set;
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
    }
}
