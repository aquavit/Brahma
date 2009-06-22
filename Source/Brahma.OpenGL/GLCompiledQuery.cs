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
using System.Linq;
using System.Linq.Expressions;

namespace Brahma.OpenGL
{
    public sealed class GLCompiledQuery: CompiledQuery
    {
        private readonly ProgramObject _program;
        private readonly ParameterExpression[] _queryParameters;
        private readonly MemberExpression[] _uniforms;

        internal GLCompiledQuery(ProgramObject program, MemberExpression[] uniforms, Type returnType, params ParameterExpression[] queryParameters)
            : base(returnType, (from ParameterExpression parameter in queryParameters
                                select parameter.Type).ToArray())
        {
            if (program == null)
                throw new ArgumentNullException("program");
            if (uniforms == null)
                throw new ArgumentNullException("uniforms");
            if (queryParameters == null)
                throw new ArgumentNullException("queryParameters");

            _program = program;
            _uniforms = uniforms;
            _queryParameters = queryParameters;
        }

        internal ProgramObject Program
        {
            get
            {
                return _program;
            }
        }

        internal MemberExpression[] Uniforms
        {
            get
            {
                return _uniforms;
            }
        }

        internal ParameterExpression[] QueryParameters
        {
            get
            {
                return _queryParameters;
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

        protected override void DisposeUnmanaged()
        {
            _program.Dispose(); // Dispose of the program. The fragment shader will be disposed with it

            base.DisposeUnmanaged();
        }
    }
}