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

using Tao.OpenGl;

namespace Brahma.OpenGL
{
    public sealed class Visualizer
    {
        public sealed class ColorMap: IDisposable
        {
            private const string PixelShaderSource = @"
                sampler2D data;
                sampler2D colorMap;

                void main()
                {
                    gl_FragColor = texture2D(colorMap, texture2D(data, gl_TexCoord[0]).xy);
                }";

            private readonly FragmentShader _fragmentShader;
            private readonly ProgramObject _programObject;
            private readonly ComputationProvider _provider;
            private readonly Texture _texture;

            private ColorMap(ComputationProvider provider, Texture texture)
            {
                if (provider == null)
                    throw new ArgumentNullException("provider");
                if (texture == null)
                    throw new ArgumentNullException("texture");

                _provider = provider;
                _texture = texture;

                _fragmentShader = new FragmentShader(provider.Context, PixelShaderSource);
                CompileResult compileResult = _fragmentShader.Compile();

                // None of the below exceptions should be thrown if our hard-coded shader is right
                if (!(bool)compileResult)
                    throw new InvalidOperationException("Cannot compile ColorMap shader");

                _programObject = new ProgramObject(provider.Context, _fragmentShader);
                LinkResult linkResult = _programObject.Link();

                if (!(bool)linkResult)
                    throw new InvalidOperationException("Cannot link ColorMap program");
            }

            internal ComputationProvider Provider
            {
                get
                {
                    return _provider;
                }
            }

            internal ProgramObject ProgramObject
            {
                get
                {
                    return _programObject;
                }
            }

            internal Texture Texture
            {
                get
                {
                    return _texture;
                }
            }

            public bool Disposed
            {
                get;
                set;
            }

            #region IDisposable Members

            public void Dispose()
            {
                if (Disposed)
                    return;

                if (!_fragmentShader.Disposed)
                    _fragmentShader.Dispose();
                if (!_texture.Disposed)
                    _texture.Dispose();

                Disposed = true;
            }

            #endregion

            // TODO: Add more overloads for FromBitmap, FromStream, FromResource etc.
            public static ColorMap FromFile(ComputationProvider provider, string filename)
            {
                return new ColorMap(provider, Texture.FromFile(provider.Context, filename));
            }

            ~ColorMap()
            {
                Dispose();
            }
        }

        // TODO: Add overloads for all kinds of data-parallel arrays
        public static void Display(ComputationProvider provider, DataParallelArray2D<float> data)
        {
            FrameBufferObject.Disable(); // We don't want to render to a framebuffer object

            Gl.glUseProgramObjectARB(ProgramObject.None); // We don't want pixel shaders

            Gl.glActiveTexture(Gl.GL_TEXTURE0);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, data.Texture.TextureId); // Enable the data, we're going to render it directly

            ComputationProvider.RenderScreenAlignedQuad(data.Width, data.Height); // Render 1:1 on the back-buffer
            provider.Context.SwapBuffers(); // Present the image
        }

        public static void Display(ComputationProvider provider, DataParallelArray2D<float> data, ColorMap colorMap)
        {
            FrameBufferObject.Disable(); // We don't want to render to a framebuffer object

            Gl.glUseProgramObjectARB(colorMap.ProgramObject.Handle); // Use the pixel shader attached to this program

            // Set up the textures
            Gl.glActiveTexture(Gl.GL_TEXTURE0);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, data.Texture.TextureId);
            Gl.glActiveTexture(Gl.GL_TEXTURE1);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, colorMap.Texture.TextureId);

            // Set up parameters on the program object
            colorMap.ProgramObject.Parameters<int>("data").Value = 0;
            colorMap.ProgramObject.Parameters<int>("colorMap").Value = 1;

            ComputationProvider.RenderScreenAlignedQuad(data.Width, data.Height); // Render 1:1 on the back-buffer
            provider.Context.SwapBuffers(); // Present the image
        }

        public static void Display(ComputationProvider provider, DataParallelArray2D<Vector4> data)
        {
            FrameBufferObject.Disable(); // We don't want to render to a framebuffer object

            Gl.glUseProgramObjectARB(ProgramObject.None); // We don't want pixel shaders

            FrameBufferObject.Disable(); // We don't want to render to a framebuffer object
            Gl.glActiveTexture(Gl.GL_TEXTURE0);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, data.Texture.TextureId); // Enable the data, we're going to render it directly

            ComputationProvider.RenderScreenAlignedQuad(data.Width, data.Height); // Render 1:1 on the back-buffer
            provider.Context.SwapBuffers(); // Present the image
        }
    }
}