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
    public struct int32: IMem
    {
        private int _value;

        public static implicit operator int32(int value)
        {
            return new int32 { _value = value };
        }

        public override bool Equals(object obj)
        {
            return obj is int32 ? ((int32)obj)._value == _value : false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static int32 operator +(int32 a, int32 b)
        {
            return new int32 { _value = a._value + b._value };
        }

        public static int32 operator -(int32 a, int32 b)
        {
            return new int32 { _value = a._value - b._value };
        }

        public static int32 operator /(int32 a, int32 b)
        {
            return new int32
            {
                _value = a._value / b._value
            };
        }

        public static int32 operator *(int32 a, int32 b)
        {
            return new int32
            {
                _value = a._value * b._value
            };
        }
        public static Set<int32> operator <=(int32 lhs, int32 rhs)
        {
            return new Set<int32>(lhs, rhs);
        }

        public static Set<int32> operator >=(int32 lhs, int32 rhs)
        {
            throw new NotSupportedException();
        }

        IntPtr IMem.Size
        {
            get
            {
                return (IntPtr)sizeof(int);
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
