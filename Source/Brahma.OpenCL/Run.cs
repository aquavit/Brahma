using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma.OpenCL
{
    public sealed class Run<TResult> : Brahma.Run<TResult> where TResult: struct
    {
        public Run(Kernel<TResult> kernel)
            : base(kernel)
        {
        }
    }

    public sealed class Run<T, TResult> : Brahma.Run<T, TResult> where T: struct where TResult: struct
    {
        public Run(Kernel<T, TResult> kernel, Buffer<T> buffer)
            : base(kernel, buffer)
        {
        }
    }

    public sealed class Run<T1, T2, TResult> : Brahma.Run<T1, T2, TResult> where T1: struct where T2: struct where TResult: struct
    {
        public Run(Kernel<T1, T2, TResult> kernel, Buffer<T1> d1, Buffer<T2> d2)
            : base(kernel, d1, d2)
        {
        }
    }
}