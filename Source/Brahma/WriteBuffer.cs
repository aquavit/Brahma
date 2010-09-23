using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma
{
    public abstract class WriteBuffer<T> : Command<T> where T : struct
    {
        protected WriteBuffer(Buffer<T> buffer,
                              bool blocking, 
                              int offset, 
                              int count, 
                              T[] data)
        {
        }
    }
}