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
using System.Windows.Forms;

using Brahma.Platform.X11;

using Tao.Platform.X11;

namespace Brahma.Platform.OpenGL.X11
{
    internal sealed class GLContext: ContextBase
    {
        private readonly Control _control;
        private readonly bool _ownContext;
        private readonly bool _ownWindow;
        private readonly IntPtr _renderingContext;
        private readonly WindowHandle _windowHandle;
        private bool _disposed;

        public GLContext() // We've not been given anything, we create a Window and the context
            : this(new Form())
        {
            _ownWindow = true; // Remember if we're creating the window and/or the context
            _ownContext = true;
        }

        public GLContext(Control control)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            _control = control; // Keep a reference to it, just in case
        }

        // TODO: Find out how we can specify the color, depth and stencil bits
        public GLContext(Control control, int colorBits, int depthBits, int stencilBits) // We're ignoring the color, depth and stencil bits for now
        {
            _ownContext = true; // Remember if we're creating the window and/or the context

            _windowHandle = new WindowHandle(control.Handle);
            if (_windowHandle.VisualInfoHandle == IntPtr.Zero)
                throw new ContextException("Cannot initialize OpenGL, could not get a handle to VisualInfo");

            _renderingContext = Glx.glXCreateContext(_windowHandle.DisplayHandle, _windowHandle.VisualInfoHandle, IntPtr.Zero, true);
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

            if (_ownContext)
                Glx.glXDestroyContext(_windowHandle.DisplayHandle, _renderingContext);
            
            _windowHandle.Dispose();

            if (_ownWindow)
                _control.Dispose(); // Dispose the control we're on

            _disposed = true;

            base.DisposeUnmanaged();
        }

        public override void SwapBuffers()
        {
            Glx.glXSwapBuffers(_windowHandle.DisplayHandle, _control.Handle);
        }

        public override void MakeCurrent()
        {
            Glx.glXMakeCurrent(_windowHandle.DisplayHandle, _control.Handle, _renderingContext);
        }

        public override bool IsCurrent
        {
            get
            {
                return Glx.glXGetCurrentContext() == _renderingContext;
            }
        }
    }
}