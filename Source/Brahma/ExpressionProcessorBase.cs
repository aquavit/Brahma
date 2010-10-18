using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Brahma
{
    public abstract class ExpressionProcessorBase
    {
        private sealed class MemberExpressionComparer : IEqualityComparer<MemberExpression>
        {
            #region IEqualityComparer<MemberExpression> Members

            public bool Equals(MemberExpression x, MemberExpression y)
            {
                return x.ToString() == y.ToString();
            }

            public int GetHashCode(MemberExpression obj)
            {
                return obj.ToString().GetHashCode();
            }

            #endregion
        }

        private readonly LambdaExpression _expression;

        // This no longer calls Process()! Anyone using ExpressionProcessors have to explicitly call it.
        protected ExpressionProcessorBase(LambdaExpression expression)
        {
            _expression = expression;
        }

        #region Expressions and terms that derived classes use

        protected IEnumerable<Expression> FlattenedExpression
        {
            get;
            private set;
        }

        protected IEnumerable<LambdaExpression> Lambdas
        {
            get;
            private set;
        }

        protected IEnumerable<Expression> FlattenedLambdas
        {
            get;
            private set;
        }

        protected IEnumerable<ParameterExpression> TransparentIdentifiers
        {
            get;
            private set;
        }

        protected IEnumerable<NewExpression> NewAnonymousTypes
        {
            get;
            private set;
        }

        protected bool IsSelectWithoutLets
        {
            get;
            private set;
        }

        protected List<ParameterExpression> KernelParameters
        {
            get;
            private set;
        }

        protected List<ParameterExpression> QueryParameters
        {
            get;
            private set;
        }

        protected LambdaExpression MainLambda
        {
            get;
            private set;
        }

        protected IEnumerable<MemberExpression> MemberAccess
        {
            get;
            private set;
        }

        #endregion

        public LambdaExpression Expression
        {
            get
            {
                return _expression;
            }
        }

        public virtual void Process()
        {
            // This is the meat of Brahma, here we'll process the expression tree
            // and generate all the sub-expressions and terms we need to generate code.
            // We're going to use linq-over-objects to examine the expression tree and
            // find the terms we'll need.

            // Remember, this should ALWAYS be a LambdaExpression or this won't work. No more imperative queries anyway!
            // See how ComputationProviderBase.CreateQuery works if you want to understand more.

            // Generate the "flattened" terms that we'll be using often
            FlattenedExpression = _expression.Flatten(); // Flatten the expression tree
            Lambdas = from Expression node in FlattenedExpression
                      let lambda = node as LambdaExpression
                      where lambda != null
                      select lambda;
            FlattenedLambdas = Lambdas.Flatten(); // Flatten the lambdas

            // TODO: Check if any query terms are provider-specific "reserved" words (functions, too). Probably means an abstract method

            // Find all transparent identifiers. This is done so far ahead because isSelectWithoutLets needs it
            TransparentIdentifiers = (from Expression node in FlattenedLambdas
                                      let parameter = node as ParameterExpression
                                      where (parameter != null) &&
                                            parameter.IsTransparentIdentifier()
                                      select parameter).Distinct();

            // Is this a simple select without lets? If so, we have to handle the parameter selection differently
            IsSelectWithoutLets = ((from Expression node in FlattenedExpression
                                    let method = node as MethodCallExpression
                                    where (method != null) && (method.Method.Name.Equals("Select"))
                                    select method).Count() > 0) &&
                                  (TransparentIdentifiers.Count() == 0);

            if (IsSelectWithoutLets)
            {
                // Find the query parameter
                QueryParameters = new List<ParameterExpression>(from ParameterExpression parameter in ((LambdaExpression)_expression).Parameters // Get the query parameter
                                                                select parameter);
                if (QueryParameters.Count == 0) // Looks like we're doing a .Select on a result of something
                {
                    var method = _expression.Body;
                }

                KernelParameters = new List<ParameterExpression>(from Expression node in FlattenedExpression
                                                                 let parameter = node as ParameterExpression
                                                                 where (parameter != null) && !(QueryParameters.Contains(parameter)) && !parameter.IsTransparentIdentifier()
                                                                 select parameter);

                //// There can be zero or more kernel parameters, but there should be at least one query parameter. If there is even one kernel parameter,
                //// then it has to equal the number of query parameters
                //if (QueryParameters.Count() != 1)
                //    throw new InvalidExpressionTreeException("Found zero or more than one query parameter in a simple \"select\" without let statements.", _expression);

                NewAnonymousTypes = new NewExpression[] { }; // Empty, so we dont get NullReferenceExceptions
            }
            else
            {
                IEnumerable<ParameterExpression> primaryParameters = from Expression node in FlattenedExpression // Get all the primary parameters except the first one
                                                                     let lambda = node as LambdaExpression
                                                                     where (lambda != null) && (lambda.Body is ParameterExpression)
                                                                     select lambda.Body as ParameterExpression;

                IEnumerable<ParameterExpression> firstPrimaryParameter = from ParameterExpression parameter in ((LambdaExpression)_expression).Parameters // Get the first primary parameter
                                                                         where !new List<ParameterExpression>(primaryParameters).Contains(parameter)
                                                                         select parameter;

                QueryParameters = new List<ParameterExpression>(firstPrimaryParameter);
                QueryParameters.AddRange(primaryParameters); // We now have all the query parameters here, in order

                // Select the kernel parameters
                KernelParameters = new List<ParameterExpression>(from Expression node in FlattenedExpression
                                                                 let parameter = node as ParameterExpression
                                                                 where (parameter != null) && !(QueryParameters.Contains(parameter)) && !parameter.IsTransparentIdentifier()
                                                                 select parameter);
                // Now, between _queryParameters and _kernelParameters, we have the main lambda's parameters and the corresponding kernel parameters for each of them

                // Find all anonymous type constructors
                NewAnonymousTypes = from LambdaExpression lambda in Lambdas
                                    where (lambda.Body is NewExpression) && lambda.Body.Type.IsAnonymous()
                                    select lambda.Body as NewExpression;
            }

            // Find the "main" lambda
            IEnumerable<LambdaExpression> mainLambda = from LambdaExpression lambda in Lambdas
                                                       where lambda.Body.Type == ((LambdaExpression)_expression).Body.Type.GetGenericArguments()[0]
                                                       select lambda;
            // Did we find one, and just one?
            if (mainLambda.Count() != 1)
                throw new InvalidExpressionTreeException("Found zero or more than one main lambda in the given expression tree", _expression);

            // Get the first (and only) lambda we've got
            MainLambda = new List<LambdaExpression>(mainLambda)[0];

            // Find all member access happening that is not part of the query expression itself
            // These are values that dont change during execution of one query, but have to be assigned for each run
            MemberAccess = (from Expression node in FlattenedExpression
                            let memberExp = node as MemberExpression
                            where (memberExp != null) &&
                                  ((memberExp.Expression == null) || (memberExp.Expression is ConstantExpression))
                            select memberExp).Distinct(new MemberExpressionComparer());
            
            // That's all this method does. Derived classes can use the protected properties this class exposes to perform further processing.
        }
    }
}
