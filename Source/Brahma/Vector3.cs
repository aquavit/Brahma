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
    [DebuggerDisplay("X={x} Y={y} Z={z}")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3
    {
        public static readonly Vector3 Zero = new Vector3(0f, 0f, 0f);
        public static readonly ConstructorInfo DefaultConstructor = typeof (Vector3).GetConstructor(Type.EmptyTypes);
        public static readonly ConstructorInfo ExplicitConstructor = typeof (Vector3).GetConstructor(new[] { typeof (float), typeof (float), typeof (float) });

        public Vector3(float x, float y, float z)
            : this()
        {
            this.x = x;
            this.y = y;
            this.z = z;
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

        public static Vector3 operator +(Vector3 operand1, Vector3 operand2)
        {
            return new Vector3(operand1.x + operand2.x,
                               operand1.y + operand2.y,
                               operand1.z + operand2.z);
        }

        public static Vector3 operator -(Vector3 operand1, Vector3 operand2)
        {
            return new Vector3(operand1.x - operand2.x,
                               operand1.y - operand2.y,
                               operand1.z - operand2.z);
        }

        public static Vector3 operator /(Vector3 operand, float value)
        {
            return new Vector3(operand.x / value,
                               operand.y / value,
                               operand.z / value);
        }

        public static Vector3 operator *(float value, Vector3 operand)
        {
            return new Vector3(value * operand.x,
                               value * operand.y,
                               value * operand.z);
        }

        public static Vector3 operator *(Vector3 operand, float value)
        {
            return new Vector3(value * operand.x,
                               value * operand.y,
                               value * operand.z);
        }

        public static Vector3 operator *(Vector3 operand1, Vector3 operand2)
        {
            return new Vector3(operand1.x * operand2.x, operand1.y * operand2.y, operand1.z * operand2.z);
        }

        public static float Length(Vector3 v)
        {
            return (float)Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }

        public override string ToString()
        {
            return string.Format("X={0:F4}, Y={1:F4}, Z={2:F4}", x, y, z);
        }
    }
}