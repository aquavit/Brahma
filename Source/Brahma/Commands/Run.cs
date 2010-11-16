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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Brahma.Commands
{
    public abstract class Run<TRange, TResult> : Command<TResult> 
        where TRange: struct, INDRangeDimension
    {
        protected Run(Kernel<TRange, TResult> kernel, TRange range)
        {
            Kernel = kernel;
            Range = range;
        }

        protected abstract void SetupArguments(object sender, uint index, IntPtr size, object value);

        public override void EnqueueInto(object sender)
        {
            uint index = 0;

            foreach (var memberExp in Kernel.Closures)
            {
                object value;
                switch (memberExp.Member.MemberType)
                {
                    case MemberTypes.Field:
                        value = (memberExp.Member as FieldInfo).GetValue((memberExp.Expression as ConstantExpression).Value);
                        break;

                    default:
                        throw new NotSupportedException("Can only access a field from inside a kernel");
                }

                SetupArguments(sender, index++, (IntPtr)Marshal.SizeOf(value), value);
            }
        }

        public TRange Range
        {
            get;
            private set;
        }

        public Kernel<TRange, TResult> Kernel
        {
            get;
            private set;
        }
    }

    public abstract class Run<TRange, T, TResult> : Command<T, TResult> 
        where T: IMem 
        where TRange: struct, INDRangeDimension
    {
        protected Run(Kernel<TRange, T, TResult> kernel, TRange range, T data)
        {
            Range = range;
            Kernel = kernel;
            Data = data;
        }

        protected abstract void SetupArguments(object sender, uint index, IntPtr size, object value);

        public override void EnqueueInto(object sender)
        {
            uint index = 0;

            SetupArguments(sender, index++, Data.Size, Data.Data);

            foreach (var memberExp in Kernel.Closures)
            {
                object value;
                switch (memberExp.Member.MemberType)
                {
                    case MemberTypes.Field:
                        value = (memberExp.Member as FieldInfo).GetValue((memberExp.Expression as ConstantExpression).Value);
                        break;

                    default:
                        throw new NotSupportedException("Can only access a field from inside a kernel");
                }

                SetupArguments(sender, index++, (IntPtr)Marshal.SizeOf(value), value);
            }
        }

        public TRange Range
        {
            get;
            private set;
        }

        protected internal Kernel Kernel
        {
            get;
            private set;
        }

        public T Data
        {
            get;
            private set;
        }
    }

    public abstract class Run<TRange, T1, T2, TResult> : Command<T1, T2, TResult> 
        where T1: IMem 
        where T2: IMem 
        where TRange: struct, INDRangeDimension
    {
        protected Run(Kernel<TRange, T1, T2, TResult> kernel, TRange range, T1 d1, T2 d2)
        {
            Range = range;
            Kernel = kernel;
            D1 = d1;
            D2 = d2;
        }

        protected abstract void SetupArguments(object sender, uint index, IntPtr size, object value);

        public override void EnqueueInto(object sender)
        {
            uint index = 0;

            SetupArguments(sender, index++, D1.Size, D1.Data);
            SetupArguments(sender, index++, D2.Size, D2.Data);

            foreach (var memberExp in Kernel.Closures)
            {
                object value;
                switch (memberExp.Member.MemberType)
                {
                    case MemberTypes.Field:
                        value = (memberExp.Member as FieldInfo).GetValue((memberExp.Expression as ConstantExpression).Value);
                        break;

                    default:
                        throw new NotSupportedException("Can only access a field from inside a kernel");
                }

                SetupArguments(sender, index++, (IntPtr)Marshal.SizeOf(value), value);
            }
        }

        public TRange Range
        {
            get;
            private set;
        }

        protected internal Kernel Kernel
        {
            get;
            private set;
        }

        public T1 D1
        {
            get; 
            private set; 
        }

        public T2 D2
        {
            get;
            private set;
        }
    }
    
    public abstract class Run<TRange, T1, T2, T3, TResult> : Command<T1, T2, T3, TResult>
        where T1 : IMem
        where T2 : IMem
        where T3: IMem
        where TRange : struct, INDRangeDimension
    {
        protected Run(Kernel<TRange, T1, T2, T3, TResult> kernel, TRange range, T1 d1, T2 d2, T3 d3)
        {
            Range = range;
            Kernel = kernel;
            D1 = d1;
            D2 = d2;
            D3 = d3;
        }

        protected abstract void SetupArguments(object sender, uint index, IntPtr size, object value);

        public override void EnqueueInto(object sender)
        {
            uint index = 0;

            SetupArguments(sender, index++, D1.Size, D1.Data);
            SetupArguments(sender, index++, D2.Size, D2.Data);
            SetupArguments(sender, index++, D3.Size, D3.Data);

            foreach (var memberExp in Kernel.Closures)
            {
                object value;
                switch (memberExp.Member.MemberType)
                {
                    case MemberTypes.Field:
                        value = (memberExp.Member as FieldInfo).GetValue((memberExp.Expression as ConstantExpression).Value);
                        break;

                    default:
                        throw new NotSupportedException("Can only access a field from inside a kernel");
                }

                SetupArguments(sender, index++, (IntPtr)Marshal.SizeOf(value), value);
            }
        }

        public TRange Range
        {
            get;
            private set;
        }

        protected internal Kernel Kernel
        {
            get;
            private set;
        }

        public T1 D1
        {
            get;
            private set;
        }

        public T2 D2
        {
            get;
            private set;
        }

        public T3 D3
        {
            get;
            private set;
        }
    }

    public abstract class Run<TRange, T1, T2, T3, T4, TResult> : Command<T1, T2, T3, T4, TResult>
        where T1 : IMem
        where T2 : IMem
        where T3 : IMem
        where T4: IMem
        where TRange : struct, INDRangeDimension
    {
        protected Run(Kernel<TRange, T1, T2, T3, T4, TResult> kernel, TRange range, T1 d1, T2 d2, T3 d3, T4 d4)
        {
            Range = range;
            Kernel = kernel;
            D1 = d1;
            D2 = d2;
            D3 = d3;
            D4 = d4;
        }

        protected abstract void SetupArguments(object sender, uint index, IntPtr size, object value);

        public override void EnqueueInto(object sender)
        {
            uint index = 0;

            SetupArguments(sender, index++, D1.Size, D1.Data);
            SetupArguments(sender, index++, D2.Size, D2.Data);
            SetupArguments(sender, index++, D3.Size, D3.Data);
            SetupArguments(sender, index++, D4.Size, D4.Data);

            foreach (var memberExp in Kernel.Closures)
            {
                object value;
                switch (memberExp.Member.MemberType)
                {
                    case MemberTypes.Field:
                        value = (memberExp.Member as FieldInfo).GetValue((memberExp.Expression as ConstantExpression).Value);
                        break;

                    default:
                        throw new NotSupportedException("Can only access a field from inside a kernel");
                }

                SetupArguments(sender, index++, (IntPtr)Marshal.SizeOf(value), value);
            }
        }

        public TRange Range
        {
            get;
            private set;
        }

        protected internal Kernel Kernel
        {
            get;
            private set;
        }

        public T1 D1
        {
            get;
            private set;
        }

        public T2 D2
        {
            get;
            private set;
        }

        public T3 D3
        {
            get;
            private set;
        }

        public T4 D4
        {
            get;
            private set;
        }
    }
}