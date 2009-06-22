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

using Brahma.Platform.OpenGL;

namespace Brahma.OpenGL
{
    internal abstract class ShaderBase: IDisposable
    {
        private readonly string _source;
        private bool _disposed;

        protected ShaderBase(ContextBase context, string source)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (source == null)
                throw new ArgumentNullException("source");

            Context = context;
            _source = source;
        }

        protected string Source
        {
            get
            {
                return _source;
            }
        }

        internal ContextBase Context
        {
            get;
            set;
        }

        public bool Disposed
        {
            get
            {
                return _disposed;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        internal abstract CompileResult Compile();

        private void Dispose(bool disposing)
        {
            if (_disposed) 
                return;

            if (disposing)
                DisposeUnmanaged();
            else
            {
                DisposeUnmanaged();
                DisposeManaged();
            }

            _disposed = true;
        }

        protected virtual void DisposeUnmanaged() // Override this to dispose finalizables we control
        {
        }

        protected virtual void DisposeManaged() // Override this to dispose shared resources and other managed resources
        {
        }

        ~ShaderBase()
        {
            Dispose(false);
        }
    }

    internal struct CompileResult
    {
        private readonly string _messages;
        private readonly bool _success;

        internal CompileResult(bool success)
        {
            _success = success;
            _messages = string.Empty;
        }

        internal CompileResult(bool success, string messages)
        {
            _success = success;

            if (messages == null)
                throw new ArgumentNullException("messages");
            _messages = messages;
        }

        public bool Success
        {
            get
            {
                return _success;
            }
        }

        public string Messages
        {
            get
            {
                return _messages;
            }
        }

        public static explicit operator bool(CompileResult operand)
        {
            return operand.Success;
        }
    }
}