using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenCL.Net;

namespace Brahma.OpenCL
{
    public enum BufferFlags: ulong
    {
        ReadWrite = Cl.MemFlags.ReadWrite,
        ReadOnly = Cl.MemFlags.ReadOnly,
        WriteOnly = Cl.MemFlags.WriteOnly,
        HostAccessible = Cl.MemFlags.AllocHostPtr
    }
    
    public sealed class Buffer<T>: Brahma.Buffer<T> where T: struct
    {
        private Cl.Mem _mem;
        
        public Buffer(ComputeProvider provider, BufferFlags flags, IntPtr size)
        {
            Cl.ErrorCode error;
            _mem = Cl.CreateBuffer(provider.Context, (Cl.MemFlags)flags, size, null, out error);
        }

        // TODO: Add overloads, one that takes an array and the other that takes a Func<IEnumerable<T>>
        
        public override IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public static class BufferExtensions
    {
        public static ReadBuffer<T> Read<T>(this Buffer<T> buffer,
            int offset,
            int count,
            T[] data) where T : struct
        {
            return new ReadBuffer<T>(buffer, true, offset, count, data);
        }

        public static ReadBuffer<T> ReadAsync<T>(this Buffer<T> buffer,
            int offset,
            int count,
            T[] data) where T : struct
        {
            return new ReadBuffer<T>(buffer, false, offset, count, data);
        }

        public static WriteBuffer<T> Write<T>(this Buffer<T> buffer,
            int offset,
            int count,
            T[] data) where T : struct
        {
            return new WriteBuffer<T>(buffer, true, offset, count, data);
        }

        public static WriteBuffer<T> WriteAsync<T>(this Buffer<T> buffer,
            int offset,
            int count,
            T[] data) where T : struct
        {
            return new WriteBuffer<T>(buffer, false, offset, count, data);
        }
    }
}