using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

using OpenCL.Net;

namespace Brahma.OpenCL
{
    public sealed class ComputeProvider: Brahma.ComputeProvider
    {
        private readonly Cl.Context _context;
        private bool _disposed = false;

        internal Cl.Context Context
        {
            get 
            {
                return _context;
            }
        }
        
        public ComputeProvider(params Cl.Device[] devices)
        {
            Cl.ErrorCode error;
            _context = Cl.CreateContext(null, (uint)devices.Length, devices, null, IntPtr.Zero, out error);
            
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);
        }

        public override Brahma.Kernel<IEnumerable<TResult>> Compile<TResult>(Expression<Func<IEnumerable<TResult>>> kernel)
        {
            throw new NotImplementedException();
        }

        public override Brahma.Kernel<T, IEnumerable<TResult>> Compile<T, TResult>(Expression<Func<Brahma.Buffer<T>, IEnumerable<TResult>>> kernel)
        {
            throw new NotImplementedException();
        }

        public override Brahma.Kernel<T1, T2, IEnumerable<TResult>> Compile<T1, T2, TResult>(Expression<Func<Brahma.Buffer<T1>, Brahma.Buffer<T2>, IEnumerable<TResult>>> kernel)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _context.Dispose();
                _disposed = true;
            }
        }
    }
}