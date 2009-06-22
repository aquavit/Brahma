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
    [DebuggerDisplay("X={x} Y={y}")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector2
    {
        public static readonly ConstructorInfo DefaultConstructor = typeof (Vector2).GetConstructor(Type.EmptyTypes);
        public static readonly ConstructorInfo ExplicitConstructor = typeof (Vector2).GetConstructor(new[] { typeof (float), typeof (float) });

        public static readonly Vector2 Zero = new Vector2(0f, 0f);

        public Vector2(float x, float y)
            : this()
        {
            this.x = x;
            this.y = y;
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

        public static Vector2 operator +(Vector2 operand1, Vector2 operand2)
        {
            return new Vector2(operand1.x + operand2.x,
                               operand1.y + operand2.y);
        }

        public static Vector2 operator -(Vector2 operand1, Vector2 operand2)
        {
            return new Vector2(operand1.x - operand2.x,
                               operand1.y - operand2.y);
        }

        public static Vector2 operator /(Vector2 operand, float value)
        {
            return new Vector2(operand.x / value,
                               operand.y / value);
        }

        public static Vector2 operator *(float value, Vector2 operand)
        {
            return new Vector2(value * operand.x,
                               value * operand.y);
        }

        public static Vector2 operator *(Vector2 operand, float value)
        {
            return new Vector2(value * operand.x,
                               value * operand.y);
        }

        public static Vector2 operator *(Vector2 operand1, Vector2 operand2)
        {
            return new Vector2(operand1.x * operand2.x, operand1.y * operand2.y);
        }

        public static float Length(Vector2 v)
        {
            return (float)Math.Sqrt(v.x * v.x + v.y * v.y);
        }

        public override string ToString()
        {
            return string.Format("X={0:F4}, Y={1:F4}", x, y);
        }
    }
}