using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma.OpenCL
{
    public sealed class Buffer<T>: Brahma.Buffer<T> where T: struct
    {
        public override IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public static class BufferExtensions
    {
        private const string AnonymousReadName = "Read_{0}";
        private static int _anonymousReadID = 0;
        
        public static ReadBuffer<T> Read<T>(this Buffer<T> buffer,
            string name,
            int offset,
            int count,
            T[] data) where T: struct
        {
            return new ReadBuffer<T>(name, buffer, true, offset, count, data);
        }

        public static ReadBuffer<T> Read<T>(this Buffer<T> buffer,
            int offset,
            int count,
            T[] data) where T : struct
        {
            return new ReadBuffer<T>(string.Format(AnonymousReadName, _anonymousReadID++), 
                buffer, true, offset, count, data);
        }

        public static ReadBuffer<T> ReadAsync<T>(this Buffer<T> buffer,
            string name,
            int offset,
            int count,
            T[] data) where T : struct
        {
            return new ReadBuffer<T>(name, buffer, false, offset, count, data);
        }

        public static ReadBuffer<T> ReadAsync<T>(this Buffer<T> buffer,
            int offset,
            int count,
            T[] data) where T : struct
        {
            return new ReadBuffer<T>(string.Format(AnonymousReadName, _anonymousReadID++),
                buffer, false, offset, count, data);
        }

        private const string AnonymousWriteName = "Read_{0}";
        private static int _anonymousWriteID = 0;

        public static WriteBuffer<T> Write<T>(this Buffer<T> buffer,
            string name,
            int offset,
            int count,
            T[] data) where T : struct
        {
            return new WriteBuffer<T>(name, buffer, true, offset, count, data);
        }

        public static WriteBuffer<T> Write<T>(this Buffer<T> buffer,
            int offset,
            int count,
            T[] data) where T : struct
        {
            return new WriteBuffer<T>(string.Format(AnonymousWriteName, _anonymousWriteID++),
                buffer, true, offset, count, data);
        }

        public static WriteBuffer<T> WriteAsync<T>(this Buffer<T> buffer,
            string name,
            int offset,
            int count,
            T[] data) where T : struct
        {
            return new WriteBuffer<T>(name, buffer, false, offset, count, data);
        }

        public static WriteBuffer<T> WriteAsync<T>(this Buffer<T> buffer,
            int offset,
            int count,
            T[] data) where T : struct
        {
            return new WriteBuffer<T>(string.Format(AnonymousWriteName, _anonymousWriteID++),
                buffer, false, offset, count, data);
        }
    }
}