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
    public sealed class DataParallelArray<T>: DataParallelArray1DBase<T>, ISampler where T : struct
    {
        private bool _cpuStoreDirty; // Is our CPU data store dirty?
        private bool _gpuStoreDirty; // Is our GPU data store dirty?

        // Make the CPU data invalid

        private DataParallelArray(ComputationProvider provider, Expression expression, int length, Func<int, T> values) // This is because no one outside this class uses it
            : base(provider, expression, length, values)
        {
            AddressingMode = AddressingMode.Clamp;
            // Make sure our type parameter is of a permissible
            if ((typeof (T) != typeof (float)) &&
                (typeof (T) != typeof (Vector2)) &&
                (typeof (T) != typeof (Vector3)) &&
                (typeof (T) != typeof (Vector4)))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Cannot create a DataParallelArray with elements of type {0}", typeof (T).FullName));

            // We know we have a valid CPU data-store
            InvalidateGPUData(); // Invalidate GPU store
            SetGPUData(); // Write the CPU data to GPU
        }

        internal DataParallelArray(ComputationProvider provider, int length) // This constructor is called to create a new data-parallel array to hold the result of a computation
            : base(provider, null, length)
        {
            AddressingMode = AddressingMode.Clamp;
            if ((typeof (T) != typeof (float)) &&
                (typeof (T) != typeof (Vector2)) &&
                (typeof (T) != typeof (Vector3)) &&
                (typeof (T) != typeof (Vector4)))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Cannot create a DataParallelArray with elements of type {0}", typeof (T).FullName));

            // We're not going to fill in any values, we're just going to get a texture from our pool
            Texture = provider.GetTexture(length, ComputationProvider.OneDimensionalHeight);

            InvalidateCPUData(); // Indicate that our CPU data store is invalid
            // But don't GetGPUData, we'll do that when enumerating
        }

        public DataParallelArray(ComputationProvider provider, T[] values)
            : this(provider, null, values.GetUpperBound(0) + 1, index => values[index])
        {
        }

        public DataParallelArray(ComputationProvider provider, int length, T initialValue)
            : this(provider, null, length, x => initialValue)
        {
        }

        public DataParallelArray(ComputationProvider provider, int length, Func<int, T> values)
            : this(provider, null, length, values)
        {
        }

        internal RenderTargetTexture Texture
        {
            get;
            private set;
        }

        public override T this[int index]
        {
            get
            {
                GetGPUData(); // Make sure we have valid CPU data to serve up
                return base[GetIndex(index)];
            }
            set
            {
                base[GetIndex(index)] = value;
                InvalidateGPUData(); // Indicate that the GPU data-store is dirty, but don't sync
            }
        }

        // This dictates what happens when we address outside the range of this array
        public AddressingMode AddressingMode
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
                return Texture;
            }
        }

        int ISampler.Width
        {
            get
            {
                return Length;
            }
        }

        int ISampler.Height
        {
            get
            {
                return ComputationProvider.OneDimensionalHeight;
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

        // Utility method to get the index based on the current addressing mode
        private int GetIndex(int index)
        {
            if ((index < 0) || (index >= Length))
                switch (AddressingMode)
                {
                    case AddressingMode.Repeat:
                        return (index < 0)
                                   ? Length - (Math.Abs(index) % Length)
                                   : index % Length;

                    case AddressingMode.Clamp:
                        return (index < 0)
                                   ? 0
                                   : Length - 1;

                    default:
                        throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The addressing mode {0} is not supported at this time", AddressingMode));
                }
            return index;
        }

        // Send CPU data to GPU
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
            if (Texture == null)
                Texture = Provider.GetTexture(Length, ComputationProvider.OneDimensionalHeight);

            // Get the system memory surface we will copy values to
            Surface systemMemSurface = Provider.GetSystemMemSurface(Length, ComputationProvider.OneDimensionalHeight); // Ask the surface pool for a surface
            GraphicsStream stream = systemMemSurface.LockRectangle(LockFlags.None); // Lock it

            if (typeof (T) == typeof (float))
                for (int x = 0; x < Length; x++)
                {
                    // Only the X component is valid for a float
                    stream.Write(Values[x]); // R = value
                    stream.Write(0f); // G = nothing
                    stream.Write(0f); // B = nothing
                    stream.Write(0f); // A = nothing
                }
            else if (typeof (T) == typeof (Vector2))
            {
                var val = Values as Vector2[];
                if (val == null)
                    throw new InvalidOperationException(string.Format("Cannot process data-parallel array, wrong CPU data-store. Expected {0}, found {1} instead", typeof (Vector2[]), Values.GetType()));

                for (int x = 0; x < Length; x++)
                {
                    stream.Write(val[x].x); // R = X
                    stream.Write(val[x].y); // G = Y
                    stream.Write(0f); // B = nothing
                    stream.Write(0f); // A = nothing
                }
            }
            else if (typeof (T) == typeof (Vector3))
            {
                var val = Values as Vector3[];
                if (val == null)
                    throw new InvalidOperationException(string.Format("Cannot process data-parallel array, wrong CPU data-store. Expected {0}, found {1} instead", typeof (Vector3[]), Values.GetType()));

                for (int x = 0; x < Length; x++)
                {
                    stream.Write(val[x].x); // R = X
                    stream.Write(val[x].y); // G = Y
                    stream.Write(val[x].z); // B = Z
                    stream.Write(0f); // A = nothing
                }
            }
            else if (typeof (T) == typeof (Vector4))
            {
                var val = Values as Vector4[];
                if (val == null)
                    throw new InvalidOperationException(string.Format("Cannot process data-parallel array, wrong CPU data-store. Expected {0}, found {1} instead", typeof (Vector4[]), Values.GetType()));

                for (int x = 0; x < Length; x++)
                {
                    stream.Write(val[x].x); // R = X
                    stream.Write(val[x].y); // G = Y 
                    stream.Write(val[x].z); // B = Z
                    stream.Write(val[x].w); // A = W
                }
            }

            systemMemSurface.UnlockRectangle(); // Unlock
            device.UpdateSurface(systemMemSurface, Texture.Surface); // Upload it to the GPU

            Provider.ReleaseSystemMemSurface(systemMemSurface); // Release the system memory surface to the pool

            _gpuStoreDirty = false;
        }

        // Get GPU data to CPU
        private void GetGPUData()
        {
            if (!_cpuStoreDirty) // CPU store isn't dirty, don't do anything
                return;

            // Read values from the GPU and write to CPU memory
            Values = new T[Length]; // (Re)Allocate memory to hold the results

            // Check to see if the texture is ok
            if (Texture == null)
                throw new InvalidOperationException("Cannot update CPU store when GPU data store is null");

            Device device = Provider.Device; // Get the device from the provider
            // Ask the pool for a system memory surface to copy the results to
            Surface systemMemSurface = Provider.GetSystemMemSurface(Length, ComputationProvider.OneDimensionalHeight);
            device.GetRenderTargetData(Texture.Surface, systemMemSurface); // Get data from the GPU

            GraphicsStream stream = systemMemSurface.LockRectangle(LockFlags.None); // Lock the system-memory surface so we can read from it
            if (typeof (T) == typeof (float))
            {
                for (int idx = 0; idx < Length; idx++)
                {
                    Values[idx] = (T)stream.Read(typeof (float)); // X
                    stream.Read(typeof (float)); // Read and discard
                    stream.Read(typeof (float)); // Read and discard
                    stream.Read(typeof (float)); // Read and discard
                }
            }
            if (typeof (T) == typeof (Vector2))
            {
                var val = Values as Vector2[];
                if (val == null)
                    throw new InvalidOperationException(string.Format("Cannot process data-parallel array, wrong CPU data-store. Expected {0}, found {1} instead", typeof (Vector2[]), Values.GetType()));
                for (int idx = 0; idx < Length; idx++)
                {
                    val[idx].x = (float)stream.Read(typeof (float)); // X
                    val[idx].y = (float)stream.Read(typeof (float)); // Y
                    stream.Read(typeof (float)); // Read and discard
                    stream.Read(typeof (float)); // Read and discard
                }
            }
            if (typeof (T) == typeof (Vector3))
            {
                var val = Values as Vector3[];
                if (val == null)
                    throw new InvalidOperationException(string.Format("Cannot process data-parallel array, wrong CPU data-store. Expected {0}, found {1} instead", typeof (Vector3[]), Values.GetType()));
                for (int idx = 0; idx < Length; idx++)
                {
                    val[idx].x = (float)stream.Read(typeof (float)); // X
                    val[idx].y = (float)stream.Read(typeof (float)); // Y
                    val[idx].z = (float)stream.Read(typeof (float)); // Z
                    stream.Read(typeof (float)); // Read and discard
                }
            }
            if (typeof (T) == typeof (Vector4))
            {
                var val = Values as Vector4[];
                if (val == null)
                    throw new InvalidOperationException(string.Format("Cannot process data-parallel array, wrong CPU data-store. Expected {0}, found {1} instead", typeof (Vector4[]), Values.GetType()));
                for (int idx = 0; idx < Length; idx++)
                {
                    val[idx].x = (float)stream.Read(typeof (float)); // X
                    val[idx].y = (float)stream.Read(typeof (float)); // Y
                    val[idx].z = (float)stream.Read(typeof (float)); // Z
                    val[idx].w = (float)stream.Read(typeof (float)); // W
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

        protected override AddressingMode GetColumnAddressingMode()
        {
            return AddressingMode;
        }

        protected override AddressingMode GetRowAddressingMode()
        {
            return AddressingMode;
        }

        protected override void DisposeUnmanaged()
        {
            if (!Texture.Texture.Disposed) // Make sure this texture isn't disposed
                Provider.ReleaseTexture(Texture);

            base.DisposeUnmanaged();
        }

        public override IEnumerator<T> GetEnumerator()
        {
            for (int idx = 0; idx < Length; idx++)
                yield return this[idx];
        }
    }
}