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

using System.Runtime.InteropServices;

using Tao.OpenGl;

namespace Brahma.OpenGL
{
    // Dummy parameter
    internal sealed class DummyParameter<T>: ParameterBase<T>
    {
        public DummyParameter()
            : base(int.MinValue)
        {
        }
    }

    internal sealed class FloatParameter: ParameterBase<float>
    {
        public FloatParameter(int location)
            : base(location)
        {
        }

        public override float Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                Gl.glUniform1fARB(Location, value);
                base.Value = value;
            }
        }
    }

    internal sealed class Vector2Parameter: ParameterBase<Vector2>
    {
        public Vector2Parameter(int location)
            : base(location)
        {
        }

        public override Vector2 Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                Gl.glUniform2fARB(Location, value.x, value.y);
                base.Value = value;
            }
        }
    }

    internal sealed class Vector3Parameter: ParameterBase<Vector3>
    {
        public Vector3Parameter(int location)
            : base(location)
        {
        }

        public override Vector3 Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                Gl.glUniform3fARB(Location, value.x, value.y, value.z);
                base.Value = value;
            }
        }
    }

    internal sealed class Vector4Parameter: ParameterBase<Vector4>
    {
        public Vector4Parameter(int location)
            : base(location)
        {
        }

        public override Vector4 Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                Gl.glUniform4fARB(Location, value.x, value.y, value.z, value.w);
                base.Value = value;
            }
        }
    }

    // This is the kind of parameter used for texture parameters. 
    // In GLSL, we set the texture uniform to the texture unit it is active in
    internal sealed class IntParameter: ParameterBase<int>
    {
        public IntParameter(int location)
            : base(location)
        {
        }

        public override int Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                Gl.glUniform1iARB(Location, value);
                base.Value = value;
            }
        }
    }

    internal sealed class Vector2ArrayParameter: ParameterBase<Vector2[]>
    {
        public Vector2ArrayParameter(int location)
            : base(location)
        {
        }

        public override Vector2[] Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                // Get the address of the beginning of the array and pass it in. Is this portable?
                Gl.glUniform2fvARB(Location, value.Length * 2, Marshal.UnsafeAddrOfPinnedArrayElement(value, 0));
                base.Value = value;
            }
        }
    }
}