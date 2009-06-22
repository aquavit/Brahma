#region License and Copyright Notice

//Brahma 2.0: Framework for streaming/parallel computing with an emphasis on GPGPU

//Copyright (c) 2007 Ananth B.
//All rights reserved.

//The contents of this file are made available under the terms of the
//Eclipse Public License v1.0 (the "License") which accompanies this
//distribution, and is available at the following URL:
//http://www.opensource.org/licenses/eclipse-1.0.php

//Software distributed under the License is distributed on an "AS IS" basis,
//WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
//the specific language governing rights and limitations under the License.

//By using this software in any fashion, you are agreeing to be bound by the
//terms of the License.

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

using Brahma.Helper;

namespace Brahma
{
    // Derive from this class and override Process()
    // This base class performs preliminary processing, additional processing
    // can be performed by sub-classes 
    public abstract class ExpressionProcessorBase
    {
        private readonly Expression _expression;

        // This no longer calls Process()! Anyone using ExpressionProcessors have to explicitly call it.
        protected ExpressionProcessorBase(Expression expression)
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

        public Expression Expression
        {
            get
            {
                return _expression;
            }
        }

        public virtual void Process()
        {
            // This is probably the meat of Brahma, here we'll process the expression tree
            // and generate all the sub-expressions and terms we need to generate code from.
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

                KernelParameters = new List<ParameterExpression>(from Expression node in FlattenedExpression
                                                                 let parameter = node as ParameterExpression
                                                                 where (parameter != null) && !(QueryParameters.Contains(parameter)) && !parameter.IsTransparentIdentifier()
                                                                 select parameter);

                // There can be zero or more kernel parameters, but there should be at least one query parameter. If there is even one kernel parameter,
                // then it has to equal the number of query parameters
                if (QueryParameters.Count() != 1)
                    throw new InvalidExpressionTreeException("Found zero or more than one query parameter in a simple \"select\" without let statements.", _expression);

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

            foreach (MemberExpression memberExp in MemberAccess)
                // It should be one of these types
                if (!(memberExp.Type == typeof(int) ||
                      memberExp.Type == typeof(float) ||
                      memberExp.Type == typeof(Vector2) ||
                      memberExp.Type == typeof(Vector3) ||
                      memberExp.Type == typeof(Vector4)))
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot access {0} of type {1}. You can only access members of type int, float, Vector2, Vector3 or Vector4",
                                                                      memberExp.Member, memberExp.Type));

            // That's all this method does. Derived classes can use the protected properties this class exposes to perform further processing.
        }
    }

    public static class ParameterExpressionExtensions
    {
        public static bool IsTransparentIdentifier(this ParameterExpression parameter)
        {
            return parameter.Name.StartsWith("<>h__TransparentIdentifier");
        }
    }

    public sealed class MemberExpressionComparer : IEqualityComparer<MemberExpression>
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
}