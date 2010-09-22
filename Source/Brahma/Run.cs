using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma
{
    public abstract class Run<TResult> : Command<TResult> where TResult : struct
    {
        protected Run(string name, Kernel<TResult> kernel)
            : base(name)
        {
        }
    }

    public abstract class Run<T, TResult> : Command<T, TResult> where T : struct
                                                                where TResult : struct
    {
        protected Run(string name, Kernel<T, TResult> kernel, Buffer<T> buffer)
            : base(name)
        { 
        }
    }

    public abstract class Run<T1, T2, TResult> : Command<T1, T2, TResult> where T1 : struct
                                                                          where T2 : struct
                                                                          where TResult : struct
    {
        protected Run(string name, Kernel<T1, T2, TResult> kernel, Buffer<T1> d1, Buffer<T2> d2)
            : base(name)
        {
        }
    }
}