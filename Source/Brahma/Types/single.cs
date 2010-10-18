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

namespace Brahma.Types
{
    public struct single: IMem
    {
        private float _value;

        public static implicit operator single(float value)
        {
            return new single() { _value = value };
        }

        public static explicit operator single(int value)
        {
            return new single() { _value = value };
        }

        public override bool Equals(object obj)
        {
            return obj is single ? ((single)obj)._value == _value : false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static single operator /(single a, single b)
        {
            return new single { _value = a._value / b._value };
        }

        public static single operator *(single a, single b)
        {
            return new single { _value = a._value * b._value };
        }

        public static single operator +(single a, single b)
        {
            return new single { _value = a._value + b._value };
        }

        public static single operator -(single a, single b)
        {
            return new single { _value = a._value - b._value };
        }

        public static Set<single> operator <=(single lhs, single rhs)
        {
            return new Set<single>(lhs, rhs);
        }

        public static Set<single> operator >=(single lhs, single rhs)
        {
            throw new NotSupportedException();
        }

        public static bool operator ==(single lhs, single rhs)
        {
            return lhs._value == rhs._value;
        }
        
        public static bool operator !=(single lhs, single rhs)
        {
            return lhs._value != rhs._value;
        }

        IntPtr IMem.Size
        {
            get
            {
                return (IntPtr)sizeof(float);
            }
        }

        object IMem.Data
        {
            get
            {
                return _value;
            }
        }
    }
}
