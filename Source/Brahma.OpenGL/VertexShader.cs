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

using System.Text;

using Brahma.Platform.OpenGL;

using Tao.OpenGl;

namespace Brahma.OpenGL
{
    // A wrapper to use vertex shaders easily
    internal sealed class VertexShader: ShaderBase
    {
        private bool _compiled;
        private int _handle = int.MinValue;

        public VertexShader(ContextBase context, string source)
            : base(context, source)
        {
        }

        internal int Handle
        {
            get
            {
                return _handle;
            }
        }

        public bool Compiled
        {
            get
            {
                return _compiled;
            }
        }

        protected override void DisposeManaged()
        {
            Context = null; // Don't hold a reference to the context anymore

            base.DisposeManaged();
        }

        internal override CompileResult Compile()
        {
            if (_compiled) // If we're compiled already, don't do it again
                return new CompileResult(true);

            int length = Source.Length;
            string source = Source;

            // Create the shader handle, register it for disposal
            _handle = Context.GetNewVertexShaderObject(true);

            // Compile the shader
            Gl.glShaderSourceARB(_handle, 1, new[] { source }, new[] { length });
            Gl.glCompileShaderARB(_handle);

            string messages = "";
            int maxLength;
            Gl.glGetObjectParameterivARB(_handle, Gl.GL_OBJECT_INFO_LOG_LENGTH_ARB, out maxLength);
            if (maxLength > 1)
            {
                // Get the messages (if any)
                var s = new StringBuilder(maxLength);
                Gl.glGetInfoLogARB(_handle, maxLength, out length, s);
                messages = s.ToString();
            }

            int compileStatus;
            Gl.glGetObjectParameterivARB(_handle, Gl.GL_OBJECT_COMPILE_STATUS_ARB, out compileStatus);

            // Return a CompileResult based on the results of the compile
            if (compileStatus == Gl.GL_TRUE)
            {
                _compiled = true;
                return new CompileResult(true, messages);
            }

            return new CompileResult(false, messages);
        }
    }
}