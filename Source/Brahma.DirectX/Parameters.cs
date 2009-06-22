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

using System.Collections.Generic;

using Microsoft.DirectX.Direct3D;

namespace Brahma.DirectX
{
    internal sealed class DummyParameter<T>: ParameterBase<T>
    {
        public DummyParameter()
            : base(null, null)
        {
        }

        public override T Value
        {
            get
            {
                return default(T);
            }
            set
            {
                // Do nothing
            }
        }
    }

    internal sealed class IntParameter : ParameterBase<int>
    {
        private int _value;

        internal IntParameter(DXCompiledQuery query, EffectHandle effectHandle)
            : base(query, effectHandle)
        {
        }

        public override int Value
        {
            get
            {
                return _value;
            }
            set
            {
                Query.Constants.SetValue(Query.Device, EffectHandle, value); // Set the shader constant
                _value = value;
            }
        }
    }

    internal sealed class FloatParameter : ParameterBase<float>
    {
        private float _value;

        internal FloatParameter(DXCompiledQuery query, EffectHandle effectHandle)
            : base(query, effectHandle)
        {
        }

        public override float Value
        {
            get
            {
                return _value;
            }
            set
            {
                Query.Constants.SetValue(Query.Device, EffectHandle, value); // Set the shader constant
                _value = value;
            }
        }
    }

    internal sealed class Vector2Parameter: ParameterBase<Vector2>
    {
        private Vector2 _value;

        internal Vector2Parameter(DXCompiledQuery query, EffectHandle effectHandle)
            : base(query, effectHandle)
        {
        }

        public override Vector2 Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                Query.Constants.SetValue(Query.Device, EffectHandle, new Microsoft.DirectX.Vector2(value.x, value.y));
            }
        }
    }

    internal sealed class Vector3Parameter: ParameterBase<Vector3>
    {
        private Vector3 _value;

        internal Vector3Parameter(DXCompiledQuery query, EffectHandle effectHandle)
            : base(query, effectHandle)
        {
        }

        public override Vector3 Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                Query.Constants.SetValue(Query.Device, EffectHandle, new Microsoft.DirectX.Vector3(value.x, value.y, value.z));
            }
        }
    }

    internal sealed class Vector4Parameter: ParameterBase<Vector4>
    {
        private Vector4 _value;

        internal Vector4Parameter(DXCompiledQuery query, EffectHandle effectHandle)
            : base(query, effectHandle)
        {
        }

        public override Vector4 Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                Query.Constants.SetValue(Query.Device, EffectHandle, new Microsoft.DirectX.Vector4(value.x, value.y, value.z, value.w));
            }
        }
    }

    internal sealed class Vector2ArrayParameter: ParameterBase<Vector2[]>
    {
        private Vector2[] _value;

        internal Vector2ArrayParameter(DXCompiledQuery query, EffectHandle effectHandle)
            : base(query, effectHandle)
        {
        }

        public override Vector2[] Value
        {
            get
            {
                return _value;
            }
            set
            {
                Query.Constants.SetValue(Query.Device, EffectHandle, value);
                _value = value;
            }
        }
    }

    internal static class ConstantTableExtensions
    {
        // Add overloads to ConstantTable.SetValue
        // For some weird reason, it has overloads for float, float[] and Vector4, not for Vector2 and Vector3. Very strange indeed.
        public static void SetValue(this ConstantTable constantTable, Device device, EffectHandle effectHandle, Microsoft.DirectX.Vector2 vector)
        {
            constantTable.SetValue(device, effectHandle, new[] { vector.X, vector.Y });
        }

        public static void SetValue(this ConstantTable constantTable, Device device, EffectHandle effectHandle, Microsoft.DirectX.Vector3 vector)
        {
            constantTable.SetValue(device, effectHandle, new[] { vector.X, vector.Y, vector.Z });
        }

        public static void SetValue(this ConstantTable constantTable, Device device, EffectHandle effectHandle, Vector2[] vectors)
        {
            var values = new List<float>();
            foreach (Vector2 vector in vectors)
            {
                values.Add(vector.x);
                values.Add(vector.y);
            }

            constantTable.SetValue(device, effectHandle, values.ToArray());
        }

        public static void SetValue(this ConstantTable constantTable, Device device, EffectHandle effectHandle, Microsoft.DirectX.Vector2[] vectors)
        {
            var values = new List<float>();
            foreach (Microsoft.DirectX.Vector2 vector in vectors)
            {
                values.Add(vector.X);
                values.Add(vector.Y);
            }

            constantTable.SetValue(device, effectHandle, values.ToArray());
        }
    }
}