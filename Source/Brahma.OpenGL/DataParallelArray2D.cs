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
using System.Globalization;
using System.Linq.Expressions;

namespace Brahma.OpenGL
{
    public sealed class DataParallelArray2D<T>: DataParallelArray2DBase<T>, ISampler where T : struct
    {
        private bool _cpuStoreDirty; // Is our CPU data store dirty?
        private bool _gpuStoreDirty; // Is our GPU data store dirty?

        // Make the CPU data invalid

        private DataParallelArray2D(ComputationProvider provider, Expression expression, int width, int height, Func<int, int, T> values) // This is because no one outside this class uses it
            : base(provider, expression, width, height, values)
        {
            RowAddressingMode = AddressingMode.Clamp;
            ColumnAddressingMode = AddressingMode.Clamp;
            // Make sure our type parameter is of a permissible
            if ((typeof (T) != typeof (float)) &&
                (typeof (T) != typeof (Vector2)) &&
                (typeof (T) != typeof (Vector3)) &&
                (typeof (T) != typeof (Vector4)))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Cannot create a 2D DataParallelArray with elements of type {0}", typeof (T).FullName));

            // We know we have a valid CPU data-store
            InvalidateGPUData(); // Invalidate GPU store
            SetGPUData(); // Write the CPU data to GPU
        }

        internal DataParallelArray2D(ComputationProvider provider, int width, int height) // This constructor is called to create a new data-parallel array to hold the result of a computation
            : base(provider, null, width, height)
        {
            RowAddressingMode = AddressingMode.Clamp;
            ColumnAddressingMode = AddressingMode.Clamp;
            if ((typeof (T) != typeof (float)) &&
                (typeof (T) != typeof (Vector2)) &&
                (typeof (T) != typeof (Vector3)) &&
                (typeof (T) != typeof (Vector4)))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Cannot create a DataParallelArray with elements of type {0}", typeof (T).FullName));

            // We're not going to fill in any values, we're just going to get a texture from our pool
            Texture = provider.GetTexture(width, height);

            InvalidateCPUData(); // Indicate that our CPU data store is invalid
            // But don't GetGPUData, we'll do that when enumerating
        }

        public DataParallelArray2D(ComputationProvider provider, T[,] values)
            : this(provider, null, values.GetUpperBound(1) + 1, values.GetUpperBound(0) + 1, (x, y) => values[x, y])
        {
        }

        public DataParallelArray2D(ComputationProvider provider, int width, int height, T initialValue)
            : this(provider, null, width, height, (x, y) => initialValue)
        {
        }

        public DataParallelArray2D(ComputationProvider provider, int width, int height, Func<int, int, T> values)
            : this(provider, null, width, height, values)
        {
        }

        internal Texture Texture
        {
            get;
            set;
        }

        public int CurrentX
        {
            get
            {
                throw new InvalidOperationException("This value can never be accessed");
            }
        }

        public int CurrentY
        {
            get
            {
                throw new InvalidOperationException("This value can never be accessed");
            }
        }

        public override T this[int x, int y]
        {
            get
            {
                GetGPUData(); // Make sure we have valid CPU data to serve up
                return base[GetX(x), GetY(y)];
            }
            set
            {
                base[GetX(x), GetY(y)] = value;
                InvalidateGPUData(); // Indicate that the GPU data-store is dirty, but don't sync just yet
            }
        }

        public AddressingMode ColumnAddressingMode
        {
            get;
            set;
        }

        public AddressingMode RowAddressingMode
        {
            get;
            set;
        }

        public new ComputationProvider Provider
        {
            get
            {
                return base.Provider as ComputationProvider;
            }
        }

        #region ISampler Members

        string ISampler.SamplingFunction
        {
            get
            {
                return "texture2D";
            }
        }

        Texture ISampler.Texture
        {
            get
            {
                return Texture;
            }
        }

        int ISampler.Width
        {
            get
            {
                return Width;
            }
        }

        int ISampler.Height
        {
            get
            {
                return Height;
            }
        }

        #endregion

        private void InvalidateCPUData()
        {
            if (_gpuStoreDirty)
                throw new InvalidOperationException("The GPU and CPU store cannot be dirty at the same time!");

            _cpuStoreDirty = true;
        }

        // Make the GPU data invalid
        private void InvalidateGPUData()
        {
            if (_cpuStoreDirty)
                throw new InvalidOperationException("The CPU and GPU store cannot be dirty at the same time!");

            _gpuStoreDirty = true;
        }

        // Utility methods for getting the current indices based on addressing mode.
        private int GetX(int x)
        {
            int newX = x;
            if ((x < 0) || (x >= Width))
                switch (ColumnAddressingMode)
                {
                    case AddressingMode.Repeat:
                        newX = (x < 0)
                                   ? Width - (Math.Abs(x) % Width)
                                   : x % Width;
                        break;

                    case AddressingMode.Clamp:
                        newX = (x < 0)
                                   ? 0
                                   : Width - 1;
                        break;

                    default:
                        throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The addressing mode {0} is not supported at this time", ColumnAddressingMode));
                }

            return newX;
        }

        private int GetY(int y)
        {
            int newY = y;
            if ((y < 0) || (y >= Width))
                switch (RowAddressingMode)
                {
                    case AddressingMode.Repeat:
                        newY = (y < 0)
                                   ? Height - (Math.Abs(y) % Height)
                                   : y % Height;
                        break;

                    case AddressingMode.Clamp:
                        newY = (y < 0)
                                   ? 0
                                   : Height - 1;
                        break;

                    default:
                        throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The addressing mode {0} is not supported at this time", RowAddressingMode));
                }

            return newY;
        }

        private void SetData(T[][] data)
        {
            // TODO: Optimize this
            var vector4Data = new Vector4[Width * Height];
            if (typeof (T) == typeof (float)) // Convert a T[][] to a Vector4[], add any missing components
            {
                var floatData = data as float[][];
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        vector4Data[y * Width + x] = new Vector4(floatData[x][y], 0f, 0f, 0f); // R = X, G = 0, B = 0, A = 0
            }
            else if (typeof (T) == typeof (Vector2))
            {
                var vector2Data = data as Vector2[][];
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        vector4Data[y * Width + x] = new Vector4(vector2Data[x][y].x, vector2Data[x][y].y, 0f, 0f); // R = X, G = Y, B = 0, A = 0
            }
            else if (typeof (T) == typeof (Vector3))
            {
                var vector3Data = data as Vector3[][];
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        vector4Data[y * Width + x] = new Vector4(vector3Data[x][y].x, vector3Data[x][y].y, vector3Data[x][y].z, 0f); // R = X, G = Y, B = Z, A = 0
            }
            else if (typeof (T) == typeof (Vector4))
            {
                var vectorInputData = data as Vector4[][];
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        vector4Data[y * Width + x] = new Vector4(vectorInputData[x][y].x, vectorInputData[x][y].y, vectorInputData[x][y].z, vectorInputData[x][y].w); // R = X, G = Y, B = Z, A = W
            }
            else
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Data-parallel arrays of type {0} are not supported", typeof (T).FullName));


            Texture.SetData(vector4Data); // Set the data
        }

        // Sends the CPU data to the GPU
        private void SetGPUData()
        {
            if (!_gpuStoreDirty) // if the gpu store is not dirty, don't do anything
                return;

            // Check to see if Values is non-null
            if (Values == null)
                throw new InvalidOperationException("Cannot upload data to GPU when CPU data store is null");

            // Ask our texture pool for a texture of this size
            if (Texture == null)
                Texture = Provider.GetTexture(Width, Height);

            SetData(Values);

            _gpuStoreDirty = false;
        }

        private T[][] GetData()
        {
            // TODO: Optimize this
            Vector4[] data = Texture.GetData(); // Get the data

            if (typeof (T) == typeof (float))
            {
                var floatData = Values as float[][];
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        floatData[x][y] = data[y * Width + x].x; // R = X, G = 0, B = 0, A = 0
            }
            else if (typeof (T) == typeof (Vector2))
            {
                var vector2Data = Values as Vector2[][];
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        vector2Data[x][y] = new Vector2(data[y * Width + x].x, data[y * Width + x].y); // R = X, G = Y, B = 0, A = 0
            }
            else if (typeof (T) == typeof (Vector3))
            {
                var vector3Data = Values as Vector3[][];
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        vector3Data[x][y] = new Vector3(data[y * Width + x].x, data[y * Width + x].y, data[y * Width + x].z); // R = X, G = Y, B = Z, A = 0
            }
            else if (typeof (T) == typeof (Vector4))
            {
                var vector4OutputData = Values as Vector4[][];
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                        vector4OutputData[x][y] = new Vector4(data[y * Width + x].x, data[y * Width + x].y, data[y * Width + x].z, data[y * Width + x].w); // R = X, G = Y, B = Z, A = W
            }
            else
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Data-parallel arrays of type {0} are not supported", typeof (T).FullName));

            return Values;
        }

        // Get GPU data to CPU
        private void GetGPUData()
        {
            if (!_cpuStoreDirty) // CPU store isn't dirty, don't do anything
                return;

            // Read values from the GPU and write to CPU memory
            Values = new T[Width][]; // (Re)Allocate memory to hold the results
            for (int x = 0; x < Height; x++)
                Values[x] = new T[Height];

            // Check to see if the texture is ok
            if (Texture == null)
                throw new InvalidOperationException("Cannot update CPU store when GPU data store is null");

            Values = GetData(); // Get the data from the GPU store

            _cpuStoreDirty = false;
        }

        protected override void BeginQuery()
        {
            base.BeginQuery();

            SetGPUData(); // Make sure the GPU data is valid and any CPU changes are sent to the GPU
        }

        protected override void DisposeUnmanaged()
        {
            if ((Texture != null) && (!Texture.Disposed)) // Make sure this texture isn't disposed
                Provider.ReleaseTexture(Texture);

            base.DisposeUnmanaged();
        }

        protected override AddressingMode GetColumnAddressingMode()
        {
            return ColumnAddressingMode;
        }

        protected override AddressingMode GetRowAddressingMode()
        {
            return RowAddressingMode;
        }

        public override int GetLength(int dimension)
        {
            switch (dimension)
            {
                case 0:
                    return Width;

                case 1:
                    return Height;

                default:
                    throw new IndexOutOfRangeException(string.Format(CultureInfo.InvariantCulture, "The dimension {0} was greater than the dimensionality of this data-parallel array", dimension));
            }
        }

        public override IEnumerator<T> GetEnumerator()
        {
            for (int idy = 0; idy < Height; idy++)
                for (int idx = 0; idx < Width; idx++)
                    yield return this[idx, idy];
        }
    }
}