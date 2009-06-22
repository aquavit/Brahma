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

using Microsoft.DirectX.Direct3D;

namespace Brahma.DirectX
{
    public sealed class DXCompiledQuery: CompiledQuery
    {
        private readonly ConstantTable _constantTable;
        private readonly Device _device;
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        private readonly PixelShader _pixelShader;
        private readonly MemberExpression[] _shaderConstants;

        internal DXCompiledQuery(Device device, PixelShader pixelShader, ConstantTable constantTable, MemberExpression[] shaderConstants, Type returnType, params Type[] parameterTypes)
            : base(returnType, parameterTypes)
        {
            if (device == null)
                throw new ArgumentNullException("device");
            if (pixelShader == null)
                throw new ArgumentNullException("pixelShader");
            if (constantTable == null)
                throw new ArgumentNullException("constantTable");
            if (shaderConstants == null)
                throw new ArgumentNullException("shaderConstants");

            _device = device;
            _pixelShader = pixelShader;
            _constantTable = constantTable;
            _shaderConstants = shaderConstants;
        }

        internal Device Device
        {
            get
            {
                return _device;
            }
        }

        internal PixelShader PixelShader
        {
            get
            {
                return _pixelShader;
            }
        }

        internal ConstantTable Constants
        {
            get
            {
                return _constantTable;
            }
        }

        internal new Type[] ParameterTypes
        {
            get
            {
                return base.ParameterTypes;
            }
        }

        internal new Type ReturnType
        {
            get
            {
                return base.ReturnType;
            }
        }

        internal MemberExpression[] ShaderConstants
        {
            get
            {
                return _shaderConstants;
            }
        }

        protected override void DisposeUnmanaged()
        {
            _constantTable.Dispose(); // Dispose the pixel shader and the constants table
            _pixelShader.Dispose();

            base.DisposeUnmanaged();
        }

        internal ParameterBase<T> Parameters<T>(string name)
        {
            return Parameters<T>(name, false);
        }

        internal ParameterBase<T> Parameters<T>(string name, bool ignoreIfNotFound)
        {
            if (!_parameters.ContainsKey(name)) // If we haven't created this parameter yet
            {
                object parameter;

                EffectHandle effectHandle = Constants.GetConstant(null, name);

                if (effectHandle == null) // Whoops!
                    if (ignoreIfNotFound)
                        return new DummyParameter<T>(); // Return a dummy parameter, don't cache it
                    else
                        throw new ParameterException(string.Format(CultureInfo.InvariantCulture, "Could not find shader parameter {0}. Check to see if it exists", name));
                
                if (typeof(T) == typeof(int))
                    parameter = new IntParameter(this, effectHandle);
                else if (typeof (T) == typeof (float))
                    parameter = new FloatParameter(this, effectHandle);
                else if (typeof (T) == typeof (Vector2))
                    parameter = new Vector2Parameter(this, effectHandle);
                else if (typeof (T) == typeof (Vector3))
                    parameter = new Vector3Parameter(this, effectHandle);
                else if (typeof (T) == typeof (Vector4))
                    parameter = new Vector4Parameter(this, effectHandle);
                else if (typeof (T) == typeof (Vector2[]))
                    parameter = new Vector2ArrayParameter(this, effectHandle);
                else
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Could not map type {0} to a valid shader parameter type", typeof (T))); // Unknown type

                _parameters.Add(name, parameter); // Cache this
                return parameter as ParameterBase<T>; // Return it
            }

            return _parameters[name] as ParameterBase<T>; // Return the cached parameter
        }
    }
}