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
    public sealed class DataParallelArray<T>: DataParallelArray1DBase<T>, ISampler where T : struct
    {
        private bool _cpuStoreDirty; // Is our CPU data store dirty?
        private bool _gpuStoreDirty; // Is our GPU data store dirty?

        // Make the CPU data invalid

        private DataParallelArray(ComputationProvider provider, Expression expression, int length, Func<int, T> values) // This is because no one outside this class uses it
            : base(provider, expression, length, values)
        {
            AddressingMode = AddressingMode.Clamp;
            Expression = expression;
            // Make sure our type parameter is permissible
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
            Texture = Provider.GetTexture(length, ComputationProvider.OneDimensionalHeight);

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

        public Expression Expression
        {
            get;
            set;
        }

        internal Texture Texture
        {
            get;
            set;
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

        public int Current
        {
            get
            {
                throw new InvalidOperationException("This value can never be accessed");
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

        private void SetData(T[] data)
        {
            // TODO: Optimize this
            Vector4[] vector4Data;
            if (typeof (T) == typeof (float)) // Convert a T[] to a Vector4[], add any missing components
            {
                vector4Data = new Vector4[data.Length];
                var floatData = data as float[];
                for (int i = 0; i < data.Length; i++)
                    vector4Data[i] = new Vector4(floatData[i], 0f, 0f, 0f); // R = X, G = 0, B = 0, A = 0
            }
            else if (typeof (T) == typeof (Vector2))
            {
                vector4Data = new Vector4[data.Length];
                var vector2Data = data as Vector2[];
                for (int i = 0; i < data.Length; i++)
                    vector4Data[i] = new Vector4(vector2Data[i].x, vector2Data[i].y, 0f, 0f); //R = X, G = Y, B = 0, A = 0
            }
            else if (typeof (T) == typeof (Vector3))
            {
                vector4Data = new Vector4[data.Length];
                var vector3Data = data as Vector3[];
                for (int i = 0; i < data.Length; i++)
                    vector4Data[i] = new Vector4(vector3Data[i].x, vector3Data[i].y, vector3Data[i].z, 0f); // R = X, G = Y, B = Z, A = W
            }
            else if (typeof (T) == typeof (Vector4))
                vector4Data = data as Vector4[];
            else
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Data-parallel arrays of type {0} are not supported", typeof (T).FullName));


            Texture.SetData(vector4Data); // Set the data
        }

        // Send CPU data to GPU
        private void SetGPUData()
        {
            if (!_gpuStoreDirty) // if the gpu store is not dirty, don't do anything
                return;

            // Check to see if Values is non-null
            if (Values == null)
                throw new InvalidOperationException("Cannot upload data to GPU when CPU data store is null");

            // Check to see if we have a valid texture
            if (Texture == null)
                Texture = Provider.GetTexture(Length, ComputationProvider.OneDimensionalHeight);

            SetData(Values); // Set these values on the texture, sending them to the GPU

            _gpuStoreDirty = false;
        }

        private T[] GetData()
        {
            // TODO: Optimize this
            Vector4[] data = Texture.GetData(); // Get the data

            if (typeof (T) == typeof (float))
            {
                var floatData = Values as float[];
                for (int i = 0; i < data.Length; i++)
                    floatData[i] = data[i].x;
            }
            else if (typeof (T) == typeof (Vector2))
            {
                var vector2Data = Values as Vector2[];
                for (int i = 0; i < data.Length; i++)
                    vector2Data[i] = new Vector2(data[i].x, data[i].y);
            }
            else if (typeof (T) == typeof (Vector3))
            {
                var vector3Data = Values as Vector3[];
                for (int i = 0; i < data.Length; i++)
                    vector3Data[i] = new Vector3(data[i].x, data[i].y, data[i].z);
            }
            else if (typeof (T) == typeof (Vector4))
            {
                var vector4Data = Values as Vector4[];
                for (int i = 0; i < data.Length; i++)
                    vector4Data[i] = data[i];
            }
            else
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Data-parallel arrays of type {0} are not supported", typeof (T).FullName));

            return Values;
        }

        private void GetGPUData()
        {
            if (!_cpuStoreDirty) // CPU store isn't dirty, don't do anything
                return;

            // Read values from the GPU and write to CPU memory
            Values = new T[Length]; // (Re)Allocate memory to hold the results

            // Check to see if the texture is ok
            if (Texture == null)
                throw new InvalidOperationException("Cannot update CPU store when GPU data store is null");

            Values = GetData(); // Get current data from the GPU

            _cpuStoreDirty = false;
        }

        protected override AddressingMode GetColumnAddressingMode()
        {
            return AddressingMode;
        }

        protected override AddressingMode GetRowAddressingMode()
        {
            return AddressingMode;
        }

        protected override void BeginQuery()
        {
            base.BeginQuery();

            SetGPUData(); // Make sure the GPU data is valid and any CPU changes are sent to the GPU
        }

        protected override void DisposeUnmanaged()
        {
            if ((Texture != null) && !Texture.Disposed) // Make sure this texture isn't disposed
                Provider.ReleaseTexture(Texture); // Give it back to the pool

            base.DisposeUnmanaged();
        }

        public override IEnumerator<T> GetEnumerator()
        {
            for (int idx = 0; idx < Length; idx++)
                yield return this[idx];
        }
    }
}