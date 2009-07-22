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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Brahma
{
    public enum AddressingMode
    {
        Repeat,
        Clamp
    }

    // Base class of all data-parallel arrays, is enumerable and queryable
    // Remember, IQueryable derives from IEnumerable so we don't need to put IEnumerable here
    public abstract class DataParallelArrayBase: IQueryable, IAddressable, IDisposable
    {
        private readonly Expression _expression;
        private readonly ComputationProviderBase _provider;

        public static readonly Type[] AllowedTypes = new[] { typeof(float), typeof(Vector2), typeof(Vector3), typeof(Vector4) };

        protected DataParallelArrayBase(ComputationProviderBase provider, Expression expression)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            _provider = provider;

            _expression = expression ?? Expression.Constant(this);
        }

        public bool Disposed
        {
            get;
            private set;
        }

        public abstract int Rank
        {
            get;
        }

        #region IAddressable Members

        AddressingMode IAddressable.ColumnAddressingMode
        {
            get
            {
                return GetColumnAddressingMode();
            }
        }

        AddressingMode IAddressable.RowAddressingMode
        {
            get
            {
                return GetRowAddressingMode();
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IQueryable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return EnumeratorGetter(); // Return the enumerator DataParallelArray<T> gave us, we don't have our own
        }

        public abstract Type ElementType
        {
            get;
        }

        Expression IQueryable.Expression
        {
            get
            {
                return _expression;
            }
        }

        public IQueryProvider Provider
        {
            get
            {
                return _provider;
            }
        }

        #endregion

        protected event Func<IEnumerator> EnumeratorGetter; // This allows DataParallelArray<T> to implement the iterator

        protected abstract AddressingMode GetColumnAddressingMode();
        protected abstract AddressingMode GetRowAddressingMode();

        // Override this to perform any initializations before a query is run using this as an argument
        // Remember, this is NOT called on the result of a query, only the arguments
        protected internal virtual void BeginQuery()
        {
        }

        // Override this to perform any initializations after a query is run using this as an argument
        // Remember, this is NOT called on the result of a query, only the arguments
        protected internal virtual void EndQuery()
        {
        }

        private void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
            {
                DisposeManaged();
                DisposeUnmanaged();
            }
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

        ~DataParallelArrayBase()
        {
            Dispose(false);
        }

        public abstract int GetLength(int dimension);
    }

    // Generic version of DataParallelArrayBase
    public abstract class DataParallelArrayBase<T>: DataParallelArrayBase, IQueryable<T> where T : struct
    {
        protected DataParallelArrayBase(ComputationProviderBase provider, Expression expression)
            : base(provider, expression)
        {
            EnumeratorGetter += GetEnumerator; // Tell the base class to use this function to generate an IEnumerator
        }

        #region IQueryable<T> Members

        public abstract IEnumerator<T> GetEnumerator(); // Implement this method in derived classes

        public override Type ElementType
        {
            get
            {
                return typeof (T);
            }
        }

        #endregion
    }
}