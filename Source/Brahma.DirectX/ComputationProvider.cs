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
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Brahma.DirectX
{
    public enum PixelShaderVersion
    {
        ps_2_0,
        ps_3_0
    }

    // TODO: Provider overloads that allow using the control's width and height for the backbuffer, or explicitly specifying them
    public sealed class ComputationProvider: ComputationProviderBase
    {
        private const int DefaultHeight = 512;
        private const int DefaultWidth = 512;

        internal const int OneDimensionalHeight = 1;
        internal const Format TextureFormat = Format.A32B32G32R32F;

        private readonly Device _device;
        private readonly bool _fromWindowHandle;

        private readonly List<CachedSurface> _systemMemSurfacePool = new List<CachedSurface>(); // System memory surface pool
        private readonly List<CachedTexture> _texturePool = new List<CachedTexture>(); // Texture pool

        private int _lastHeight = int.MinValue;

        private int _lastWidth = int.MinValue;
        private VertexBuffer _vertexBuffer;

        private CustomVertex.TransformedTextured[] _vertices;

        public ComputationProvider()
            : this(new Form()) // The form can be garbage-collected, that's alright
        {
        }

        public ComputationProvider(Control control)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            IntPtr windowHandle = control.Handle;
            if (windowHandle == IntPtr.Zero)
                throw new ArgumentException("Invalid window handle");

            _fromWindowHandle = true; // We created this device, we control it and we will be disposing it

            // Create the device now
            _device = new Device(0, DeviceType.Hardware, windowHandle, CreateFlags.HardwareVertexProcessing,
                                 new PresentParameters
                                 {
                                     BackBufferCount = 1,
                                     Windowed = true,
                                     SwapEffect = SwapEffect.Discard,
                                     BackBufferWidth = DefaultWidth,
                                     BackBufferHeight = DefaultHeight,
                                     PresentationInterval = PresentInterval.Immediate,
                                 });
            SetupDevice();
        }

        public ComputationProvider(Device device)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            // We didn't create this device, we shouldn't dispose it
            _device = device;

            SetupDevice();
        }

        internal Surface BackBuffer
        {
            get;
            private set;
        }

        public Device Device
        {
            get
            {
                return _device;
            }
        }

        public PixelShaderVersion PixelShaderVersion
        {
            get;
            set;
        }

        private void SetupDevice()
        {
            // Set up sampler states
            for (int i = 0; i < _device.DeviceCaps.MaxSimultaneousTextures; i++)
            {
                // Clamp all texture addressing
                _device.SetSamplerState(i, SamplerStageStates.AddressU, (int)TextureAddress.Clamp);
                _device.SetSamplerState(i, SamplerStageStates.AddressV, (int)TextureAddress.Clamp);

                // Enable point sampling
                _device.SetSamplerState(i, SamplerStageStates.MagFilter, (int)Filter.Point);
                _device.SetSamplerState(i, SamplerStageStates.MinFilter, (int)Filter.Point);
            }

            // Create the vertex buffer we will use to draw screen-aligned quads
            _vertexBuffer = new VertexBuffer(typeof (CustomVertex.TransformedTextured), 4, _device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.TransformedTextured.Format, Pool.Default);
            BackBuffer = _device.GetBackBuffer(0, 0, BackBufferType.Mono); // Keep a reference to the backbuffer

            PixelShaderVersion = GetMaxPixelShaderVersion();
        }

        internal PixelShaderVersion GetMaxPixelShaderVersion()
        {
            Version maxVersion = _device.DeviceCaps.PixelShaderVersion;
            return (PixelShaderVersion)Enum.Parse(typeof (PixelShaderVersion), string.Format(CultureInfo.InvariantCulture, "ps_{0}_{1}", maxVersion.Major, maxVersion.Minor));
        }

        private sealed class CachedTexture
        {
            public int Width
            {
                get;
                set;
            }

            public int Height
            {
                get;
                set;
            }

            public RenderTargetTexture Texture
            {
                get;
                set;
            }
        }

        private sealed class CachedSurface
        {
            public int Width
            {
                get;
                set;
            }

            public int Height
            {
                get;
                set;
            }

            public Surface Surface
            {
                get;
                set;
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

        internal void RenderScreenAlignedQuad(int width, int height)
        {
            // TODO: Set up projection and world/view matrices appropriately, these may not be identity

            // Don't re-create the vertices if the width and height haven't changed
            if ((_lastWidth != width) || (_lastHeight != height))
            {
                _vertices = new CustomVertex.TransformedTextured[4];

                // The 0.5f is VERY important.
                // Remember, texels are points, not squares like pixels. A texel lies at the center of a pixel.
                // http://www.paradoxalpress.info/Docs/dx9_out/directly_mapping_texels_to_pixels.htm
                _vertices[0].X = 0f - 0.5f;
                _vertices[0].Y = 0f - 0.5f;
                _vertices[1].X = width - 0.5f;
                _vertices[1].Y = 0f - 0.5f;
                _vertices[2].X = width - 0.5f;
                _vertices[2].Y = height - 0.5f;
                _vertices[3].X = 0f - 0.5f;
                _vertices[3].Y = height - 0.5f;

                _vertices[0].Tu = 0f;
                _vertices[0].Tv = 0f;
                _vertices[1].Tu = 1f;
                _vertices[1].Tv = 0f;
                _vertices[2].Tu = 1f;
                _vertices[2].Tv = 1f;
                _vertices[3].Tu = 0f;
                _vertices[3].Tv = 1f;

                _lastWidth = width;
                _lastHeight = height;

                _vertexBuffer.SetData(_vertices, 0, LockFlags.Discard);
            }

            // Draw the quad
            _device.BeginScene();

            _device.SetStreamSource(0, _vertexBuffer, 0);
            _device.VertexFormat = CustomVertex.TransformedTextured.Format;

            _device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);
            _device.EndScene();
        }

        protected override object Execute(Expression expression)
        {
            throw new NotImplementedException(); // This method is never called
        }

        protected override CompiledQuery CompileQuery(LambdaExpression expression)
        {
            // Create an HLSL generator and pass in an ExpressionProcessor to it
            // Although there is an overload where we could pass in an expression tree,
            // We need the expression processor to create the CompiledQuery
            var expressionProcessor = new ExpressionProcessor(expression);
            expressionProcessor.Process(); // Tell the expression processor to process the given expression.
            string hlsl = HLSLGenerator.Generate(expressionProcessor);

            PixelShader pixelShader;
            ConstantTable constantTable;
            string errors = string.Empty;
            try
            {
                using (GraphicsStream codeStream = ShaderLoader.CompileShader(hlsl, HLSLGenerator.EntryPoint, null, null, PixelShaderVersion.ToString(), ShaderFlags.None, out errors, out constantTable))
                    if (errors == string.Empty)
                        pixelShader = new PixelShader(_device, codeStream);
                    else
                        throw new GraphicsException("Errors in shader source code");
            }
            catch (GraphicsException ex) // Should I just catch GraphicsException, or Exception here?
            {
                errors = string.Format("Could not compile pixel shader, exception was: {0}\nThe shader compiler said: {1}", ex.Message, errors);
                throw new InvalidProgramException(string.Format(CultureInfo.InvariantCulture, "The generated HLSL was invalid. You should not be seeing this error.\n" +
                                                                                              "Please report this error to ananth<at>ananthonline<dot>net, along with the query that generated it.\n" +
                                                                                              "{0}\n" +
                                                                                              "Shader source:\n" +
                                                                                              "{1}", errors, hlsl));
            }

            // Get the query parameter types so CompiledQuery knows what to expect when run
            IEnumerable<Type> queryParameterTypes = from ParameterExpression parameter in expressionProcessor.QueryParameters
                                                    select parameter.Type;

            return new DXCompiledQuery(_device, pixelShader, constantTable, expressionProcessor.MemberAccess.ToArray(), expression.Body.Type, queryParameterTypes.ToArray());
        }

        protected override IQueryable RunQuery(CompiledQuery query, DataParallelArrayBase[] arguments)
        {
            var q = query as DXCompiledQuery;
            if (q == null) // Make sure it IS a Brahma.DirectX.CompiledQuery
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "{0} cannot execute query of type {1}", GetType().FullName, query.GetType().FullName));

            if (arguments.Length != q.ParameterTypes.Length) // Make sure we have the correct number of arguments
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Expected {0} arguments, found {1} instead", q.ParameterTypes.Length, arguments.Length));

            _device.PixelShader = q.PixelShader; // Set the pixel shader, we're going to be setting up textures next

            for (int i = 0; i < arguments.Length; i++) // Make sure all the types are what we expect, too
            {
                if (arguments[i] == null) // Make sure none of the arguments are null
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Parameter {0} was null. Valid reference expected", i));

                if (!q.ParameterTypes[i].IsAssignableFrom(arguments[i].GetType())) // Is is of the correct type, or assignable to the correct type?
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Expected argument {0} to be of type {1}, found {2} instead",
                                                              i, q.ParameterTypes[i].FullName, arguments[i].GetType().FullName));

                _device.SetTexture(i, ((ISampler)arguments[i]).Texture.Texture); // Set up the texture

                // Set up the addressing mode for columns (U) and rows (V)
                var addressable = arguments[i] as IAddressable;
                _device.SetSamplerState(i, SamplerStageStates.AddressU, addressable.ColumnAddressingMode.ToDXAddressingMode());
                _device.SetSamplerState(i, SamplerStageStates.AddressV, addressable.RowAddressingMode.ToDXAddressingMode());
            }

            // Finally, set up the shader constants here
            foreach (MemberExpression memberExp in q.ShaderConstants)
            {
                if (memberExp.Type == typeof (float))
                    q.Parameters<float>(memberExp.Member.Name).Value =
                        memberExp.Expression == null // Is this a static field?
                            ? (float)((FieldInfo)memberExp.Member).GetValue(null) // Yes it is. We don't need an instance
                            : (float)((FieldInfo)memberExp.Member).GetValue(((ConstantExpression)memberExp.Expression).Value); // We need an instance. Get it from memberExp.Expression

                if (memberExp.Type == typeof (Vector2))
                    q.Parameters<Vector2>(memberExp.Member.Name).Value =
                        memberExp.Expression == null // Is this a static field?
                            ? (Vector2)((FieldInfo)memberExp.Member).GetValue(null) // Yes it is. We don't need an instance
                            : (Vector2)((FieldInfo)memberExp.Member).GetValue(((ConstantExpression)memberExp.Expression).Value); // We need an instance. Get it from memberExp.Expression

                if (memberExp.Type == typeof (Vector3))
                    q.Parameters<Vector3>(memberExp.Member.Name).Value =
                        memberExp.Expression == null // Is this a static field?
                            ? (Vector3)((FieldInfo)memberExp.Member).GetValue(null) // Yes it is. We don't need an instance
                            : (Vector3)((FieldInfo)memberExp.Member).GetValue(((ConstantExpression)memberExp.Expression).Value); // We need an instance. Get it from memberExp.Expression

                if (memberExp.Type == typeof (Vector4))
                    q.Parameters<Vector4>(memberExp.Member.Name).Value =
                        memberExp.Expression == null // Is this a static field?
                            ? (Vector4)((FieldInfo)memberExp.Member).GetValue(null) // Yes it is. We don't need an instance
                            : (Vector4)((FieldInfo)memberExp.Member).GetValue(((ConstantExpression)memberExp.Expression).Value); // We need an instance. Get it from memberExp.Expression
            }

            // *** IMPORTANT ***
            // In Brahma, given multiple data-parallel arrays (say: d1, d2, d3), the output array will have dimensions
            // min(d1.Width, d2.Width, d3.Width) X min(d1.Height, d2.Height, d3.Height). All operations will then run element-wise
            // on the output array. Extra elements on any of the data-parallel arrays will not be processed!
            // All "special" functionality that is not element-wise needs to be implemented as operators or extension methods on data-parallel arrays
            // Example: Given 3 data-parallel arrays of dimensions d1 = 3x3 d2 = 2x3 d3 = 10x1, the result would have dimensions min(2, 2, 10) x min(3, 3, 1) = 2 x 1
            // so, only two columns and one row will be processed on ALL the data-parallel arrays. The rest of the elements will NOT be processed.
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

            // Set up the shader constants we've added for internal use
            q.Parameters<float>("_brahma_minWidth", true).Value = width;
            q.Parameters<float>("_brahma_minHeight", true).Value = height;
            q.Parameters<Vector2[]>("_brahma_invDimensions", true).Value = invDimensions.ToArray();

            var resultSampler = result as ISampler;
            if (resultSampler != null) // This check might be redundant since we're creating this object ourselves, and we know what type it is
                _device.SetRenderTarget(0, resultSampler.Texture.Surface); // Set result as the current rendertarget

            RenderScreenAlignedQuad(width, height); // Render the screen aligned quad

            return result;
        }

        protected override void DisposeUnmanaged()
        {
            _vertexBuffer.Dispose(); // Dispose the vertex buffer, we control it

            // Dipose all cached textures
            for (int i = 0; i < _texturePool.Count; i++)
                if (!_texturePool[i].Texture.Texture.Disposed) // Make sure it isn't disposed already
                    _texturePool[i].Texture.Texture.Dispose();
            _texturePool.Clear(); // Clear the texture pool

            // Dispose all system memory surfaces
            for (int i = 0; i < _systemMemSurfacePool.Count; i++)
                if (!_systemMemSurfacePool[i].Surface.Disposed) // Make sure it isn't disposed already
                    _systemMemSurfacePool[i].Surface.Dispose();
            _systemMemSurfacePool.Clear(); // Clear the surface pool

            if (_fromWindowHandle) // Dispose the device ONLY IF we created it
                _device.Dispose();

            base.DisposeUnmanaged();
        }

        internal void ReleaseTexture(RenderTargetTexture texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");

            // Don't dispose this texture, add it to our pool
            _texturePool.Add(new CachedTexture
                             {
                                 Width = texture.Surface.Description.Width,
                                 Height = texture.Surface.Description.Height,
                                 Texture = texture
                             });
        }

        internal RenderTargetTexture GetTexture(int width, int height)
        {
            CachedTexture result = null;
            int index = int.MinValue;

            // Do we have a pooled texture that matches this description ?
            for (int i = 0; i < _texturePool.Count; i++)
            {
                CachedTexture texture = _texturePool[i];
                if ((texture.Width == width) && (texture.Height == height))
                {
                    result = texture; // Yes
                    index = i;
                    break;
                }
            }

            if (result != null)
            {
                _texturePool.RemoveAt(index); // Remove it from our pool and return it
                return result.Texture;
            }

            return new RenderTargetTexture(new Texture(_device, width, height, 1, Usage.RenderTarget, TextureFormat, Pool.Default)); // We don't, create one now
        }

        internal void ReleaseSystemMemSurface(Surface surface)
        {
            if (surface == null)
                throw new ArgumentNullException("surface");

            if (surface.Description.Pool != Pool.SystemMemory)
                throw new ArgumentException("Can only pool system memory surfaces");

            // Don't dispose this surface, add it to our pool
            _systemMemSurfacePool.Add(new CachedSurface
                                      {
                                          Width = surface.Description.Width,
                                          Height = surface.Description.Height,
                                          Surface = surface
                                      });
        }

        internal Surface GetSystemMemSurface(int width, int height)
        {
            CachedSurface result = null;
            int index = int.MinValue;

            // Do we have a pooled system memory surface that matches this description ?
            for (int i = 0; i < _systemMemSurfacePool.Count; i++)
            {
                CachedSurface surface = _systemMemSurfacePool[i];
                if ((surface.Width == width) && (surface.Height == height))
                {
                    result = surface;
                    index = 0;
                    break;
                }
            }

            if (result != null)
            {
                _systemMemSurfacePool.RemoveAt(index); // Remove it from our pool and return it
                return result.Surface;
            }

            return _device.CreateOffscreenPlainSurface(width, height, TextureFormat, Pool.SystemMemory); // We can't find one of these dimensions, create one
        }
    }
}