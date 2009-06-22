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
using System.Globalization;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Brahma.DirectX
{
    public static class Visualizer
    {
        public sealed class ColorMap: IDisposable
        {
            private const string PixelShaderSource = @"
                sampler data;
                sampler colorMap;

                float4 main(float2 texCoord: TEXCOORD0): COLOR
                {
                    return tex1D(colorMap, tex2D(data, texCoord).xy);
                }";

            private readonly PixelShader _pixelShader;
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

                ConstantTable constantTable;
                string errors;
                using (GraphicsStream codeStream = ShaderLoader.CompileShader(PixelShaderSource, "main", null, null, provider.GetMaxPixelShaderVersion().ToString(), ShaderFlags.None, out errors, out constantTable))
                    if (string.IsNullOrEmpty(errors))
                        _pixelShader = new PixelShader(_provider.Device, codeStream);
                    else
                        throw new GraphicsException("Cannot compile ColorMap shader");
            }

            internal ComputationProvider Provider
            {
                get
                {
                    return _provider;
                }
            }

            internal PixelShader PixelShader
            {
                get
                {
                    return _pixelShader;
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
                private set;
            }

            #region IDisposable Members

            public void Dispose()
            {
                if (Disposed)
                    return;

                if (!_pixelShader.Disposed)
                    _pixelShader.Dispose();
                if (!_texture.Disposed)
                    _texture.Dispose();

                Disposed = true;
            }

            #endregion

            // TODO: Add more overloads for FromBitmap, FromStream, FromResource etc.
            public static ColorMap FromFile(ComputationProvider provider, string filename)
            {
                return new ColorMap(provider, TextureLoader.FromFile(provider.Device, filename));
            }

            ~ColorMap()
            {
                Dispose();
            }
        }

        // TODO: Add overloads for all kinds of data-parallel arrays
        public static void Display(ComputationProvider provider, DataParallelArray2D<float> data)
        {
            provider.Device.PixelShader = null; // We don't want pixel shaders

            provider.Device.SetRenderTarget(0, provider.BackBuffer);
            provider.Device.SetTexture(0, data.Texture.Texture);

            provider.RenderScreenAlignedQuad(data.Width, data.Height); // Render 1:1 on the back-buffer
            provider.Device.Present();
        }

        public static void Display(ComputationProvider provider, DataParallelArray2D<float> data, ColorMap colorMap)
        {
            provider.Device.PixelShader = colorMap.PixelShader;

            provider.Device.SetRenderTarget(0, provider.BackBuffer);

            provider.Device.SetTexture(0, data.Texture.Texture);
            provider.Device.SetTexture(1, colorMap.Texture);

            provider.RenderScreenAlignedQuad(provider.BackBuffer.Description.Width, provider.BackBuffer.Description.Height); // Render 1:1 on the back-buffer
            provider.Device.Present();
        }

        public static void Display(ComputationProvider provider, DataParallelArray2D<Vector4> data)
        {
            provider.Device.PixelShader = null; // We don't want pixel shaders

            provider.Device.SetRenderTarget(0, provider.BackBuffer);
            provider.Device.SetTexture(0, data.Texture.Texture);

            provider.RenderScreenAlignedQuad(provider.BackBuffer.Description.Width, provider.BackBuffer.Description.Height); // Render 1:1 on the back-buffer
            provider.Device.Present();
        }
    }
}