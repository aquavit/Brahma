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
using OpenCL.Net;

namespace Brahma.OpenCL.Commands
{
    public sealed class Run<TRange, TResult> : Brahma.Commands.Run<TRange, TResult>
        where TRange: struct, Brahma.INDRangeDimension
    {
        protected override void SetupArguments(object sender, uint index, IntPtr size, object value)
        {
            var kernel = Kernel as ICLKernel;
            
            Cl.ErrorCode error = Cl.SetKernelArg(kernel.ClKernel, index, size, value);
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);
        }
        
        internal Run(Kernel<TRange, TResult> kernel, TRange range)
            : base(kernel, range)
        {
        }

        public override void EnqueueInto(object sender)
        {
            base.EnqueueInto(sender);

            var queue = sender as CommandQueue;
            var kernel = Kernel as ICLKernel;
            var range = Range as INDRangeDimension;
            var waitList = from name in WaitList
                           let ev = CommandQueue.FindEvent(name)
                           where ev != null
                           select ev.Value;

            Cl.Event eventID;
            Cl.ErrorCode error = Cl.EnqueueNDRangeKernel(queue.Queue, kernel.ClKernel, (uint)kernel.WorkDim, null,
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
        protected override void SetupArguments(object sender, uint index, IntPtr size, object value)
        {
            var kernel = Kernel as ICLKernel;
            Cl.ErrorCode error = Cl.SetKernelArg(kernel.ClKernel, index, size, value);
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);
        }
        
        internal Run(Kernel<TRange, T, TResult> kernel, TRange range, T data)
            : base(kernel, range, data)
        {
        }

        public override void EnqueueInto(object sender)
        {
            base.EnqueueInto(sender);

            var queue = sender as CommandQueue;
            var kernel = Kernel as ICLKernel;
            var range = Range as INDRangeDimension;
            var waitList = from name in WaitList
                           let ev = CommandQueue.FindEvent(name)
                           where ev != null
                           select ev.Value;

            Cl.Event eventID;
            Cl.ErrorCode error = Cl.EnqueueNDRangeKernel(queue.Queue, kernel.ClKernel, (uint)kernel.WorkDim, null,
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
        protected override void SetupArguments(object sender, uint index, IntPtr size, object value)
        {
            var kernel = Kernel as ICLKernel;
            Cl.ErrorCode error = Cl.SetKernelArg(kernel.ClKernel, index, size, value);
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);
        }
        
        internal Run(Kernel<TRange, T1, T2, TResult> kernel, TRange range, T1 d1, T2 d2)
            : base(kernel, range, d1, d2)
        {
        }

        public override void EnqueueInto(object sender)
        {
            base.EnqueueInto(sender);

            var queue = sender as CommandQueue;
            var kernel = Kernel as ICLKernel;
            var range = Range as INDRangeDimension;
            var waitList = from name in WaitList
                           let ev = CommandQueue.FindEvent(name)
                           where ev != null
                           select ev.Value;

            Cl.Event eventID;
            Cl.ErrorCode error = Cl.EnqueueNDRangeKernel(queue.Queue, kernel.ClKernel, (uint)kernel.WorkDim, null,
                range.GlobalWorkSize, range.LocalWorkSize, (uint)waitList.Count(), waitList.Count() == 0 ? null : waitList.ToArray(), out eventID);
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);

            if (Name == string.Empty)
                eventID.Dispose();
            else
                CommandQueue.AddEvent(Name, eventID);
        }
    }

    public sealed class Run<TRange, T1, T2, T3, TResult> : Brahma.Commands.Run<TRange, T1, T2, T3, TResult>
        where TRange : struct, Brahma.INDRangeDimension
        where T1 : IMem
        where T2 : IMem
        where T3 : IMem
    {
        protected override void SetupArguments(object sender, uint index, IntPtr size, object value)
        {
            var kernel = Kernel as ICLKernel;
            Cl.ErrorCode error = Cl.SetKernelArg(kernel.ClKernel, index, size, value);
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);
        }

        internal Run(Kernel<TRange, T1, T2, T3, TResult> kernel, TRange range, T1 d1, T2 d2, T3 d3)
            : base(kernel, range, d1, d2, d3)
        {
        }

        public override void EnqueueInto(object sender)
        {
            base.EnqueueInto(sender);

            var queue = sender as CommandQueue;
            var kernel = Kernel as ICLKernel;
            var range = Range as INDRangeDimension;
            var waitList = from name in WaitList
                           let ev = CommandQueue.FindEvent(name)
                           where ev != null
                           select ev.Value;

            Cl.Event eventID;
            Cl.ErrorCode error = Cl.EnqueueNDRangeKernel(queue.Queue, kernel.ClKernel, (uint)kernel.WorkDim, null,
                range.GlobalWorkSize, range.LocalWorkSize, (uint)waitList.Count(), waitList.Count() == 0 ? null : waitList.ToArray(), out eventID);
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);

            if (Name == string.Empty)
                eventID.Dispose();
            else
                CommandQueue.AddEvent(Name, eventID);
        }
    }

    public sealed class Run<TRange, T1, T2, T3, T4, TResult> : Brahma.Commands.Run<TRange, T1, T2, T3, T4, TResult>
        where TRange : struct, Brahma.INDRangeDimension
        where T1 : IMem
        where T2 : IMem
        where T3 : IMem
        where T4: IMem
    {
        protected override void SetupArguments(object sender, uint index, IntPtr size, object value)
        {
            var kernel = Kernel as ICLKernel;
            Cl.ErrorCode error = Cl.SetKernelArg(kernel.ClKernel, index, size, value);
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);
        }

        internal Run(Kernel<TRange, T1, T2, T3, T4, TResult> kernel, TRange range, T1 d1, T2 d2, T3 d3, T4 d4)
            : base(kernel, range, d1, d2, d3, d4)
        {
        }

        public override void EnqueueInto(object sender)
        {
            base.EnqueueInto(sender);

            var queue = sender as CommandQueue;
            var kernel = Kernel as ICLKernel;
            var range = Range as INDRangeDimension;
            var waitList = from name in WaitList
                           let ev = CommandQueue.FindEvent(name)
                           where ev != null
                           select ev.Value;

            Cl.Event eventID;
            Cl.ErrorCode error = Cl.EnqueueNDRangeKernel(queue.Queue, kernel.ClKernel, (uint)kernel.WorkDim, null,
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