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

namespace Brahma.OpenGL
{
    // This is simply a sealed wrapper. We don't have anything extra to do, except expose certain protected members
    // Also, we're internal. We don't need to be visible to the world.
    internal sealed class ExpressionProcessor: ExpressionProcessorBase
    {
        public ExpressionProcessor(Expression expression)
            : base(expression)
        {
        }

        public new IEnumerable<Expression> FlattenedExpression
        {
            get
            {
                return base.FlattenedExpression;
            }
        }

        public new IEnumerable<LambdaExpression> Lambdas
        {
            get
            {
                return base.Lambdas;
            }
        }

        public new IEnumerable<Expression> FlattenedLambdas
        {
            get
            {
                return base.FlattenedLambdas;
            }
        }

        public new IEnumerable<ParameterExpression> TransparentIdentifiers
        {
            get
            {
                return base.TransparentIdentifiers;
            }
        }

        public new IEnumerable<NewExpression> NewAnonymousTypes
        {
            get
            {
                return base.NewAnonymousTypes;
            }
        }

        public new bool IsSelectWithoutLets
        {
            get
            {
                return base.IsSelectWithoutLets;
            }
        }

        public new List<ParameterExpression> KernelParameters
        {
            get
            {
                return base.KernelParameters;
            }
        }

        public new List<ParameterExpression> QueryParameters
        {
            get
            {
                return base.QueryParameters;
            }
        }

        public new IEnumerable<MemberExpression> MemberAccess
        {
            get
            {
                return base.MemberAccess;
            }
        }

        public new LambdaExpression MainLambda
        {
            get
            {
                return base.MainLambda;
            }
        }
    }
}