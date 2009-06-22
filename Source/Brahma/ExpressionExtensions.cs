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

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Brahma
{
    public static class ExpressionExtensions
    {
        // This class derives from ExpressionVisitor and flattens the tree by visiting each node and adding them to a "flattened" list

        // Extension methods to flatten an expression
        public static IEnumerable<Expression> Flatten(this Expression expression)
        {
            return TreeFlattener.Flatten(expression);
        }

        // Extension methods to flatten a bunch of expressions. Comes in handy for subqueries
        public static IEnumerable<Expression> Flatten(this IEnumerable<Expression> expressions)
        {
            var flattenedExpressions = new List<Expression>();

            foreach (Expression expression in expressions)
                flattenedExpressions.AddRange(expression.Flatten());

            return flattenedExpressions;
        }

        // Extension methods to flatten a bunch of lambdas
        public static IEnumerable<Expression> Flatten(this IEnumerable<LambdaExpression> expressions)
        {
            var flattenedExpressions = new List<Expression>();

            foreach (LambdaExpression expression in expressions)
                flattenedExpressions.AddRange(expression.Flatten());

            return flattenedExpressions;
        }

        private sealed class TreeFlattener: ExpressionVisitor
        {
            // A list to hold the flattened expression tree
            private readonly List<Expression> _flattened = new List<Expression>();

            // No one can create an instance of this class
            private TreeFlattener()
            {
            }

            // Override the main Visit method, since we don't care what what node we see, we need them all
            protected override Expression Visit(Expression exp)
            {
                if (!_flattened.Contains(exp))
                    _flattened.Add(exp); // Add to a list

                return base.Visit(exp); // Call base to process the rest of the tree
            }

            // Static method to flatten an expression since no one can make an instance of this class
            public static IEnumerable<Expression> Flatten(Expression expression)
            {
                var f = new TreeFlattener();
                f.Visit(expression); // Start flattening
                return f._flattened; // Return the "flattened" list
            }
        }
    }
}