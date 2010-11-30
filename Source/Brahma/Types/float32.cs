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
    public struct float32: IMem
    {
        private float _value;

        public static implicit operator float32(float value)
        {
            return new float32 { _value = value };
        }

        public static explicit operator float32(int value)
        {
            return new float32 { _value = value };
        }

        public override bool Equals(object obj)
        {
            return obj is float32 ? ((float32)obj)._value == _value : false;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static float32 operator /(float32 a, float32 b)
        {
            return new float32 { _value = a._value / b._value };
        }

        public static float32 operator *(float32 a, float32 b)
        {
            return new float32 { _value = a._value * b._value };
        }

        public static float32 operator +(float32 a, float32 b)
        {
            return new float32 { _value = a._value + b._value };
        }

        public static float32 operator -(float32 a, float32 b)
        {
            return new float32 { _value = a._value - b._value };
        }

        public static Set<float32> operator <=(float32 lhs, float32 rhs)
        {
            return new Set<float32>(lhs, rhs);
        }

        public static Set<float32> operator >=(float32 lhs, float32 rhs)
        {
            throw new NotSupportedException();
        }

        public static bool operator ==(float32 lhs, float32 rhs)
        {
            return lhs._value == rhs._value;
        }
        
        public static bool operator !=(float32 lhs, float32 rhs)
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
