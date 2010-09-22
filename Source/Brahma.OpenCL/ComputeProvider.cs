using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenCL.Net;

namespace Brahma.OpenCL
{
    public sealed class ComputeProvider: Brahma.ComputeProvider
    {
        public ComputeProvider(Cl.DeviceType deviceType)
        {
        }

        public ComputeProvider(params Cl.Device[] devices)
        {
        }

        public override Brahma.Kernel<IEnumerable<TResult>> Compile<TResult>(System.Linq.Expressions.Expression<Func<IEnumerable<TResult>>> kernel)
        {
            throw new NotImplementedException();
        }

        public override Brahma.Kernel<T, IEnumerable<TResult>> Compile<T, TResult>(System.Linq.Expressions.Expression<Func<Brahma.Buffer<T>, IEnumerable<TResult>>> kernel)
        {
            throw new NotImplementedException();
        }

        public override Brahma.Kernel<T1, T2, IEnumerable<TResult>> Compile<T1, T2, TResult>(System.Linq.Expressions.Expression<Func<Brahma.Buffer<T1>, Brahma.Buffer<T2>, IEnumerable<TResult>>> kernel)
        {
            throw new NotImplementedException();
        }
    }

    // Usage example
    public static class Program
    {
        public struct Coefficients
        {
            public float a;
            public float b;
            public float factor;
        }
        
        static void main()
        {
            ComputeProvider provider = new ComputeProvider(Cl.DeviceType.Cpu);
            CommandQueue queue = new CommandQueue(provider);
            var coefficients = new Buffer<Coefficients>();
            var coeffData = new Coefficients[10];

            queue.Add(
                    coefficients.WriteAsync("write", 0, 10, coeffData),
                    (WaitFor)"write" & provider.Compile<Coefficients, float>(coeffs => from c in coeffs select c.factor).Run(),
                    coefficients.Read(0, 10, coeffData)
                );
        }
    }
}