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
using System.Linq;
using System.Linq.Expressions;

namespace Brahma
{
    public abstract class ComputationProviderBase: IQueryProvider, IDisposable
    {
        // This is called from Run because we need to let arguments "initialize" and "de-initialize" themselves before and after execution
        // This method internally calls Run. Remember, derived classes only need to override RunQuery.
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

        #region IQueryProvider Members

        [Obsolete("Brahma does not support imperative queries anymore. Use compiled queries instead.", true)]
        IQueryable<S> IQueryProvider.CreateQuery<S>(Expression expression)
        {
            return (IQueryable<S>)CreateQuery(expression);
        }

        [Obsolete("Brahma does not support imperative queries anymore. Use compiled queries instead.", true)]
        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            return CreateQuery(expression);
        }

        S IQueryProvider.Execute<S>(Expression expression)
        {
            return (S)Execute(expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute(expression);
        }

        #endregion

        private IQueryable InitializeAndRunQuery(CompiledQuery query, params DataParallelArrayBase[] arguments)
        {
            foreach (DataParallelArrayBase argument in arguments)
                argument.BeginQuery(); // Call this to let the data-parallel array initialize itself before a query is run on it

            IQueryable result = RunQuery(query, arguments); // Run the query

            foreach (DataParallelArrayBase argument in arguments)
                argument.EndQuery(); // Call this to let the data-parallel array perform cleanup on itself after a query is run on it

            return result;
        }

        protected abstract object Execute(Expression expression); // I don't know what the heck this is for!

        // Brahma does not support imperative queries anymore.
        [Obsolete("Brahma does not support imperative queries anymore. Use compiled queries instead.", true)]
        protected virtual IQueryable CreateQuery(Expression expression)
        {
            throw new NotSupportedException("Brahma does not support imperative queries anymore. Use compiled queries instead.");
        }

        // Override this method in derived classes to create a CompiledQuery
        protected abstract CompiledQuery CompileQuery(LambdaExpression expression);

        // Override this method in derived classes to run a CompiledQuery
        protected abstract IQueryable RunQuery(CompiledQuery query, params DataParallelArrayBase[] arguments);

        private void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                    DisposeUnmanaged();
                else
                {
                    DisposeUnmanaged();
                    DisposeManaged();
                }

                Disposed = true;
            }
        }

        protected virtual void DisposeUnmanaged() // Override this to dispose finalizables we control
        {
        }

        protected virtual void DisposeManaged() // Override this to dispose shared resources and other managed resources
        {
        }

        ~ComputationProviderBase()
        {
            Dispose(false);
        }

        // All the overloads that compile a query into a CompiledQuery. Supports upto 4 data-parallel arrays at this time
        public CompiledQuery Compile<T>(Expression<Func<T, IQueryable>> query)
            where T : DataParallelArrayBase
        {
            return CompileQuery(query);
        }

        public CompiledQuery Compile<T1, T2>(Expression<Func<T1, T2, IQueryable>> query)
            where T1 : DataParallelArrayBase
            where T2 : DataParallelArrayBase
        {
            return CompileQuery(query);
        }

        public CompiledQuery Compile<T1, T2, T3>(Expression<Func<T1, T2, T3, IQueryable>> query)
            where T1 : DataParallelArrayBase
            where T2 : DataParallelArrayBase
            where T3 : DataParallelArrayBase
        {
            return CompileQuery(query);
        }

        public CompiledQuery Compile<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, IQueryable>> query)
            where T1 : DataParallelArrayBase
            where T2 : DataParallelArrayBase
            where T3 : DataParallelArrayBase
            where T4 : DataParallelArrayBase
        {
            return CompileQuery(query);
        }

        // All the overloads that run a CompiledQuery
        public IQueryable Run(CompiledQuery query, DataParallelArrayBase data)
        {
            return InitializeAndRunQuery(query, data);
        }

        public IQueryable Run(CompiledQuery query, DataParallelArrayBase data1, DataParallelArrayBase data2)
        {
            return InitializeAndRunQuery(query, data1, data2);
        }

        public IQueryable Run(CompiledQuery query, DataParallelArrayBase data1, DataParallelArrayBase data2, DataParallelArrayBase data3)
        {
            return InitializeAndRunQuery(query, data1, data2, data3);
        }

        public IQueryable Run(CompiledQuery query, DataParallelArrayBase data1, DataParallelArrayBase data2, DataParallelArrayBase data3, DataParallelArrayBase data4)
        {
            return InitializeAndRunQuery(query, data1, data2, data3, data4);
        }
    }
}