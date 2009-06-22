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
using System.Collections.Generic;
using System.Windows.Forms;

namespace Brahma.Platform.OpenGL
{
    internal static class ContextFactory
    {
        private static readonly Dictionary<Platform.WindowManager, Type> _implementations =
            new Dictionary<Platform.WindowManager, Type>
                {
                    { Platform.WindowManager.Windows, typeof (Windows.GLContext) },
                    { Platform.WindowManager.X11, typeof (X11.GLContext) }
                };

        public static ContextBase CreateContext()
        {
            if (!_implementations.ContainsKey(Platform.WindowingManager))
                throw new NotSupportedException(
                    "The platform you're running on is not supported. Please send this error message along with your platform details to ananth<at>ananthonline<dot>net");

            return (ContextBase)Activator.CreateInstance(_implementations[Platform.WindowingManager]);
        }

        public static ContextBase CreateContext(Control control)
        {
            if (!_implementations.ContainsKey(Platform.WindowingManager))
                throw new NotSupportedException(
                    "The platform you're running on is not supported. Please send this error message along with your platform details to ananth<at>ananthonline<dot>net");

            return (ContextBase)Activator.CreateInstance(_implementations[Platform.WindowingManager], new object[] { control });
        }

        public static ContextBase CreateContext(Control control, int colorBits, int depthBits, int stencilBits)
        {
            if (!_implementations.ContainsKey(Platform.WindowingManager))
                throw new NotSupportedException(
                    "The platform you're running on is not supported. Please send this error message along with your platform details to ananth<at>ananthonline<dot>net");

            return (ContextBase)Activator.CreateInstance(_implementations[Platform.WindowingManager], new object[] { control, colorBits, depthBits, stencilBits });
        }

        public static ContextBase CreateContext(IntPtr renderingContext)
        {
            if (!_implementations.ContainsKey(Platform.WindowingManager))
                throw new NotSupportedException(
                    "The platform you're running on is not supported. Please send this error message along with your platform details to ananth<at>ananthonline<dot>net");

            return (ContextBase)Activator.CreateInstance(_implementations[Platform.WindowingManager], new object[] { renderingContext });
        }
    }
}