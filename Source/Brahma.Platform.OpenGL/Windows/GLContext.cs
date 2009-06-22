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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Brahma.Platform.Windows;

using Tao.Platform.Windows;

namespace Brahma.Platform.OpenGL.Windows
{
    internal sealed class GLContext: ContextBase
    {
        private const int DefaultColorBits = 32;
        private const int DefaultDepthBits = 0;
        private const int DefaultStencilBits = 0;

        private readonly Control _control;
        private readonly bool _ownContext;
        private readonly bool _ownWindow;
        private readonly IntPtr _renderingContext;
        private readonly WindowHandle _windowHandle;
        private bool _disposed;

        public GLContext() // We've not been given anything, we create Window and the context
            : this(new Form())
        {
            _ownWindow = true; // Remember if we're creating the window and/or the context
            _ownContext = true;
        }

        public GLContext(Control control) // We've been given a control, we just need to create a context
            : this(control, DefaultColorBits, DefaultDepthBits, DefaultStencilBits)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            _control = control; // Keep a reference to it, just in case
        }

        public GLContext(Control control, int colorBits, int depthBits, int stencilBits)
        {
            _ownContext = true; // Remember if we're creating the window and/or the context

            _windowHandle = new WindowHandle(control.Handle);

            try
            {
                var pfd = new Gdi.PIXELFORMATDESCRIPTOR();

                pfd.nSize = (short)Marshal.SizeOf(pfd);
                pfd.nVersion = 1;

                pfd.dwFlags = Gdi.PFD_DRAW_TO_WINDOW | Gdi.PFD_SUPPORT_OPENGL | Gdi.PFD_DOUBLEBUFFER;

                pfd.iPixelType = Gdi.PFD_TYPE_RGBA;
                pfd.cColorBits = (byte)colorBits;
                pfd.cRedBits = 0;
                pfd.cRedShift = 0;
                pfd.cGreenBits = 0;
                pfd.cGreenShift = 0;
                pfd.cBlueBits = 0;
                pfd.cBlueShift = 0;
                pfd.cAlphaBits = 0;
                pfd.cAlphaShift = 0;
                pfd.cAccumBits = 0;
                pfd.cAccumRedBits = 0;
                pfd.cAccumGreenBits = 0;
                pfd.cAccumBlueBits = 0;
                pfd.cAccumAlphaBits = 0;
                pfd.cDepthBits = (byte)depthBits;
                pfd.cStencilBits = (byte)stencilBits;
                pfd.cAuxBuffers = 0;
                pfd.iLayerType = Gdi.PFD_MAIN_PLANE;
                pfd.bReserved = 0;
                pfd.dwLayerMask = 0;
                pfd.dwVisibleMask = 0;
                pfd.dwDamageMask = 0;

                int pixelformatIndex = Gdi.ChoosePixelFormat(_windowHandle.DeviceContext, ref pfd);
                if (pixelformatIndex == 0)
                    throw new ContextException(string.Format(CultureInfo.InvariantCulture, "No pixelformat matching {0} colorbits, {1} depthbits and {2} stencil bits was found on the current video/driver combination", colorBits, depthBits, stencilBits));

                if (!Gdi.SetPixelFormat(_windowHandle.DeviceContext, pixelformatIndex, ref pfd))
                    throw new ContextException(string.Format(CultureInfo.InvariantCulture, "Could not set pixelformat index {0} as the current pixelformat for supplied device context", pixelformatIndex));

                _renderingContext = Wgl.wglCreateContext(_windowHandle.DeviceContext);
                if (_renderingContext == IntPtr.Zero)
                    throw new ContextException("Could not create rendering context, wglCreateContext failed");
            }
            catch (Exception ex)
            {
                throw new ContextException("Unable to acquire context", ex);
            }
        }

        public GLContext(IntPtr renderingContext) // We don't need to create anything, we just use the context we've been given.
        {
            if (renderingContext == IntPtr.Zero)
                throw new ArgumentException("Invalid or null rendering context provided");

            _renderingContext = renderingContext;
        }

        protected override void DisposeUnmanaged()
        {
            if (_disposed)
                return;

            base.DisposeUnmanaged(); // Dispose all the resources we need to dispose first

            _windowHandle.Dispose();

            if (_ownContext)
                Wgl.wglDeleteContext(_renderingContext);

            if (_ownWindow)
                _control.Dispose(); // Dispose the control we're on

            _disposed = true;
        }

        public override void SwapBuffers()
        {
            Gdi.SwapBuffers(_windowHandle.DeviceContext);
        }

        public override void MakeCurrent()
        {
            Wgl.wglMakeCurrent(_windowHandle.DeviceContext, _renderingContext);
        }
        
        public override bool IsCurrent
        {
            get
            {
                return Wgl.wglGetCurrentContext() == _renderingContext;
            }
        }

    }
}