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

namespace Brahma.Helper
{
    public static class ComparisonExtensions
    {
        private const float epsilon = 0.000001f;

        public static bool IsCloseTo(this float value1, float value)
        {
            return Math.Abs(value1 - value) <= epsilon;
        }

        public static bool IsCloseTo(this Vector2 actual, Vector2 expected)
        {
            return ((expected.x.IsCloseTo(actual.x)) && (expected.y.IsCloseTo(actual.y)));
        }

        public static bool IsCloseTo(this Vector3 actual, Vector3 expected)
        {
            return ((expected.x.IsCloseTo(actual.x)) && (expected.y.IsCloseTo(actual.y)) && (expected.z.IsCloseTo(actual.z)));
        }

        public static bool IsCloseTo(this Vector4 actual, Vector4 expected)
        {
            return ((expected.x.IsCloseTo(actual.x)) && (expected.y.IsCloseTo(actual.y)) && (expected.z.IsCloseTo(actual.z)) && (expected.w.IsCloseTo(actual.w)));
        }
    }
}