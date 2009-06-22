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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Brahma
{
    [DebuggerDisplay("X={x} Y={y} Z={z} W={w}")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4
    {
        public static readonly Vector4 Zero = new Vector4(0f, 0f, 0f, 1f);
        public static readonly ConstructorInfo DefaultConstructor = typeof (Vector4).GetConstructor(Type.EmptyTypes);
        public static readonly ConstructorInfo ExplicitConstructor = typeof (Vector4).GetConstructor(new[] { typeof (float), typeof (float), typeof (float), typeof (float) });

        public Vector4(float x, float y, float z, float w)
            : this()
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public float x
        {
            get;
            set;
        }

        public float y
        {
            get;
            set;
        }

        public float z
        {
            get;
            set;
        }

        public float w
        {
            get;
            set;
        }

        public static Vector4 operator +(Vector4 operand1, Vector4 operand2)
        {
            return new Vector4(operand1.x + operand2.x,
                               operand1.y + operand2.y,
                               operand1.z + operand2.z,
                               operand1.w + operand2.w);
        }

        public static Vector4 operator -(Vector4 operand1, Vector4 operand2)
        {
            return new Vector4(operand1.x - operand2.x,
                               operand1.y - operand2.y,
                               operand1.z - operand2.z,
                               operand1.w - operand2.w);
        }

        public static Vector4 operator /(Vector4 operand, float value)
        {
            return new Vector4(operand.x / value,
                               operand.y / value,
                               operand.z / value,
                               operand.w / value);
        }

        public static Vector4 operator *(float value, Vector4 operand)
        {
            return new Vector4(value * operand.x,
                               value * operand.y,
                               value * operand.z,
                               value * operand.w);
        }

        public static Vector4 operator *(Vector4 operand, float value)
        {
            return new Vector4(value * operand.x,
                               value * operand.y,
                               value * operand.z,
                               value * operand.w);
        }

        public static Vector4 operator *(Vector4 operand1, Vector4 operand2)
        {
            return new Vector4(operand1.x * operand2.x, operand1.y * operand2.y, operand1.z * operand2.z, operand1.w * operand2.w);
        }

        public static float Length(Vector4 v)
        {
            return (float)Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }

        public override string ToString()
        {
            return string.Format("X={0:F4}, Y={1:F4}, Z={2:F4}, W={3:F4}", x, y, z, w);
        }
    }
}