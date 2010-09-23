﻿#region License and Copyright Notice

//Brahma: Framework for streaming/parallel computing with an emphasis on GPGPU

//Copyright (c) 2007 Ananth B.
//All rights reserved.

//The contents of this file are made available under the terms of the
//Eclipse Public License v1.0 (the "License") which accompanies this
//distribution, and is available at the following URL:
//http://www.opensource.org/licenses/eclipse-1.0.php

//Software distributed under the License is distributed on an "AS IS" basis,
//WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
//the specific language governing rights and limitations under the License.

//By using this software in any fashion, you are agreeing to be bound by the
//terms of the License.

#endregion

using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Brahma
{
    public abstract class ComputeProvider: IDisposable
    {
        public abstract Kernel<IEnumerable<TResult>> Compile<TResult>(Expression<Func<IEnumerable<TResult>>> kernel) where TResult: struct;

        public abstract Kernel<T, IEnumerable<TResult>> Compile<T, TResult>(Expression<Func<Buffer<T>, IEnumerable<TResult>>> kernel) where T: struct 
                                                                                                                 where TResult: struct;

        public abstract Kernel<T1, T2, IEnumerable<TResult>> Compile<T1, T2, TResult>(Expression<Func<Buffer<T1>, Buffer<T2>, IEnumerable<TResult>>> kernel) where T1: struct 
                                                                                                                                where T2: struct 
                                                                                                                                where TResult: struct;

        public abstract void Dispose();
    }
}