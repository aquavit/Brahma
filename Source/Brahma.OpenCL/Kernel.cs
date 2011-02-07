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
using System.Text;
using Brahma.OpenCL.Commands;
using OpenCL.Net;

namespace Brahma.OpenCL
{
    internal interface ICLKernel
    {
        StringBuilder Source
        {
            get;
        }

        IEnumerable<MemberExpression> Closures
        {
            get;
            set;
        }

        IEnumerable<ParameterExpression> Parameters
        {
            get;
            set;
        }

        Cl.Kernel ClKernel
        {
            get;
            set;
        }

        int WorkDim
        {
            get;
        }
    }

    public sealed class Kernel<TRange, TResult>: Brahma.Kernel<TRange, TResult>, ICLKernel
        where TRange: struct, Brahma.INDRangeDimension
    {
        private static readonly TRange _range = new TRange();

        private readonly StringBuilder _source = new StringBuilder();

        StringBuilder ICLKernel.Source
        {
            get 
            {
                return _source;
            }
        }

        IEnumerable<MemberExpression> ICLKernel.Closures
        {
            get { return Closures; }
            set { Closures = value; }
        }

        IEnumerable<ParameterExpression> ICLKernel.Parameters
        {
            get { return Parameters; }
            set { Parameters = value; }
        }

        Cl.Kernel ICLKernel.ClKernel
        {
            get;
            set;
        }

        int ICLKernel.WorkDim
        {
            get
            {
                return ((INDRangeDimension)_range).Dimensions;
            }
        }
    }

    public sealed class Kernel<TRange, T, TResult>: Brahma.Kernel<TRange, T, TResult>, ICLKernel 
        where TRange: struct, Brahma.INDRangeDimension
        where T: IMem
    {
        private static readonly TRange _range = new TRange();

        private readonly StringBuilder _source = new StringBuilder();

        StringBuilder ICLKernel.Source
        {
            get
            {
                return _source;
            }
        }

        IEnumerable<MemberExpression> ICLKernel.Closures
        {
            get
            {
                return Closures;
            }
            set
            {
                Closures = value;
            }
        }

        IEnumerable<ParameterExpression> ICLKernel.Parameters
        {
            get
            {
                return Parameters;
            }
            set
            {
                Parameters = value;
            }
        }

        Cl.Kernel ICLKernel.ClKernel
        {
            get;
            set;
        }

        int ICLKernel.WorkDim
        {
            get
            {
                return ((INDRangeDimension)_range).Dimensions;
            }
        }
    }

    public sealed class Kernel<TRange, T1, T2, TResult>: Brahma.Kernel<TRange, T1, T2, TResult>, ICLKernel 
        where TRange: struct, Brahma.INDRangeDimension
        where T1: IMem 
        where T2: IMem
    {
        private static readonly TRange _range = new TRange();
        
        private readonly StringBuilder _source = new StringBuilder();

        StringBuilder ICLKernel.Source
        {
            get
            {
                return _source;
            }
        }

        IEnumerable<MemberExpression> ICLKernel.Closures
        {
            get
            {
                return Closures;
            }
            set
            {
                Closures = value;
            }
        }

        IEnumerable<ParameterExpression> ICLKernel.Parameters
        {
            get
            {
                return Parameters;
            }
            set
            {
                Parameters = value;
            }
        }

        Cl.Kernel ICLKernel.ClKernel
        {
            get;
            set;
        }

        int ICLKernel.WorkDim
        {
            get
            {
                return ((INDRangeDimension)_range).Dimensions;
            }
        }
    }

    public sealed class Kernel<TRange, T1, T2, T3, TResult> : Brahma.Kernel<TRange, T1, T2, T3, TResult>, ICLKernel
        where TRange : struct, Brahma.INDRangeDimension
        where T1 : IMem
        where T2 : IMem
        where T3: IMem
    {
        private static readonly TRange _range = new TRange();

        private readonly StringBuilder _source = new StringBuilder();

        StringBuilder ICLKernel.Source
        {
            get
            {
                return _source;
            }
        }

        IEnumerable<MemberExpression> ICLKernel.Closures
        {
            get
            {
                return Closures;
            }
            set
            {
                Closures = value;
            }
        }

        IEnumerable<ParameterExpression> ICLKernel.Parameters
        {
            get
            {
                return Parameters;
            }
            set
            {
                Parameters = value;
            }
        }

        Cl.Kernel ICLKernel.ClKernel
        {
            get;
            set;
        }

        int ICLKernel.WorkDim
        {
            get
            {
                return ((INDRangeDimension)_range).Dimensions;
            }
        }
    }

    public sealed class Kernel<TRange, T1, T2, T3, T4, TResult> : Brahma.Kernel<TRange, T1, T2, T3, T4, TResult>, ICLKernel
        where TRange : struct, Brahma.INDRangeDimension
        where T1 : IMem
        where T2 : IMem
        where T3 : IMem
        where T4 : IMem
    {
        private static readonly TRange _range = new TRange();

        private readonly StringBuilder _source = new StringBuilder();

        StringBuilder ICLKernel.Source
        {
            get
            {
                return _source;
            }
        }

        IEnumerable<MemberExpression> ICLKernel.Closures
        {
            get
            {
                return Closures;
            }
            set
            {
                Closures = value;
            }
        }

        IEnumerable<ParameterExpression> ICLKernel.Parameters
        {
            get
            {
                return Parameters;
            }
            set
            {
                Parameters = value;
            }
        }

        Cl.Kernel ICLKernel.ClKernel
        {
            get;
            set;
        }

        int ICLKernel.WorkDim
        {
            get
            {
                return ((INDRangeDimension)_range).Dimensions;
            }
        }
    }

    public static class KernelExtensions
    {
        public static Run<TRange, Set[]> Run<TRange>(this Kernel<TRange, Set[]> kernel, TRange range)
            where TRange: struct, Brahma.INDRangeDimension
        {
            return new Run<TRange, Set[]>(kernel, range);
        }
        
        public static Run<TRange, T, Set[]> Run<TRange, T>(this Kernel<TRange, T, Set[]> kernel, TRange range, T data) 
            where TRange: struct, INDRangeDimension
            where T: IMem
        {
            return new Run<TRange, T, Set[]>(kernel, range, data);
        }
        
        public static Run<TRange, T1, T2, Set[]> Run<TRange, T1, T2>(this Kernel<TRange, T1, T2, Set[]> kernel, TRange range, T1 d1, T2 d2) 
            where TRange: struct, INDRangeDimension
            where T1: IMem 
            where T2: IMem
        {
            return new Run<TRange, T1, T2, Set[]>(kernel, range, d1, d2);
        }
        
        public static Run<TRange, T1, T2, T3, Set[]> Run<TRange, T1, T2, T3>(this Kernel<TRange, T1, T2, T3, Set[]> kernel, TRange range, T1 d1, T2 d2, T3 d3)
            where TRange : struct, INDRangeDimension
            where T1 : IMem
            where T2 : IMem
            where T3 : IMem
        {
            return new Run<TRange, T1, T2, T3, Set[]>(kernel, range, d1, d2, d3);
        }

        public static Run<TRange, T1, T2, T3, T4, Set[]> Run<TRange, T1, T2, T3, T4>(this Kernel<TRange, T1, T2, T3, T4, Set[]> kernel, TRange range, T1 d1, T2 d2, T3 d3, T4 d4)
            where TRange : struct, INDRangeDimension
            where T1 : IMem
            where T2 : IMem
            where T3 : IMem
            where T4 : IMem
        {
            return new Run<TRange, T1, T2, T3, T4, Set[]>(kernel, range, d1, d2, d3, d4);
        }
    }
}