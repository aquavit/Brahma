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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;

using Brahma.OpenGL.Helper;
using Brahma.Platform.OpenGL;

using Tao.OpenGl;

namespace Brahma.OpenGL
{
    public sealed class ComputationProvider: ComputationProviderBase
    {
        internal const int OneDimensionalHeight = 1;
        internal const PixelFormat TextureFormat = PixelFormat.RgbaFloat; // 4-component floating point

        private static readonly string[] RequiredExtensions = new[]
                                                                  {
                                                                      "GL_ARB_fragment_shader",
                                                                      "GL_ARB_vertex_shader",
                                                                      "GL_ARB_shader_objects",
                                                                      "GL_ARB_shading_language_100",
                                                                      "GL_EXT_framebuffer_object",
                                                                      "GL_ARB_texture_float",
                                                                      "GL_ARB_texture_non_power_of_two"
                                                                  };

        private readonly FrameBufferObject _fbo; // Our FBO, to which we'll attach textures
        private readonly bool _fromWindowHandle;
        private readonly ContextBase _glContext; // Our platform independent OpenGL context
        private readonly List<Texture> _texturePool = new List<Texture>(); // A pool of textures to avoid re-creating them

        public ComputationProvider()
            : this(new Form())
        {
        }

        public ComputationProvider(Control control)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            IntPtr windowHandle = control.Handle;
            if (windowHandle == IntPtr.Zero)
                throw new ArgumentException("Invalid window handle");

            _fromWindowHandle = true; // We created this context, we control it and we will be disposing it
            _glContext = ContextFactory.CreateContext(control);
            _glContext.MakeCurrent();

            // Let Tao load all extensions currently supported
            Gl.ReloadFunctions();

            // Check if this hardware/driver combination supports all the extensions we need
            foreach (string extension in RequiredExtensions)
                if (!Gl.IsExtensionSupported(extension))
                    throw new InitializationException(string.Format(CultureInfo.InvariantCulture, "Your current driver/hardware combination does not support {0}", extension));

            _fbo = new FrameBufferObject(_glContext);
        }

        public ComputationProvider(IntPtr renderingContext)
        {
            if (renderingContext == IntPtr.Zero)
                throw new ArgumentException("Invalid rendering context provided");

            _fromWindowHandle = false; // We didn't create this context, our responsibilities include only resources registered for disposal
            _glContext = ContextFactory.CreateContext(renderingContext);
            _glContext.MakeCurrent();

            // Let Tao load all extensions currently supported
            Gl.ReloadFunctions();

            // Check if this hardware/driver combination supports all the extensions we need
            foreach (string extension in RequiredExtensions)
                if (!Gl.IsExtensionSupported(extension))
                    throw new InitializationException(string.Format(CultureInfo.InvariantCulture, "Your current driver/hardware combination does not support {0}", extension));

            _fbo = new FrameBufferObject(_glContext);
        }

        internal FrameBufferObject FBO
        {
            get
            {
                return _fbo;
            }
        }

        internal ContextBase Context
        {
            get
            {
                return _glContext;
            }
        }

        private static Type TransformGenericTypeArgument(Type type)
        {
            if ((type == typeof (float)) || (type == typeof (double)))
                return typeof (float);

            if ((type == typeof (Vector2)) || (type == typeof (Vector3)) || (type == typeof (Vector4)))
                return type;

            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Cannot generate result with element type {0}", type.FullName));
        }

        protected override object Execute(Expression expression)
        {
            throw new NotImplementedException(); // This method is never called
        }

        protected override CompiledQuery CompileQuery(LambdaExpression expression)
        {
            if (!_glContext.IsCurrent)
                _glContext.MakeCurrent(); // Make sure this context is active
            
            // Create a GLSL generator and pass in an ExpressionProcessor to it
            // Although there is an overload where we could pass in an expression tree,
            // We need the expression processor to create the CompiledQuery
            var expressionProcessor = new ExpressionProcessor(expression);
            expressionProcessor.Process(); // Tell the expression processor to process the given expression.
            string glsl = GLSLGenerator.Generate(expressionProcessor);

            var fragmentShader = new FragmentShader(_glContext, glsl);
            CompileResult result = fragmentShader.Compile();

            if (!(bool)result)
            {
                string errors = string.Format("Could not compile pixel shader, the shader compiler said: {0}", result.Messages);
                throw new InvalidProgramException(string.Format(CultureInfo.InvariantCulture, "The generated GLSL was invalid. You should not be seeing this error.\n" +
                                                                                              "Please report this error to ananth<at>ananthonline<dot>net, along with the query that generated it.\n" +
                                                                                              "{0}\n" +
                                                                                              "Shader source:\n" +
                                                                                              "{1}", errors, glsl));
            }

            // Create and link the program object
            var program = new ProgramObject(_glContext, fragmentShader);
            LinkResult linkResult = program.Link();
            if (!(bool)linkResult)
            {
                string errors = string.Format("Could not link program object, the shader compiler said: {0}", linkResult.Messages);
                throw new InvalidProgramException(string.Format(CultureInfo.InvariantCulture, "The generated GLSL was invalid. You should not be seeing this error.\n" +
                                                                                              "Please report this error to ananth<at>ananthonline<dot>net, along with the query that generated it.\n" +
                                                                                              "{0}\n" +
                                                                                              "Shader source:\n" +
                                                                                              "{1}", errors, glsl));
            }

            // This is different from HLSLGenerator in the sense that we pass in the parameters themselves, not only their types
            return new GLCompiledQuery(program, expressionProcessor.MemberAccess.ToArray(), expression.Body.Type, expressionProcessor.QueryParameters.ToArray());
        }

        protected override IQueryable RunQuery(CompiledQuery query, int[] outputDimensions, params DataParallelArrayBase[] arguments)
        {
            if (!_glContext.IsCurrent)
                _glContext.MakeCurrent(); // Make sure this context is active
            
            var q = query as GLCompiledQuery;
            if (q == null) // Make sure it IS a Brahma.DirectX.CompiledQuery
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} cannot execute query of type {1}", GetType().FullName, query.GetType().FullName));

            if (arguments.Length != q.ParameterTypes.Length) // Make sure we have the correct number of arguments
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Expected {0} arguments, found {1} instead", q.ParameterTypes.Length, arguments.Length));

            // This section has been moved up in the execution order for OpenGL because fbo preparing does a glBindTexture too. Better to get it done before the query shader executes
            // *** IMPORTANT ***
            // In Brahma, given multiple data-parallel arrays (say: d1, d2, d3), the output array will have dimensions
            // min(d1.Width, d2.Width, d3.Width) X min(d1.Height, d2.Height, d3.Height). All operations will then run element-wise
            // on the output array. Extra elements on any of the data-parallel arrays will not be processed!
            // All "special" functionality that is not element-wise needs to be implemented as operators or extension methods on data-parallel arrays
            // Example: Given 3 data-parallel arrays of dimensions d1 = 3x3 d2 = 2x3 d3 = 10x1, the result would have dimensions min(2, 2, 10) x min(3, 3, 1) = 2 x 1
            // so, only two columns and one row will be processed on ALL the data-parallel arrays. The rest of the elements will NOT be processed.
            // *** Addendum ***
            // The user is allowed to pass in the output dimensions, so these will be used (if valid)

            int width = int.MaxValue;
            int height = int.MaxValue;

            var invDimensions = new List<Vector2>();
            foreach (DataParallelArrayBase param in arguments)
            {
                var sampler = param as ISampler; // Cast this to an ISampler, we need Height and Width information from it

                if (sampler == null)
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Given argument of type {0} does not implement ISampler", param.GetType()));

                // Find the minimum width and height, the result will be a dataparallel array of these dimensions
                if (sampler.Width < width)
                    width = sampler.Width;
                if (sampler.Height < height)
                    height = sampler.Height;

                invDimensions.Add(new Vector2(1f / sampler.Width, 1f / sampler.Height));
            }

            // Force the specified dimensions (if correct)
            if ((outputDimensions != null) && (outputDimensions.Length > 0))
            {
                if (outputDimensions.Length > 2)
                    throw new ArgumentOutOfRangeException("outputDimensions", "The rank of the output cannot exceed 2");

                // Get the width and height
                width = outputDimensions[0];
                height = outputDimensions.Length == 2 ? outputDimensions[1] : 1;
            }

            Type resultType = height == 1
                                  ? typeof (DataParallelArray<>)
                                  : typeof (DataParallelArray2D<>); // First get an non-sealed generic based on dimensions
            // Use the return type of the expression to determine the generic type argument
            resultType = resultType.MakeGenericType(TransformGenericTypeArgument(q.ReturnType.GetGenericArguments()[0]));

            DataParallelArrayBase result;
            if (height == 1) // This is a one-dimensional data-parallel array
                result = Activator.CreateInstance(resultType, BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { this, width }, CultureInfo.InvariantCulture) as DataParallelArrayBase;
            else // This is a two-dimensional data-parallel array
                result = Activator.CreateInstance(resultType, BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { this, width, height }, CultureInfo.InvariantCulture) as DataParallelArrayBase;

            // Set the program object we've been given
            Gl.glUseProgramObjectARB(q.Program.Handle);

            // Set up the samplers next
            for (int i = 0; i < arguments.Length; i++) // Make sure all the types are what we expect, too
            {
                if (arguments[i] == null) // Make sure none of the arguments are null
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Parameter {0} was null. Valid reference expected", i));

                if (!q.ParameterTypes[i].IsAssignableFrom(arguments[i].GetType())) // Is is of the correct type, or assignable to the correct type?
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Expected argument {0} to be of type {1}, found {2} instead",
                                                              i, q.ParameterTypes[i].FullName, arguments[i].GetType().FullName));

                Gl.glActiveTexture(Gl.GL_TEXTURE0 + i);
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, ((ISampler)arguments[i]).Texture.TextureId);

                // Set up the addressing mode for columns (U) and rows (V)
                var addressable = arguments[i] as IAddressable;
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, addressable.ColumnAddressingMode.ToGLAddressingMode());
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, addressable.RowAddressingMode.ToGLAddressingMode());

                // Make sure we tell our shader where the different samplers are. Remember, we dont need to add Gl.GL_TEXTURE0 to it
                q.Program.Parameters<int>(q.QueryParameters[i].Name, true).Value = i;
            }

            // Finally, set up the shader constants here
            foreach (MemberExpression memberExp in q.Uniforms)
            {
                if (memberExp.IsOutputCoordAccess()) // Skip output.Current<blah> accesses
                    continue;

                if (memberExp.Type == typeof(int))
                    q.Program.Parameters<int>(memberExp.Member.Name).Value =
                        memberExp.Expression == null // Is this a static field?
                            ? (int)((FieldInfo)memberExp.Member).GetValue(null) // Yes it is. We don't need an instance
                            : (int)((FieldInfo)memberExp.Member).GetValue(((ConstantExpression)memberExp.Expression).Value); // We need an instance. Get it from memberExp.Expression

                if (memberExp.Type == typeof(float))
                    q.Program.Parameters<float>(memberExp.Member.Name).Value =
                        memberExp.Expression == null // Is this a static field?
                            ? (float)((FieldInfo)memberExp.Member).GetValue(null) // Yes it is. We don't need an instance
                            : (float)((FieldInfo)memberExp.Member).GetValue(((ConstantExpression)memberExp.Expression).Value); // We need an instance. Get it from memberExp.Expression

                if (memberExp.Type == typeof (Vector2))
                    q.Program.Parameters<Vector2>(memberExp.Member.Name).Value =
                        memberExp.Expression == null // Is this a static field?
                            ? (Vector2)((FieldInfo)memberExp.Member).GetValue(null) // Yes it is. We don't need an instance
                            : (Vector2)((FieldInfo)memberExp.Member).GetValue(((ConstantExpression)memberExp.Expression).Value); // We need an instance. Get it from memberExp.Expression

                if (memberExp.Type == typeof (Vector3))
                    q.Program.Parameters<Vector3>(memberExp.Member.Name).Value =
                        memberExp.Expression == null // Is this a static field?
                            ? (Vector3)((FieldInfo)memberExp.Member).GetValue(null) // Yes it is. We don't need an instance
                            : (Vector3)((FieldInfo)memberExp.Member).GetValue(((ConstantExpression)memberExp.Expression).Value); // We need an instance. Get it from memberExp.Expression

                if (memberExp.Type == typeof (Vector4))
                    q.Program.Parameters<Vector4>(memberExp.Member.Name).Value =
                        memberExp.Expression == null // Is this a static field?
                            ? (Vector4)((FieldInfo)memberExp.Member).GetValue(null) // Yes it is. We don't need an instance
                            : (Vector4)((FieldInfo)memberExp.Member).GetValue(((ConstantExpression)memberExp.Expression).Value); // We need an instance. Get it from memberExp.Expression
            }

            // Set up the shader constants we've added for internal use
            q.Program.Parameters<float>("_brahma_minWidth", true).Value = width;
            q.Program.Parameters<float>("_brahma_minHeight", true).Value = height;
            q.Program.Parameters<Vector2[]>("_brahma_invDimensions", true).Value = invDimensions.ToArray();

            var resultSampler = result as ISampler;
            if (resultSampler != null) // This check might be redundant since we're creating this object ourselves, and we know what type it is
            {
                _fbo.Enable(); // Set fbo as the current rendertarget
                _fbo.Attach(Gl.GL_TEXTURE_2D, resultSampler.Texture.TextureId);
                if (!_fbo.Valid)
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The framebuffer object was invalid, error was {0}", _fbo.Error));
            }

            RenderScreenAlignedQuad(width, height); // Render the screen aligned quad
            Gl.glFinish(); // Make sure OpenGL is done with all drawing

            _fbo.Detach(Gl.GL_COLOR_ATTACHMENT0_EXT); // Remove this texture from the FBO

            return result;
        }

        protected override void DisposeUnmanaged()
        {
            if (!_glContext.IsCurrent)
                _glContext.MakeCurrent(); // Make sure this context is active

            foreach (Texture texture in _texturePool)
                texture.Dispose(); // Dispose each texture we've pooled

            _fbo.Dispose(); // Dispose our FBO

            if (_fromWindowHandle) // We're responsible for this context
            {
                var disposableContext = _glContext as IDisposable;
                if (disposableContext != null)
                    disposableContext.Dispose();
            }

            base.DisposeUnmanaged();
        }

        internal void ReleaseTexture(Texture texture)
        {
            _texturePool.Add(texture); // Just add it to our pool
        }

        internal Texture GetTexture(int width, int height)
        {
            Texture result = null;
            int index = int.MinValue;

            for (int i = 0; i < _texturePool.Count; i++)
            {
                Texture texture = _texturePool[i];
                if ((texture.Width != width) || (texture.Height != height)) 
                    continue;
                
                result = texture;
                index = i;
            }

            if (result != null)
            {
                _texturePool.RemoveAt(index); // Remove this from our pool
                return result;
            }

            return new Texture(_glContext, width, height); // We don't have this pooled, make a new one
        }

        internal static void RenderScreenAlignedQuad(int width, int height)
        {
            // Set up a 2D ortho projection matrix
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix(); // Push the projection matrix
            Gl.glLoadIdentity(); // Set it to identity
            Glu.gluOrtho2D(0, width, 0, height); // Ortho projection matrix

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix(); // Push the modelview matrix
            Gl.glLoadIdentity(); // Set it to identity

            // Identity modelview
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();

            Gl.glViewport(0, 0, width, height);

            Gl.glPolygonMode(Gl.GL_FRONT, Gl.GL_FILL);

            Gl.glBegin(Gl.GL_QUADS);
            Gl.glTexCoord2f(0f, 0f);
            Gl.glVertex2f(0f, 0f);

            Gl.glTexCoord2f(1f, 0f);
            Gl.glVertex2f(width, 0f);

            Gl.glTexCoord2f(1f, 1f);
            Gl.glVertex2f(width, height);

            Gl.glTexCoord2f(0f, 1f);
            Gl.glVertex2f(0f, height);

            Gl.glEnd();

            Gl.glPopMatrix(); // Restore the ModelView matrix

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix(); // Restore the Projection matrix
        }
    }

    [Serializable]
    public class InitializationException: Exception
    {
        public InitializationException(string message)
            : base(message)
        {
        }

        public InitializationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InitializationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}