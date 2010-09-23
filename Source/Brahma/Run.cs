using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma
{
    public abstract class Run<TResult> : Command<TResult> where TResult : struct
    {
        protected Run(Kernel<TResult> kernel)
        {
        }
    }

    public abstract class Run<T, TResult> : Command<T, TResult> where T : struct
                                                                where TResult : struct
    {
        protected Run(Kernel<T, TResult> kernel, Buffer<T> buffer)
        { 
        }
    }

    public abstract class Run<T1, T2, TResult> : Command<T1, T2, TResult> where T1 : struct
                                                                          where T2 : struct
                                                                          where TResult : struct
    {
        protected Run(Kernel<T1, T2, TResult> kernel, Buffer<T1> d1, Buffer<T2> d2)
        {
        }
    }
}