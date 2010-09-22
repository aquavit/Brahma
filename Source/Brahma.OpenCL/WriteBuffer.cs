using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma.OpenCL
{
    public sealed class WriteBuffer<T>: Brahma.WriteBuffer<T> where T: struct
    {
        public WriteBuffer(string name, 
                           Buffer<T> buffer, 
                           bool blocking, 
                           int offset, 
                           int count, 
                           T[] data)
            : base(name, buffer, blocking, offset, count, data)
        {
        }
    }
}