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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenCL.Net;

namespace Brahma.OpenCL.Commands
{
    public sealed class Run<TRange, TResult> : Brahma.Commands.Run<TRange, TResult>
        where TRange: struct, Brahma.INDRangeDimension
    {
        internal Run(Kernel<TRange, TResult> kernel, TRange range)
            : base(kernel)
        {
            Kernel = kernel;
            Range = range;
        }

        internal Kernel<TRange, TResult> Kernel
        {
            get;
            private set;
        }

        internal TRange Range
        {
            get;
            private set;
        }

        public override void EnqueueInto(object sender)
        {
            CommandQueue queue = sender as CommandQueue;
            var kernel = Kernel as ICLKernel;
            var range = Range as INDRangeDimension;
            var waitList = from name in WaitList
                           let ev = CommandQueue.FindEvent(name)
                           where ev != null
                           select ev.Value;

            Cl.ErrorCode error;

            // No kernel arguments to set up

            // TODO: Add closure arguments

            Cl.Event eventID;
            error = Cl.EnqueueNDRangeKernel(queue.Queue, kernel.ClKernel, (uint)kernel.WorkDim, null,
                range.GlobalWorkSize, range.LocalWorkSize, (uint)waitList.Count(), waitList.Count() == 0 ? null : waitList.ToArray(), out eventID);
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);

            if (Name == string.Empty)
                eventID.Dispose();
            else
                CommandQueue.AddEvent(Name, eventID);
        }
    }

    public sealed class Run<TRange, T, TResult> : Brahma.Commands.Run<TRange, T, TResult> 
        where TRange: struct, Brahma.INDRangeDimension
        where T: IMem
    {
        internal Run(Kernel<TRange, T, TResult> kernel, TRange range, T data)
            : base(kernel, data)
        {
            Kernel = kernel;
            Data = data;
            Range = range;
        }

        internal Kernel<TRange, T, TResult> Kernel
        {
            get;
            private set;
        }

        internal T Data
        {
            get;
            private set;
        }

        internal TRange Range
        {
            get;
            private set;
        }

        public override void EnqueueInto(object sender)
        {
            CommandQueue queue = sender as CommandQueue;
            var kernel = Kernel as ICLKernel;
            var range = Range as INDRangeDimension;
            var waitList = from name in WaitList
                           let ev = CommandQueue.FindEvent(name)
                           where ev != null
                           select ev.Value;

            Cl.ErrorCode error;
            uint argIndex = 0;

            // Set up kernel arguments
            error = Cl.SetKernelArg(kernel.ClKernel, argIndex++, Data.Size, Data.Data);
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);

            // TODO: Add closure arguments

            Cl.Event eventID;
            error = Cl.EnqueueNDRangeKernel(queue.Queue, kernel.ClKernel, (uint)kernel.WorkDim, null,
                range.GlobalWorkSize, range.LocalWorkSize, (uint)waitList.Count(), waitList.Count() == 0 ? null : waitList.ToArray(), out eventID);
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);

            if (Name == string.Empty)
                eventID.Dispose();
            else
                CommandQueue.AddEvent(Name, eventID);
        }
    }

    public sealed class Run<TRange, T1, T2, TResult> : Brahma.Commands.Run<TRange, T1, T2, TResult> 
        where TRange: struct, Brahma.INDRangeDimension
        where T1: IMem 
        where T2: IMem
    {
        internal Run(Kernel<TRange, T1, T2, TResult> kernel, TRange range, T1 d1, T2 d2)
            : base(kernel, d1, d2)
        {
            Kernel = kernel;
            D1 = d1;
            D2 = d2;
            Range = range;
        }

        internal Kernel<TRange, T1, T2, TResult> Kernel
        {
            get;
            private set;
        }

        internal T1 D1
        {
            get;
            private set;
        }

        internal T2 D2
        {
            get;
            private set;
        }

        internal TRange Range
        {
            get;
            private set;
        }

        public override void EnqueueInto(object sender)
        {
            CommandQueue queue = sender as CommandQueue;
            var kernel = Kernel as ICLKernel;
            var range = Range as INDRangeDimension;
            var waitList = from name in WaitList
                           let ev = CommandQueue.FindEvent(name)
                           where ev != null
                           select ev.Value;

            Cl.ErrorCode error;
            uint argIndex = 0;

            // Set up kernel arguments
            error = Cl.SetKernelArg(kernel.ClKernel, argIndex++, D1.Size, D1.Data);
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);
            error = Cl.SetKernelArg(kernel.ClKernel, argIndex++, D1.Size, D2.Data);
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);

            object value = null;

            foreach (var memberExp in kernel.Closures)
            {
                switch (memberExp.Member.MemberType)
                {
                    case MemberTypes.Field:
                        value = (memberExp.Member as FieldInfo).GetValue((memberExp.Expression as ConstantExpression).Value);
                        break;

                    default:
                        throw new NotSupportedException("Can only access a field from inside a kernel");
                }
                
                error = Cl.SetKernelArg(kernel.ClKernel, argIndex++, (IntPtr)Marshal.SizeOf(value), value);
                if (error != Cl.ErrorCode.Success)
                    throw new CLException(error);
            }

            Cl.Event eventID;
            error = Cl.EnqueueNDRangeKernel(queue.Queue, kernel.ClKernel, (uint)kernel.WorkDim, null, 
                range.GlobalWorkSize, range.LocalWorkSize, (uint)waitList.Count(), waitList.Count() == 0 ? null : waitList.ToArray(), out eventID);
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);

            if (Name == string.Empty)
                eventID.Dispose();
            else
                CommandQueue.AddEvent(Name, eventID);
        }
    }
}