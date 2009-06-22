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

using Tao.OpenGl;

namespace Brahma.OpenGL
{
    // Classes and enums that expose a myriad of functionality for pixel formats and other OpenGL integer codes
    public enum PixelFormat
    {
        Unknown,

        Rgb16 = Gl.GL_RGB16,
        Rgb24 = Gl.GL_RGB8,

        Rgba32 = Gl.GL_RGBA8,

        Alpha8 = Gl.GL_ALPHA8,
        Alpha16 = Gl.GL_ALPHA16,

        Luminance8 = Gl.GL_LUMINANCE8,
        Luminance16 = Gl.GL_LUMINANCE16,

        RgbFloat = Gl.GL_RGB32F_ARB,

        RgbaFloat = Gl.GL_RGBA32F_ARB
    }

    public enum ColorFormat
    {
        Unknown,

        Rgb = Gl.GL_RGB,
        Rgba = Gl.GL_RGBA,
        Luminance = Gl.GL_LUMINANCE,
        Alpha = Gl.GL_ALPHA
    }

    public enum DepthFormat
    {
        None = 0,
        D8 = 8,
        D16 = 16,
        D24 = 24,
        D32 = 32
    }

    public enum StencilFormat
    {
        None = 0,
        S8 = 8,
        S16 = 16
    }

    public enum DataType
    {
        Unknown = 0,
        Unsigned8 = Gl.GL_UNSIGNED_BYTE,
        Unsigned16 = Gl.GL_UNSIGNED_SHORT,
        Float32 = Gl.GL_FLOAT
    }

    public enum EndianOrder
    {
        LittleEndian,
        BigEndian
    }

    public static class FormatExtensions
    {
        public static int GetBitDepth(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Alpha16:
                    return 16;

                case PixelFormat.Alpha8:
                    return 8;

                case PixelFormat.Luminance16:
                    return 16;

                case PixelFormat.Luminance8:
                    return 8;

                case PixelFormat.Rgb16:
                    return 16;

                case PixelFormat.Rgb24:
                    return 24;

                case PixelFormat.Rgba32:
                    return 32;

                case PixelFormat.RgbFloat:
                    return 32 * 3;

                case PixelFormat.RgbaFloat:
                    return 32 * 4;

                default:
                    return 0;
            }
        }

        public static int GetOnlyColorDepth(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Alpha16:
                    return 0;

                case PixelFormat.Alpha8:
                    return 0;

                case PixelFormat.Luminance16:
                    return 0;

                case PixelFormat.Luminance8:
                    return 0;

                case PixelFormat.Rgb16:
                    return 16;

                case PixelFormat.Rgb24:
                    return 24;

                case PixelFormat.Rgba32:
                    return 24;

                case PixelFormat.RgbFloat:
                    return 32 * 3;

                case PixelFormat.RgbaFloat:
                    return 32 * 3;

                default:
                    return 0;
            }
        }

        public static int GetOnlyAlphaDepth(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Alpha16:
                    return 16;

                case PixelFormat.Alpha8:
                    return 8;

                case PixelFormat.Luminance16:
                    return 0;

                case PixelFormat.Luminance8:
                    return 0;

                case PixelFormat.Rgb16:
                    return 0;

                case PixelFormat.Rgb24:
                    return 0;

                case PixelFormat.Rgba32:
                    return 8;

                case PixelFormat.RgbFloat:
                    return 0;

                case PixelFormat.RgbaFloat:
                    return 32;

                default:
                    return 0;
            }
        }

        public static bool IsFloatingPoint(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Alpha16:
                case PixelFormat.Alpha8:
                case PixelFormat.Luminance16:
                case PixelFormat.Luminance8:
                case PixelFormat.Rgb16:
                case PixelFormat.Rgb24:
                case PixelFormat.Rgba32:
                    return false;

                case PixelFormat.RgbFloat:
                case PixelFormat.RgbaFloat:
                    return true;

                default:
                    return false;
            }
        }

        public static int GetBitDepth(this DepthFormat depthFormat)
        {
            return (int)depthFormat;
        }

        public static int GetBitDepth(this StencilFormat stencilFormat)
        {
            return (int)stencilFormat;
        }

        public static int GetInternalFormat(this PixelFormat pixelFormat)
        {
            return (int)pixelFormat;
        }

        public static int GetInternalFormat(this DepthFormat depthFormat)
        {
            switch (depthFormat)
            {
                case DepthFormat.None:
                    return 0;

                case DepthFormat.D8:
                    return Gl.GL_DEPTH_COMPONENT;

                case DepthFormat.D16:
                    return Gl.GL_DEPTH_COMPONENT16;

                case DepthFormat.D24:
                    return Gl.GL_DEPTH_COMPONENT24;

                case DepthFormat.D32:
                    return Gl.GL_DEPTH_COMPONENT32;

                default:
                    return 0;
            }
        }

        public static ColorFormat GetColorFormat(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Alpha16:
                case PixelFormat.Alpha8:
                    return ColorFormat.Alpha;

                case PixelFormat.Luminance16:
                case PixelFormat.Luminance8:
                    return ColorFormat.Luminance;

                case PixelFormat.Rgb24:
                case PixelFormat.Rgb16:
                case PixelFormat.RgbFloat:
                    return ColorFormat.Rgb;

                case PixelFormat.Rgba32:
                case PixelFormat.RgbaFloat:
                    return ColorFormat.Rgba;

                default:
                    return ColorFormat.Unknown;
            }
        }

        public static DataType GetDataType(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Rgb16:
                case PixelFormat.Rgb24:
                case PixelFormat.Rgba32:
                case PixelFormat.Luminance8:
                case PixelFormat.Alpha8:
                    return DataType.Unsigned8;

                case PixelFormat.Alpha16:
                case PixelFormat.Luminance16:
                    return DataType.Unsigned16;

                case PixelFormat.RgbaFloat:
                case PixelFormat.RgbFloat:
                    return DataType.Float32;

                default:
                    return DataType.Unknown;
            }
        }
    }
}