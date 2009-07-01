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

using Brahma.Helper;

using NUnit.Framework;

namespace Brahma.OpenGL.Tests.Helper
{
    internal static class AssertHelper
    {
        public static void AssertIfClose(this float actual, float expected)
        {
            if (!expected.IsCloseTo(actual))
                Assert.Fail(string.Format("Value was supposed to be ~ {0}, but was {1}", expected, actual));
        }

        public static void AssertIfClose(this Vector2 actual, Vector2 expected)
        {
            if (!actual.IsCloseTo(expected))
                Assert.Fail(string.Format("Value was supposed to be ~ {0}, but was {1}", expected, actual));
        }

        public static void AssertIfClose(this Vector3 actual, Vector3 expected)
        {
            if (!actual.IsCloseTo(expected))
                Assert.Fail(string.Format("Value was supposed to be ~ {0}, but was {1}", expected, actual));
        }

        public static void AssertIfClose(this Vector4 actual, Vector4 expected)
        {
            if (!actual.IsCloseTo(expected))
                Assert.Fail(string.Format("Value was supposed to be ~ {0}, but was {1}", expected, actual));
        }
    }
}