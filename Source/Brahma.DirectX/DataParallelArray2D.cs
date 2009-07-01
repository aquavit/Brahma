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

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Brahma.DirectX
{
    public sealed class DataParallelArray2D<T>: DataParallelArray2DBase<T>, ISampler where T : struct
    {
        private bool _cpuStoreDirty; // Is our CPU data store dirty?
        private bool _gpuStoreDirty; // Is our GPU data store dirty?
        private RenderTargetTexture _texture; // A wrapper to a texture and its surface

        // Make the CPU data invalid

        private DataParallelArray2D(ComputationProvider provider, Expression expression, int width, int height, Func<int, int, T> values) // This is because no one outside this class uses it
            : base(provider, expression, width, height, values)
        {
            ColumnAddressingMode = AddressingMode.Clamp;
            RowAddressingMode = AddressingMode.Clamp;
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
            ColumnAddressingMode = AddressingMode.Clamp;
            RowAddressingMode = AddressingMode.Clamp;
            if ((typeof (T) != typeof (float)) &&
                (typeof (T) != typeof (Vector2)) &&
                (typeof (T) != typeof (Vector3)) &&
                (typeof (T) != typeof (Vector4)))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Cannot create a DataParallelArray with elements of type {0}", typeof (T).FullName));

            // We're not going to fill in any values, we're just going to get a texture from our pool
            _texture = provider.GetTexture(width, height);

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

        internal RenderTargetTexture Texture
        {
            get
            {
                return _texture;
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
                return "tex2D";
            }
        }

        RenderTargetTexture ISampler.Texture
        {
            get
            {
                return _texture;
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

        // Sends the CPU data to the GPU
        private void SetGPUData()
        {
            if (!_gpuStoreDirty) // if the gpu store is not dirty, don't do anything
                return;

            // Check to see if Values is non-null
            if (Values == null)
                throw new InvalidOperationException("Cannot upload data to GPU when CPU data store is null");

            // Get the device
            Device device = Provider.Device;

            // Ask our texture "cache" for a texture of this size
            if (_texture == null)
                _texture = Provider.GetTexture(Width, Height);

            // Get the system memory surface we will copy values to
            Surface systemMemSurface = Provider.GetSystemMemSurface(Width, Height); // Ask the surface pool for a surface
            GraphicsStream stream = systemMemSurface.LockRectangle(LockFlags.None); // Lock it

            if (typeof (T) == typeof (float))
            {
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                    {
                        stream.Write(Values[x][y]); // R = value
                        stream.Write(0f); // G = nothing
                        stream.Write(0f); // B = nothing
                        stream.Write(0f); // A = nothing
                    }
            }
            else if (typeof (T) == typeof (Vector2))
            {
                var val = Values as Vector2[][];
                if (val == null)
                    throw new InvalidOperationException(string.Format("Cannot process data-parallel array, wrong CPU data-store. Expected {0}, found {1} instead", typeof (Vector2[]), Values.GetType()));

                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                    {
                        stream.Write(val[x][y].x); // R = X
                        stream.Write(val[x][y].y); // G = Y
                        stream.Write(0f); // B = nothing
                        stream.Write(0f); // A = nothing
                    }
            }
            else if (typeof (T) == typeof (Vector3))
            {
                var val = Values as Vector3[][];
                if (val == null)
                    throw new InvalidOperationException(string.Format("Cannot process data-parallel array, wrong CPU data-store. Expected {0}, found {1} instead", typeof (Vector3[]), Values.GetType()));

                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                    {
                        stream.Write(val[x][y].x); // R = X
                        stream.Write(val[x][y].y); // G = Y
                        stream.Write(val[x][y].z); // B = Z
                        stream.Write(0f); // A = nothing
                    }
            }
            else if (typeof (T) == typeof (Vector4))
            {
                var val = Values as Vector4[][];
                if (val == null)
                    throw new InvalidOperationException(string.Format("Cannot process data-parallel array, wrong CPU data-store. Expected {0}, found {1} instead", typeof (Vector4[]), Values.GetType()));

                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                    {
                        stream.Write(val[x][y].x); // R = X
                        stream.Write(val[x][y].y); // G = Y
                        stream.Write(val[x][y].z); // B = Z
                        stream.Write(val[x][y].w); // A = W
                    }
            }

            systemMemSurface.UnlockRectangle(); // Unlock
            device.UpdateSurface(systemMemSurface, _texture.Surface); // Upload it to the GPU

            Provider.ReleaseSystemMemSurface(systemMemSurface); // Release the system memory surface to the pool

            _gpuStoreDirty = false;
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
            if (_texture == null)
                throw new InvalidOperationException("Cannot update CPU store when GPU data store is null");

            Device device = Provider.Device; // Get the device from the provider

            // Ask the pool for a system memory surface to copy the results to
            Surface systemMemSurface = Provider.GetSystemMemSurface(Width, Height);
            device.GetRenderTargetData(Texture.Surface, systemMemSurface); // Get data from the GPU

            GraphicsStream stream = systemMemSurface.LockRectangle(LockFlags.None); // Lock the system-memory surface so we can read from it
            if (typeof (T) == typeof (float))
            {
                for (int idx = 0; idx < Width; idx++)
                    for (int idy = 0; idy < Height; idy++)
                    {
                        Values[idy][idx] = (T)stream.Read(typeof (float));
                        stream.Read(typeof (float));
                        stream.Read(typeof (float));
                        stream.Read(typeof (float));
                    }
            }
            if (typeof (T) == typeof (Vector2))
            {
                var val = Values as Vector2[][];
                if (val == null)
                    throw new InvalidOperationException(string.Format("Cannot process data-parallel array, wrong CPU data-store. Expected {0}, found {1} instead", typeof (Vector2[]), Values.GetType()));

                for (int idx = 0; idx < Width; idx++)
                    for (int idy = 0; idy < Height; idy++)
                    {
                        val[idy][idx].x = (float)stream.Read(typeof (float));
                        val[idy][idx].y = (float)stream.Read(typeof (float));
                        stream.Read(typeof (float));
                        stream.Read(typeof (float));
                    }
            }
            if (typeof (T) == typeof (Vector3))
            {
                var val = Values as Vector3[][];
                if (val == null)
                    throw new InvalidOperationException(string.Format("Cannot process data-parallel array, wrong CPU data-store. Expected {0}, found {1} instead", typeof (Vector3[]), Values.GetType()));

                for (int idx = 0; idx < Width; idx++)
                    for (int idy = 0; idy < Height; idy++)
                    {
                        val[idy][idx].x = (float)stream.Read(typeof (float));
                        val[idy][idx].y = (float)stream.Read(typeof (float));
                        val[idy][idx].z = (float)stream.Read(typeof (float));
                        stream.Read(typeof (float));
                    }
            }
            if (typeof (T) == typeof (Vector4))
            {
                var val = Values as Vector4[][];
                if (val == null)
                    throw new InvalidOperationException(string.Format("Cannot process data-parallel array, wrong CPU data-store. Expected {0}, found {1} instead", typeof (Vector4[]), Values.GetType()));

                for (int idx = 0; idx < Width; idx++)
                    for (int idy = 0; idy < Height; idy++)
                    {
                        val[idy][idx].x = (float)stream.Read(typeof (float));
                        val[idy][idx].y = (float)stream.Read(typeof (float));
                        val[idy][idx].z = (float)stream.Read(typeof (float));
                        val[idy][idx].w = (float)stream.Read(typeof (float));
                    }
            }

            systemMemSurface.UnlockRectangle(); // Unlock
            Provider.ReleaseSystemMemSurface(systemMemSurface); // Release the system memory surface to the pool

            _cpuStoreDirty = false;
        }

        protected override void BeginQuery()
        {
            base.BeginQuery();

            SetGPUData(); // Make sure the GPU data is valid and any CPU changes are sent to the GPU
        }

        protected override void DisposeUnmanaged()
        {
            if (!_texture.Texture.Disposed) // Make sure this texture isn't disposed
                Provider.ReleaseTexture(_texture);

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
                    throw new IndexOutOfRangeException(string.Format(CultureInfo.InvariantCulture, "The dimension {0} was greater than the rank of this data-parallel array", dimension));
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