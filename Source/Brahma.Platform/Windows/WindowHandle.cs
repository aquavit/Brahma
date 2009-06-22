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

using Tao.Platform.Windows;

namespace Brahma.Platform.Windows
{
    public sealed class WindowHandle: IWindowHandle, IDisposable
    {
        private readonly bool _ownDeviceContext;
        private bool _disposed;

        public WindowHandle(IntPtr handle, IntPtr deviceContext)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Invalid window handle");
            Handle = handle;

            if (deviceContext == IntPtr.Zero) // Obtain a device context now
            {
                _ownDeviceContext = true;
                DeviceContext = User.GetDC(handle);
            }
            else
                DeviceContext = deviceContext;
        }

        public WindowHandle(IntPtr handle)
            : this(handle, IntPtr.Zero)
        {
        }

        public IntPtr Handle
        {
            get;
            private set;
        }

        public IntPtr DeviceContext
        {
            get;
            private set;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_ownDeviceContext)
                User.ReleaseDC(Handle, DeviceContext);

            _disposed = true;
        }

        #endregion

        #region IWindowHandle Members

        IntPtr IWindowHandle.Handle
        {
            get
            {
                return Handle;
            }
            set
            {
                Handle = value;
            }
        }

        #endregion

        public override int GetHashCode()
        {
            return Handle.ToInt32() ^ DeviceContext.ToInt32();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is WindowHandle))
                return false;

            var w = obj as WindowHandle;
            return (Handle == w.Handle) && (DeviceContext == w.DeviceContext);
        }

        ~WindowHandle()
        {
            Dispose(); // That's all we need to do here
        }
    }
}