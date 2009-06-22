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

namespace Brahma
{
    // This is the base class of all CompiledQuery handles. For different API's and 
    // computation providers, this will contain different bits of information
    public abstract class CompiledQuery: IDisposable
    {
        private readonly string _id = string.Empty; // An id is essential
        private readonly Type[] _parameterTypes; // The Types of the parameters we need
        private readonly Type _returnType; // The return type of this query

        protected CompiledQuery(Type returnType, params Type[] parameterTypes)
        {
            _id = Guid.NewGuid().ToString();

            if (returnType == null)
                throw new ArgumentNullException("returnType");
            if (parameterTypes == null)
                throw new ArgumentNullException("parameterTypes");
            if (parameterTypes.Length == 0)
                throw new ArgumentException("A query must have at least one parameter");

            _returnType = returnType;
            _parameterTypes = parameterTypes;
        }

        protected internal Type ReturnType
        {
            get
            {
                return _returnType;
            }
        }

        protected internal Type[] ParameterTypes
        {
            get
            {
                return _parameterTypes;
            }
        }

        public string Id
        {
            get
            {
                return _id;
            }
        }

        public bool Disposed
        {
            get;
            private set;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
                DisposeUnmanaged();
            else
            {
                DisposeUnmanaged();
                DisposeManaged();
            }

            Disposed = true;
        }

        protected virtual void DisposeUnmanaged() // Override this to dispose finalizables we control
        {
        }

        protected virtual void DisposeManaged() // Override this to dispose shared resources and other managed resources
        {
        }

        ~CompiledQuery()
        {
            Dispose(false);
        }
    }
}