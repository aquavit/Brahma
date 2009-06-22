using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

using Brahma.Platform.OpenGL;

using Tao.OpenGl;

namespace Brahma.OpenGL
{
    internal sealed class Texture: IDisposable
    {
        private const int DefaultColorFormat = Gl.GL_RGBA;
        private const int DefaultDataType = Gl.GL_FLOAT;
        private const int DefaultInternalFormat = Gl.GL_RGBA32F_ARB;
        private const int TextureTarget = Gl.GL_TEXTURE_2D;

        private Texture()
        {
        }

        public Texture(ContextBase context, int width, int height, Vector4[] data)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            Context = context;

            Width = width;
            Height = height;

            // Enable texturing
            Gl.glEnable(Gl.GL_TEXTURE_1D);
            Gl.glEnable(Gl.GL_TEXTURE_2D);

            TextureId = context.GetNewTextureId(true); // Create the texture id, register it for disposal
            Gl.glBindTexture(TextureTarget, TextureId); // bind the texture to the default texture target

            // Turn off filtering and set proper wrap mode 
            // (obligatory for float textures at the moment)
            Gl.glTexParameteri(TextureTarget, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
            Gl.glTexParameteri(TextureTarget, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
            Gl.glTexParameteri(TextureTarget, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP);
            Gl.glTexParameteri(TextureTarget, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP);

            // set texenv to replace instead of the default modulate
            Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_REPLACE);

            // and allocate graphics memory
            if (data == null)
                Gl.glTexImage2D(TextureTarget, 0, DefaultInternalFormat,
                                width, height, 0, DefaultColorFormat, DefaultDataType, IntPtr.Zero); // No data to send
            else
                Gl.glTexImage2D(TextureTarget, 0, DefaultInternalFormat,
                                width, height, 0, DefaultColorFormat, DefaultDataType, data); // Send the data we've been given
        }

        public Texture(ContextBase context, int width, int height)
            : this(context, width, height, null)
        {
        }

        internal int TextureId
        {
            get;
            private set;
        }

        public bool Disposed
        {
            get;
            set;
        }

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

        public ContextBase Context
        {
            get;
            private set;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (Disposed)
                return;

            Context = null; // Don't hold a reference to this context

            GC.SuppressFinalize(this);
            Disposed = true;
        }

        #endregion

        internal void SetData(Vector4[] data)
        {
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, TextureId);
            Gl.glTexSubImage2D(Gl.GL_TEXTURE_2D, 0, 0, 0, Width, Height,
                               DefaultColorFormat, DefaultDataType, data);
        }

        internal Vector4[] GetData()
        {
            var outputData = new Vector4[Width * Height]; // Create an array to hold the output values

            // Note: The reason the glGetTexImage method is used is because the glReadBuffer method requires the FBO to be bound
            //Gl.glReadBuffer(Gl.GL_COLOR_ATTACHMENT0_EXT);
            //Gl.glReadPixels(0, 0, Width, Height, Gl.GL_RGBA, Gl.GL_FLOAT, outputData); // Read back the data from the FBO using glReadPixels

            Gl.glBindTexture(Gl.GL_TEXTURE_2D, TextureId);
            Gl.glGetTexImage(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, Gl.GL_FLOAT, outputData); // Read back the data from the texture using glGetTexImage

            return outputData;
        }

        ~Texture()
        {
            Dispose();
        }

        public static Texture FromFile(ContextBase context, string filename)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (filename == null)
                throw new ArgumentNullException("filename");
            if (!File.Exists(filename))
                throw new IOException(string.Format(CultureInfo.InvariantCulture, "Cannot file file {0}", filename));

            var bitmap = Image.FromFile(filename) as Bitmap; // Load the bitmap
            if (bitmap == null)
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                                                                  "Could not load texture from {0}", filename));
            var texture = new Texture // Create a texture
                              {
                                  Width = bitmap.Width,
                                  Height = bitmap.Height
                              };
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                 ImageLockMode.ReadOnly,
                                                 System.Drawing.Imaging.PixelFormat.Format32bppArgb); // Get an IntPtr containing pixel data
            int textureId = context.GetNewTextureId(true); // Generate the texture id
            Gl.glBindTexture(TextureTarget, textureId);

            // Turn off filtering and set proper wrap mode 
            Gl.glTexParameteri(TextureTarget, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
            Gl.glTexParameteri(TextureTarget, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
            Gl.glTexParameteri(TextureTarget, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP);
            Gl.glTexParameteri(TextureTarget, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP);

            // set texenv to replace instead of the default modulate
            Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_REPLACE);

            Gl.glTexImage2D(TextureTarget, 0, Gl.GL_RGB8, bitmap.Width, bitmap.Height, 0, Gl.GL_BGRA_EXT,
                            Gl.GL_UNSIGNED_BYTE, bmpData.Scan0); // Pass the data in to OpenGL

            bitmap.UnlockBits(bmpData);
            bitmap.Dispose(); // Remember to unlock and dispose the bitmap

            texture.TextureId = textureId;
            return texture; // We're done
        }
    }
}