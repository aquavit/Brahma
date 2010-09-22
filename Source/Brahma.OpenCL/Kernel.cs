using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma.OpenCL
{
    public sealed class Kernel<TResult>: Brahma.Kernel<TResult>
    {
    }

    public sealed class Kernel<T, TResult>: Brahma.Kernel<T, TResult>
    {
    }

    public sealed class Kernel<T1, T2, TResult>: Brahma.Kernel<T1, T2, TResult>
    {
    }

    public static class KernelExtensions
    {
        public static Run<TResult> Run<TResult>(this Brahma.Kernel<IEnumerable<TResult>> kernel) where TResult: struct
        {
            throw new NotImplementedException();
        }

        public static Run<T, TResult> Run<T, TResult>(this Brahma.Kernel<T, IEnumerable<TResult>> kernel) where T: struct where TResult: struct
        {
            throw new NotImplementedException();
        }

        public static Run<T1, T2, TResult> Run<T1, T2, TResult>(this Brahma.Kernel<T1, T2, IEnumerable<TResult>> kernel) where T1: struct where T2: struct where TResult: struct
        {
            throw new NotImplementedException();
        }
    }
}