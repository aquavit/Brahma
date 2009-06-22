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
using System.Linq.Expressions;

namespace Brahma
{
    // Generic base class for all 1D data-parallel arrays
    public abstract class DataParallelArray1DBase<T>: DataParallelArrayBase<T> where T : struct
    {
        private readonly int _length;
        private T[] _values;

        // Create a DataParallelArray1DBase, and populate values in it using the provided lambda
        protected DataParallelArray1DBase(ComputationProviderBase provider, Expression expression, int length, Func<int, T> getValues)
            : base(provider, expression)
        {
            _length = length;

            if (getValues == null)
                throw new ArgumentNullException("getValues");

            _values = new T[_length]; // Create the array containing the CPU data-store
            for (int index = 0; index < _length; index++)
                _values[index] = getValues(index); // Fill it up using the provided lambda
        }

        protected DataParallelArray1DBase(ComputationProviderBase provider, Expression expression, int length)
            : this(provider, expression, length, index => default(T)) // Fill with default values
        {
        }

        protected T[] Values
        {
            get
            {
                return _values;
            }
            set
            {
                _values = value;
            }
        }

        // This may change, or action might need to be taken before this is accessed
        public virtual int Length
        {
            get
            {
                return _length;
            }
        }

        public virtual T this[int index]
        {
            get
            {
                return _values[index];
            }
            set
            {
                _values[index] = value;
            }
        }

        public override int Rank
        {
            get
            {
                return 1;
            }
        }

        protected override void DisposeManaged()
        {
            _values = null; // This is a lot of memory. Null it so the GC can collect it

            base.DisposeManaged();
        }

        public override IEnumerator<T> GetEnumerator()
        {
            // enumerate through _values
            foreach (T value in _values)
                yield return value;
        }

        public override int GetLength(int dimension)
        {
            switch (dimension)
            {
                case 1:
                    return Length;

                default:
                    throw new ArgumentOutOfRangeException("dimension");
            }
        }
    }
}