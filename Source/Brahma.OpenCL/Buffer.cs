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
using System.Runtime.InteropServices;
using Brahma.OpenCL.Commands;
using OpenCL.Net;

namespace Brahma.OpenCL
{
    public enum Operations: ulong
    {
        ReadWrite = MemFlags.ReadWrite,
        ReadOnly = MemFlags.ReadOnly,
        WriteOnly = MemFlags.WriteOnly,
    }

    public enum Memory : ulong 
    {
        Device = 0,
        HostAccessible = MemFlags.AllocHostPtr,
        Host = MemFlags.UseHostPtr
    }
    
    public class Buffer<T>: Brahma.Buffer<T> where T: struct, IMem
    {
        private static readonly IntPtr _intPtrSize = (IntPtr)Marshal.SizeOf(typeof(IntPtr));
        private static readonly int _elementSize = Marshal.SizeOf(typeof(T));

        private global::OpenCL.Net.IMem _mem;
        private bool _disposed;
        private readonly int _length;

        public readonly Operations Operations;
        public readonly Memory Memory;

        internal global::OpenCL.Net.IMem Mem
        {
            get
            {
                return _mem;
            }
        }

        public Buffer(ComputeProvider provider, Operations operations, bool hostAccessible, int length) // Create, no data
        {
            ErrorCode error;
            _length = length;
            var size = (IntPtr)(_length * _elementSize);
            _mem = Cl.CreateBuffer(provider.Context, (MemFlags)operations | (hostAccessible ? MemFlags.AllocHostPtr : 0), size, null, out error);

            if (error != ErrorCode.Success)
                throw new CLException(error);

            Operations = operations;
            Memory = Memory.Device;
        }

        public Buffer(ComputeProvider provider, Operations operations, Memory memory, Array data) // Create and copy/use data from host
        {
            ErrorCode error;
            _length = data.Length;

            _mem = Cl.CreateBuffer(provider.Context, (MemFlags)operations | (memory == Memory.Host ? MemFlags.UseHostPtr : (MemFlags)memory | MemFlags.CopyHostPtr),
                (IntPtr)(_elementSize * data.Length), data, out error);

            if (error != ErrorCode.Success)
                throw new CLException(error);

            Operations = operations;
            Memory = memory;
        }

        public Buffer(ComputeProvider provider, Operations operations, Memory memory, T[] data) // Create and copy/use data from host
            : this(provider, operations, memory, (Array)data)
        {
        }

        public Buffer(ComputeProvider provider, Operations operations, Memory memory, IntPtr data, int length) // Create and copy/use data from host
        {
            ErrorCode error;
            _length = length;
            _mem = Cl.CreateBuffer(provider.Context, (MemFlags)operations | (memory == Memory.Host ? MemFlags.UseHostPtr : (MemFlags)memory | (data != IntPtr.Zero ? MemFlags.CopyHostPtr : 0)),
                (IntPtr)(_elementSize * _length), data, out error);

            if (error != ErrorCode.Success)
                throw new CLException(error);

            Operations = operations;
            Memory = memory;
        }

        [KernelCallable]
        public override T this[Types.int32 index]
        {
            get
            {
                throw new InvalidOperationException("Can only index into a buffer inside a kernel");
            }
            set
            {
                throw new InvalidOperationException("Can only index into a buffer inside a kernel");
            }
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _mem.Dispose();
                _disposed = true;
            }
        }

        public override int Length
        {
            get
            {
                return _length;
            }
        }

        public int ElementSize
        {
            get
            {
                return _elementSize;
            }
        }

        public override IntPtr Size
        {
            get
            {
                return _intPtrSize;
            }
        }

        public override object Data
        {
            get
            {
                return _mem;
            }
        }
    }

    public static class BufferExtensions
    {
        public static ReadBuffer<T> Read<T>(this Buffer<T> buffer,
            int offset,
            int count,
            T[] data) where T : struct, IMem
        {
            return new ReadBuffer<T>(buffer, true, offset, count, data);
        }

        public static ReadBuffer<T> Read<T>(this Buffer<T> buffer,
            int offset,
            int count,
            Array data) where T : struct, IMem
        {
            return new ReadBuffer<T>(buffer, true, offset, count, data);
        }

        public static ReadBuffer<T> Read<T>(this Buffer<T> buffer,
            int offset,
            int count,
            IntPtr data) where T : struct, IMem
        {
            return new ReadBuffer<T>(buffer, true, offset, count, data);
        }

        public static ReadBuffer<T> ReadAsync<T>(this Buffer<T> buffer,
            int offset,
            int count,
            T[] data) where T : struct, IMem
        {
            return new ReadBuffer<T>(buffer, false, offset, count, data);
        }

        public static ReadBuffer<T> ReadAsync<T>(this Buffer<T> buffer,
            int offset,
            int count,
            Array data) where T : struct, IMem
        {
            return new ReadBuffer<T>(buffer, false, offset, count, data);
        }

        public static ReadBuffer<T> ReadAsync<T>(this Buffer<T> buffer,
            int offset,
            int count,
            IntPtr data) where T : struct, IMem
        {
            return new ReadBuffer<T>(buffer, false, offset, count, data);
        }

        public static WriteBuffer<T> Write<T>(this Buffer<T> buffer,
            int offset,
            int count,
            T[] data) where T : struct, IMem
        {
            return new WriteBuffer<T>(buffer, true, offset, count, data);
        }

        public static WriteBuffer<T> Write<T>(this Buffer<T> buffer,
            int offset,
            int count,
            Array data) where T : struct, IMem
        {
            return new WriteBuffer<T>(buffer, true, offset, count, data);
        }

        public static WriteBuffer<T> Write<T>(this Buffer<T> buffer,
            int offset,
            int count,
            IntPtr data) where T : struct, IMem
        {
            return new WriteBuffer<T>(buffer, true, offset, count, data);
        }

        public static WriteBuffer<T> WriteAsync<T>(this Buffer<T> buffer,
            int offset,
            int count,
            T[] data) where T : struct, IMem
        {
            return new WriteBuffer<T>(buffer, false, offset, count, data);
        }

        public static WriteBuffer<T> WriteAsync<T>(this Buffer<T> buffer,
            int offset,
            int count,
            Array data) where T : struct, IMem
        {
            return new WriteBuffer<T>(buffer, false, offset, count, data);
        }

        public static WriteBuffer<T> WriteAsync<T>(this Buffer<T> buffer,
            int offset,
            int count,
            IntPtr data) where T : struct, IMem
        {
            return new WriteBuffer<T>(buffer, false, offset, count, data);
        }
    }
}