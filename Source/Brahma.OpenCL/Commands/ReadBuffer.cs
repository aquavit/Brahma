﻿#region License and Copyright Notice
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
    public sealed class ReadBuffer<T> : Brahma.Commands.ReadBuffer<T> where T : struct, IMem
    {
        internal ReadBuffer(Buffer<T> buffer,
                          bool blocking,
                          int offset,
                          int count,
                          T[] data)
            : base(buffer, blocking, offset, count, data)
        {
            Buffer = buffer;
            Blocking = blocking;
            Offset = offset;
            Count = count;
            Data = data;
        }

        internal ReadBuffer(Buffer<T> buffer,
                          bool blocking,
                          int offset,
                          int count,
                          IntPtr data)
            : base(buffer, blocking, offset, count, data)
        {
            Buffer = buffer;
            Blocking = blocking;
            Offset = offset;
            Count = count;
            DataPtr = data;
        }

        internal Buffer<T> Buffer
        {
            get;
            private set;
        }

        internal bool Blocking
        {
            get;
            private set;
        }

        internal int Offset
        {
            get;
            private set;
        }

        internal int Count
        {
            get;
            private set;
        }

        internal T[] Data
        {
            get;
            private set;
        }

        internal IntPtr DataPtr
        {
            get;
            private set;
        }

        public override void EnqueueInto(object sender)
        {
            CommandQueue commandQueue = sender as CommandQueue;

            var waitList = from name in WaitList
                           let ev = CommandQueue.FindEvent(name)
                           where ev != null
                           select ev.Value;

            Cl.Event eventID;
            Cl.ErrorCode error;
            if (Data == null)
                error = Cl.EnqueueReadBuffer(commandQueue.Queue, Buffer.Mem,
                    Blocking ? Cl.Bool.True : Cl.Bool.False, (IntPtr)Offset,
                    (IntPtr)(Count * Buffer.ElementSize), DataPtr, (uint)waitList.Count(), waitList.Count() == 0 ? null : waitList.ToArray(), out eventID);
            else
                error = Cl.EnqueueReadBuffer(commandQueue.Queue, Buffer.Mem,
                    Blocking ? Cl.Bool.True : Cl.Bool.False, (IntPtr)Offset,
                    (IntPtr)(Count * Buffer.ElementSize), Data, (uint)waitList.Count(), waitList.Count() == 0 ? null : waitList.ToArray(), out eventID);
            
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);

            if (Name == string.Empty)
                eventID.Dispose();
            else
                CommandQueue.AddEvent(Name, eventID);
        }
    }
}