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
    [Flags]
    public enum CompileOptions
    {
        UseNativeFunctions,
        FastRelaxedMath,
        FusedMultiplyAdd,
        DisableOptimizations,
        StrictAliasing,
        NoSignedZeros,
        UnsafeMathOptimizations,
        FiniteMathOnly
    }
    
    public sealed class ComputeProvider: Brahma.ComputeProvider
    {
        private const CompileOptions DefaultOptions = CompileOptions.UseNativeFunctions | CompileOptions.FusedMultiplyAdd | CompileOptions.FastRelaxedMath;
        
        private readonly Cl.Context _context;
        private readonly Cl.Device[] _devices;
        private bool _disposed;
        private string _compileOptions = string.Empty;

        private void SetCompileOptions(CompileOptions options)
        {
            CompileOptions = options;
            
            _compileOptions = string.Empty;

            // UseNativeFunctions = ((options & CompileOptions.UseNativeFunctions) == CompileOptions.UseNativeFunctions);
            _compileOptions += ((options & CompileOptions.FastRelaxedMath) == CompileOptions.FastRelaxedMath ? " -cl-fast-relaxed-math " : string.Empty);
            _compileOptions += ((options & CompileOptions.FusedMultiplyAdd) == CompileOptions.FusedMultiplyAdd ? " -cl-mad-enable " : string.Empty);
            _compileOptions += ((options & CompileOptions.DisableOptimizations) == CompileOptions.DisableOptimizations ? " -cl-opt-disable " : string.Empty);
            _compileOptions += ((options & CompileOptions.StrictAliasing) == CompileOptions.StrictAliasing ? " -cl-strict-aliasing " : string.Empty);
            _compileOptions += ((options & CompileOptions.NoSignedZeros) == CompileOptions.NoSignedZeros ? " -cl-no-signed-zeros " : string.Empty);
            _compileOptions += ((options & CompileOptions.UnsafeMathOptimizations) == CompileOptions.UnsafeMathOptimizations ? " -cl-unsafe-math-optimizations " : string.Empty);
            _compileOptions += ((options & CompileOptions.FiniteMathOnly) == CompileOptions.FiniteMathOnly ? " -cl-finite-math-only " : string.Empty);
        }

        internal CompileOptions CompileOptions
        {
            get;
            private set;
        }

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
            if (devices.Length == 0)
                throw new ArgumentException("Need at least one device!");
            
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
            lambda.GenerateKernel(this, kernel);

            Cl.ErrorCode error;
            using (Cl.Program program = Cl.CreateProgramWithSource(_context, 1, new[] { (kernel as ICLKernel).Source.ToString() }, null, out error))
            {
                error = Cl.BuildProgram(program, (uint)_devices.Length, _devices, _compileOptions, null, IntPtr.Zero);
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
            lambda.GenerateKernel(this, kernel);

            Cl.ErrorCode error;
            using (Cl.Program program = Cl.CreateProgramWithSource(_context, 1, new[] { (kernel as ICLKernel).Source.ToString() }, null, out error))
            {
                error = Cl.BuildProgram(program, (uint)_devices.Length, _devices, _compileOptions, null, IntPtr.Zero);
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
            lambda.GenerateKernel(this, kernel);

            Cl.ErrorCode error;
            using (Cl.Program program = Cl.CreateProgramWithSource(_context, 1, new[] { (kernel as ICLKernel).Source.ToString() }, null, out error))
            {
                error = Cl.BuildProgram(program, (uint)_devices.Length, _devices, _compileOptions, null, IntPtr.Zero);
                if (error != Cl.ErrorCode.Success)
                    throw new Exception(string.Join("\n", from device in _devices
                                 select Cl.GetProgramBuildInfo(program, device, Cl.ProgramBuildInfo.Log, out error).ToString()));
                (kernel as ICLKernel).ClKernel = Cl.CreateKernel(program, CLCodeGenerator.KernelName, out error);
            }

            return kernel;
        }

        protected override Brahma.Kernel<TRange, T1, T2, T3, Set[]> CompileQuery<TRange, T1, T2, T3>(Expression<Func<Brahma.NDRange<TRange>, T1, T2, T3, IEnumerable<Set[]>>> query)
        {
            var lambda = query as LambdaExpression;
            var kernel = new Kernel<TRange, T1, T2, T3, Set[]>();
            lambda.GenerateKernel(this, kernel);

            Cl.ErrorCode error;
            using (Cl.Program program = Cl.CreateProgramWithSource(_context, 1, new[] { (kernel as ICLKernel).Source.ToString() }, null, out error))
            {
                error = Cl.BuildProgram(program, (uint)_devices.Length, _devices, _compileOptions, null, IntPtr.Zero);
                if (error != Cl.ErrorCode.Success)
                    throw new Exception(string.Join("\n", from device in _devices
                                                          select Cl.GetProgramBuildInfo(program, device, Cl.ProgramBuildInfo.Log, out error).ToString()));
                (kernel as ICLKernel).ClKernel = Cl.CreateKernel(program, CLCodeGenerator.KernelName, out error);
            }

            return kernel;
        }

        protected override Brahma.Kernel<TRange, T1, T2, T3, T4, Set[]> CompileQuery<TRange, T1, T2, T3, T4>(Expression<Func<Brahma.NDRange<TRange>, T1, T2, T3, T4, IEnumerable<Set[]>>> query)
        {
            var lambda = query as LambdaExpression;
            var kernel = new Kernel<TRange, T1, T2, T3, T4, Set[]>();
            lambda.GenerateKernel(this, kernel);

            Cl.ErrorCode error;
            using (Cl.Program program = Cl.CreateProgramWithSource(_context, 1, new[] { (kernel as ICLKernel).Source.ToString() }, null, out error))
            {
                error = Cl.BuildProgram(program, (uint)_devices.Length, _devices, _compileOptions, null, IntPtr.Zero);
                if (error != Cl.ErrorCode.Success)
                    throw new Exception(string.Join("\n", from device in _devices
                                                          select Cl.GetProgramBuildInfo(program, device, Cl.ProgramBuildInfo.Log, out error).ToString()));
                (kernel as ICLKernel).ClKernel = Cl.CreateKernel(program, CLCodeGenerator.KernelName, out error);
            }

            return kernel;
        }

        public Kernel<TRange, Set[]> Compile<TRange>(Expression<Func<Brahma.NDRange<TRange>, IEnumerable<Set[]>>> query, CompileOptions options = DefaultOptions)
            where TRange : struct, INDRangeDimension
        {
            SetCompileOptions(options);
            return CompileQuery(query) as Kernel<TRange, Set[]>;
        }

        public Kernel<TRange, T, Set[]> Compile<TRange, T>(Expression<Func<Brahma.NDRange<TRange>, T, IEnumerable<Set[]>>> query, CompileOptions options = DefaultOptions)
            where TRange: struct, INDRangeDimension
            where T : IMem
        {
            SetCompileOptions(options);
            return CompileQuery(query) as Kernel<TRange, T, Set[]>;
        }

        public Kernel<TRange, T1, T2, Set[]> Compile<TRange, T1, T2>(Expression<Func<Brahma.NDRange<TRange>, T1, T2, IEnumerable<Set[]>>> query, CompileOptions options = DefaultOptions)
            where TRange: struct, INDRangeDimension
            where T1 : IMem
            where T2 : IMem
        {
            SetCompileOptions(options);
            return CompileQuery(query) as Kernel<TRange, T1, T2, Set[]>;
        }

        public Kernel<TRange, T1, T2, T3, Set[]> Compile<TRange, T1, T2, T3>(Expression<Func<Brahma.NDRange<TRange>, T1, T2, T3, IEnumerable<Set[]>>> query, CompileOptions options = DefaultOptions)
            where TRange : struct, INDRangeDimension
            where T1 : IMem
            where T2 : IMem
            where T3: IMem
        {
            SetCompileOptions(options);
            return CompileQuery(query) as Kernel<TRange, T1, T2, T3, Set[]>;
        }

        public Kernel<TRange, T1, T2, T3, T4, Set[]> Compile<TRange, T1, T2, T3, T4>(Expression<Func<Brahma.NDRange<TRange>, T1, T2, T3, T4, IEnumerable<Set[]>>> query, CompileOptions options = DefaultOptions)
            where TRange : struct, INDRangeDimension
            where T1 : IMem
            where T2 : IMem
            where T3 : IMem
            where T4: IMem
        {
            SetCompileOptions(options);
            return CompileQuery(query) as Kernel<TRange, T1, T2, T3, T4, Set[]>;
        }

        [KernelCallable]
        public Func<int, IEnumerable<Set[]>> Loop(int startValue, int count, Func<IEnumerable<int>, IEnumerable<Set[]>> body)
        {
            throw new NotSupportedException("Cannot call this method from code, only inside a kernel");
        }

        [KernelCallable]
        public Func<int, IEnumerable<Set[]>> Loop(int startValue, int count, Func<int, IEnumerable<Set>> body)
        {
            throw new NotSupportedException("Cannot call this method from code, only inside a kernel");
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _context.Dispose();
                _disposed = true;
            }
        }

        public IEnumerable<Cl.Device> Devices
        {
            get
            {
                for (int i = 0; i < _devices.Length; i++)
                    yield return _devices[i];
            }
        }
    }
}