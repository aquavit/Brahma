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

using System.Collections.Generic;
using System.Linq.Expressions;

namespace Brahma
{
    public abstract class Kernel
    {
        protected internal IEnumerable<MemberExpression> Closures
        {
            get; 
            set; 
        }

        protected internal IEnumerable<ParameterExpression> Parameters
        {
            get; 
            set; 
        }
    }
    
    public abstract class Kernel<TRange, TResult>: Kernel 
        where TRange: struct, INDRangeDimension
    {
    }

    public abstract class Kernel<TRange, T, TResult>: Kernel 
        where T: IMem 
        where TRange: struct, INDRangeDimension
    {
    }

    public abstract class Kernel<TRange, T1, T2, TResult>: Kernel 
        where T1: IMem 
        where T2: IMem 
        where TRange: struct, INDRangeDimension
    {
    }

    public abstract class Kernel<TRange, T1, T2, T3, TResult>: Kernel
        where T1: IMem 
        where T2: IMem 
        where T3: IMem
        where TRange: struct, INDRangeDimension
    {
    }

    public abstract class Kernel<TRange, T1, T2, T3, T4, TResult> : Kernel
        where T1 : IMem
        where T2 : IMem
        where T3 : IMem
        where T4: IMem
        where TRange : struct, INDRangeDimension
    {
    }
}