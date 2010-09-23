using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma.OpenCL
{
    public sealed class ReadBuffer<T> : Brahma.ReadBuffer<T> where T : struct
    {
        public ReadBuffer(Buffer<T> buffer,
                          bool blocking,
                          int offset,
                          int count,
                          T[] data)
            : base(buffer, blocking, offset, count, data)
        {
        }
    }
}