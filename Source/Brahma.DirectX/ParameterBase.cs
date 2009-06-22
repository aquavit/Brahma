#region License and Copyright Notice

//Brahma 2.0: Framework for streaming/parallel computing with an emphasis on GPGPU

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
using System.Runtime.Serialization;

using Microsoft.DirectX.Direct3D;

namespace Brahma.DirectX
{
    // Base class for all shader parameters
    internal abstract class ParameterBase<T>
    {
        private readonly EffectHandle _effectHandle;
        private readonly DXCompiledQuery _query;

        protected ParameterBase(DXCompiledQuery query, EffectHandle effectHandle)
        {
            // query AND effectHandle can be null in some cases.

            _query = query; // We need the query to access the ConstantTable and the Device
            _effectHandle = effectHandle; // We need this, of course!
        }

        protected DXCompiledQuery Query
        {
            get
            {
                return _query;
            }
        }

        protected EffectHandle EffectHandle
        {
            get
            {
                return _effectHandle;
            }
        }

        // Derived sealed generic classes will implement this to ACTUALLY set the value
        // See Parameters.cs
        public abstract T Value
        {
            get;
            set;
        }
    }

    [Serializable]
    public class ParameterException: Exception
    {
        public ParameterException()
        {
        }

        public ParameterException(string message)
            : base(message)
        {
        }

        public ParameterException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ParameterException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}