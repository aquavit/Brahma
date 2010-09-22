using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma
{
    public abstract class ReadBuffer<T>: Command<T> where T: struct
    {
        protected ReadBuffer(string name,
                             Buffer<T> buffer,
                             bool blocking,
                             int offset,
                             int count,
                             T[] data)
            : base(name)
        {
        }
    }
}