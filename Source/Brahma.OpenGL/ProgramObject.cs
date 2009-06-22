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
using System.Text;

using Brahma.Platform.OpenGL;

using Tao.OpenGl;

namespace Brahma.OpenGL
{
    internal sealed class ProgramObject: IDisposable
    {
        public const int None = 0;

        private readonly FragmentShader _fs;

        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        private readonly VertexShader _vs;
        private int _handle = int.MinValue;

        public ProgramObject(ContextBase context, VertexShader vs, FragmentShader fs)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            Context = context;

            // Vertex AND Fragment shaders can be null
            _vs = vs;
            _fs = fs;
        }

        public ProgramObject(ContextBase context, VertexShader vs)
            : this(context, vs, null)
        {
        }

        public ProgramObject(ContextBase context, FragmentShader fs)
            : this(context, null, fs)
        {
        }

        internal int Handle
        {
            get
            {
                return _handle;
            }
        }

        public bool Disposed
        {
            get;
            set;
        }

        public bool Linked
        {
            get;
            set;
        }

        public ContextBase Context
        {
            get;
            private set;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (Disposed || (!Linked))
                return;

            if (_vs != null)
                _vs.Dispose();
            if (_fs != null)
                _fs.Dispose();

            Context = null; // Don't keep a reference to this context

            Linked = false; // This program object isnt linked anymore
            Disposed = true;

            GC.SuppressFinalize(this);
        }

        #endregion

        internal LinkResult Link()
        {
            if (Linked) // If this program is linked already, don't do anything
                return new LinkResult(true);

            _handle = Context.GetNewProgramObject(true); // Create a program object

            // Attach any shaders we've been given
            if (_vs != null)
                Gl.glAttachObjectARB(_handle, _vs.Handle);
            if (_fs != null)
                Gl.glAttachObjectARB(_handle, _fs.Handle);

            // Link
            Gl.glLinkProgramARB(_handle);

            int maxLength;
            string messages = string.Empty;

            Gl.glGetObjectParameterivARB(_handle, Gl.GL_OBJECT_INFO_LOG_LENGTH_ARB, out maxLength);
            if (maxLength > 1)
            {
                var s = new StringBuilder(maxLength);
                int length;
                Gl.glGetInfoLogARB(_handle, maxLength, out length, s); // Get messages from the shader compiler
                messages = s.ToString();
            }

            int linkStatus;
            Gl.glGetObjectParameterivARB(_handle, Gl.GL_OBJECT_LINK_STATUS_ARB, out linkStatus);

            if (linkStatus == Gl.GL_TRUE)
            {
                Linked = true;
                return new LinkResult(true, messages);
            }

            return new LinkResult(false, messages);
        }

        ~ProgramObject()
        {
            Dispose();
        }

        internal ParameterBase<T> Parameters<T>(string name)
        {
            return Parameters<T>(name, false);
        }

        internal ParameterBase<T> Parameters<T>(string name, bool ignoreIfNotFound)
        {
            if (!_parameters.ContainsKey(name)) // Have we created this parameter yet?
            {
                object parameter;

                int location = Gl.glGetUniformLocationARB(_handle, name);
                if (location == -1) // Did we find this uniform?
                    if (ignoreIfNotFound)
                        return new DummyParameter<T>();
                    else
                        throw new ParameterException(string.Format(CultureInfo.InvariantCulture, "Could not find shader parameter {0}. Check to see if it exists", name));

                if (typeof (T) == typeof (float))
                    parameter = new FloatParameter(location);
                else if (typeof (T) == typeof (Vector2))
                    parameter = new Vector2Parameter(location);
                else if (typeof (T) == typeof (Vector3))
                    parameter = new Vector3Parameter(location);
                else if (typeof (T) == typeof (Vector4))
                    parameter = new Vector4Parameter(location);
                else if (typeof (T) == typeof (Vector2[]))
                    parameter = new Vector2ArrayParameter(location);
                else if (typeof (T) == typeof (int))
                    parameter = new IntParameter(location);
                else
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Could not map type {0} to a valid GLSL shader parameter type", typeof (T))); // Unknown type

                _parameters.Add(name, parameter); // Cache this
                return parameter as ParameterBase<T>; // Return it
            }

            return _parameters[name] as ParameterBase<T>; // We already have this parameter, return it
        }
    }

    internal struct LinkResult
    {
        private readonly string _messages;
        private readonly bool _success;

        internal LinkResult(bool success)
        {
            _success = success;
            _messages = string.Empty;
        }

        internal LinkResult(bool success, string messages)
        {
            _success = success;

            if (messages == null)
                throw new ArgumentNullException("messages");
            _messages = messages;
        }

        public bool Success
        {
            get
            {
                return _success;
            }
        }

        public string Messages
        {
            get
            {
                return _messages;
            }
        }

        public static explicit operator bool(LinkResult operand)
        {
            return operand.Success;
        }
    }
}