﻿#region License and Copyright Notice
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

namespace Brahma.Commands
{
    public abstract class Run<TRange, TResult> : Command<TResult> where TRange: struct, INDRangeDimension
    {
        protected Run(Kernel<TRange, TResult> kernel)
        {
        }
    }

    public abstract class Run<TRange, T, TResult> : Command<T, TResult> where T: IMem where TRange: struct, INDRangeDimension
    {
        protected Run(Kernel<TRange, T, TResult> kernel, T data)
        { 
        }
    }

    public abstract class Run<TRange, T1, T2, TResult> : Command<T1, T2, TResult> where T1: IMem where T2: IMem where TRange: struct, INDRangeDimension
    {
        protected Run(Kernel<TRange, T1, T2, TResult> kernel, T1 d1, T2 d2)
        {
        }
    }
}