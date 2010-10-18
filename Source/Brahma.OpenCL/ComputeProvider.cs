#region License and Copyright Notice
// Copyright (c) 2010 Ananth B.
// All rights reserved.
// 
// The contents of this file are made available under the terms of the
// Eclipse Public License v1.0 (the "License") which accompanies this
// distribution, and is available at the following URL:
// http://www.opensource.org/licenses/eclipse-1.0.php
// 
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
// the specific language governing rights and limitations under the License.
// 
// By using this software in any fashion, you are agreeing to be bound by the
// terms of the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OpenCL.Net;

namespace Brahma.OpenCL
{
    public sealed class ComputeProvider: Brahma.ComputeProvider
    {
        private readonly Cl.Context _context;
        private readonly Cl.Device[] _devices;
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
            if (devices == null)
                throw new ArgumentNullException("devices");
            
            _devices = devices;
            
            Cl.ErrorCode error;
            _context = Cl.CreateContext(null, (uint)devices.Length, _devices, null, IntPtr.Zero, out error);
            
            if (error != Cl.ErrorCode.Success)
                throw new CLException(error);
        }

        protected override Brahma.Kernel<TRange, Set[]> CompileQuery<TRange>(Expression<Func<Brahma.NDRange<TRange>,IEnumerable<Set[]>>> query)
        {
            var lambda = query as LambdaExpression;
            var kernel = new Kernel<TRange, Set[]>();
            lambda.GenerateKernel(kernel);

            Cl.ErrorCode error;
            using (Cl.Program program = Cl.CreateProgramWithSource(_context, 1, new[] { (kernel as ICLKernel).Source.ToString() }, null, out error))
            {
                error = Cl.BuildProgram(program, (uint)_devices.Length, _devices, string.Empty, null, IntPtr.Zero);
                if (error != Cl.ErrorCode.Success)
                    throw new Exception(string.Join("\n", from device in _devices
                                                          select Cl.GetProgramBuildInfo(program, device, Cl.ProgramBuildInfo.Log, out error).ToString()));
                (kernel as ICLKernel).ClKernel = Cl.CreateKernel(program, CLCodeGenerator.KernelName, out error);
            }

            return kernel;
        }

        protected override Brahma.Kernel<TRange, T,Set[]> CompileQuery<TRange, T>(Expression<Func<Brahma.NDRange<TRange>,T,IEnumerable<Set[]>>> query)
        {
            var lambda = query as LambdaExpression;
            var kernel = new Kernel<TRange, T, Set[]>();
            lambda.GenerateKernel(kernel);

            Cl.ErrorCode error;
            using (Cl.Program program = Cl.CreateProgramWithSource(_context, 1, new[] { (kernel as ICLKernel).Source.ToString() }, null, out error))
            {
                error = Cl.BuildProgram(program, (uint)_devices.Length, _devices, string.Empty, null, IntPtr.Zero);
                if (error != Cl.ErrorCode.Success)
                    throw new Exception(string.Join("\n", from device in _devices
                                                          select Cl.GetProgramBuildInfo(program, device, Cl.ProgramBuildInfo.Log, out error).ToString()));
                (kernel as ICLKernel).ClKernel = Cl.CreateKernel(program, CLCodeGenerator.KernelName, out error);
            }

            return kernel;
        }

        protected override Brahma.Kernel<TRange, T1, T2, Set[]> CompileQuery<TRange,T1,T2>(Expression<Func<Brahma.NDRange<TRange>, T1, T2,IEnumerable<Set[]>>> query)
        {
            var lambda = query as LambdaExpression;
            var kernel = new Kernel<TRange, T1, T2, Set[]>();
            lambda.GenerateKernel(kernel);

            Cl.ErrorCode error;
            using (Cl.Program program = Cl.CreateProgramWithSource(_context, 1, new[] { (kernel as ICLKernel).Source.ToString() }, null, out error))
            {
                error = Cl.BuildProgram(program, (uint)_devices.Length, _devices, string.Empty, null, IntPtr.Zero);
                if (error != Cl.ErrorCode.Success)
                    throw new Exception(string.Join("\n", from device in _devices
                                 select Cl.GetProgramBuildInfo(program, device, Cl.ProgramBuildInfo.Log, out error).ToString()));
                (kernel as ICLKernel).ClKernel = Cl.CreateKernel(program, CLCodeGenerator.KernelName, out error);
            }

            return kernel;
        }

        public Kernel<TRange, Set[]> Compile<TRange>(Expression<Func<Brahma.NDRange<TRange>, IEnumerable<Set[]>>> query)
            where TRange : struct, INDRangeDimension
        {
            return CompileQuery<TRange>(query) as Kernel<TRange, Set[]>;
        }

        public Kernel<TRange, T, Set[]> Compile<TRange, T>(Expression<Func<Brahma.NDRange<TRange>, T, IEnumerable<Set[]>>> query)
            where TRange: struct, INDRangeDimension
            where T : IMem
        {
            return CompileQuery<TRange, T>(query) as Kernel<TRange, T, Set[]>;
        }

        public Kernel<TRange, T1, T2, Set[]> Compile<TRange, T1, T2>(Expression<Func<Brahma.NDRange<TRange>, T1, T2, IEnumerable<Set[]>>> query)
            where TRange: struct, INDRangeDimension
            where T1 : IMem
            where T2 : IMem
        {
            return CompileQuery<TRange, T1, T2>(query) as Kernel<TRange, T1, T2, Set[]>;
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