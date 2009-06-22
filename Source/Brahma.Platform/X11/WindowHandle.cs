using System;
using System.Reflection;
using System.Runtime.InteropServices;

using Tao.Platform.X11;

namespace Brahma.Platform.X11
{
    public sealed class WindowHandle: IWindowHandle, IDisposable
    {
        public WindowHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentException("Invalid window handle");

            Handle = handle;

            Type xplatui = Type.GetType("System.Windows.Forms.XplatUIX11, System.Windows.Forms");
            if (xplatui == null)
                return;

            DisplayHandle = (IntPtr)xplatui.GetField("DisplayHandle", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            var screenNo = (int)xplatui.GetField("ScreenNo", BindingFlags.Static |
                BindingFlags.NonPublic).GetValue(null);
            var dblBuf = new[] { 5, Glx.GLX_RGBA, Glx.GLX_RED_SIZE, 1, Glx.GLX_GREEN_SIZE, 1, Glx.GLX_BLUE_SIZE, 1, Glx.GLX_DEPTH_SIZE, 1, 0 };

            VisualInfoHandle = Glx.glXChooseVisual(handle, screenNo, dblBuf);
        }

        public IntPtr Handle
        {
            get;
            private set;
        }

        public IntPtr DisplayHandle
        {
            get;
            private set;
        }

        public IntPtr VisualInfoHandle
        {
            get;
            private set;
        }

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

        #region IDisposable Members

        public void Dispose()
        {
            Handle = IntPtr.Zero;
            VisualInfoHandle = IntPtr.Zero;
        }

        #endregion

        ~WindowHandle()
        {
            Dispose(); // That's all we need to do here
        }
    }
}
