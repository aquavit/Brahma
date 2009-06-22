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

namespace Brahma.OpenGL
{
    internal abstract class ParameterBase<T>
    {
        private readonly int _location = int.MinValue; // The location of this uniform

        internal ParameterBase(int location)
        {
            _location = location; // Save this
        }

        protected int Location
        {
            get
            {
                return _location;
            }
        }

        public virtual T Value
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