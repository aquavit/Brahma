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
using System.Linq.Expressions;

namespace Brahma
{
    public abstract class DataParallelArray2DBase<T>: DataParallelArrayBase<T> where T : struct
    {
        private readonly int _height;
        private readonly int _width;

        private T[][] _values;

        protected DataParallelArray2DBase(ComputationProviderBase provider, Expression expression, int width, int height, Func<int, int, T> getValues)
            : base(provider, expression)
        {
            _width = width;
            _height = height;

            if (getValues == null)
                throw new ArgumentNullException("getValues");

            _values = new T[width][];
            for (int x = 0; x < width; x++)
            {
                _values[x] = new T[height];
                for (int y = 0; y < height; y++)
                    _values[x][y] = getValues(x, y);
            }
        }

        protected DataParallelArray2DBase(ComputationProviderBase provider, Expression expression, int width, int height)
            : base(provider, expression)
        {
            _width = width;
            _height = height;
        }

        protected T[][] Values
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

        public int Width
        {
            get
            {
                return _width;
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
        }

        public virtual T this[int x, int y]
        {
            get
            {
                return _values[x][y];
            }
            set
            {
                _values[x][y] = value;
            }
        }

        public override int Rank
        {
            get
            {
                return 2;
            }
        }

        protected override void DisposeManaged()
        {
            _values = null; // This is a lot of memory. Null it so the GC can collect it

            base.DisposeManaged();
        }
    }
}