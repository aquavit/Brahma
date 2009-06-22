using System;

using Brahma.Platform.OpenGL;

using Tao.OpenGl;

namespace Brahma.OpenGL
{
    internal sealed class FrameBufferObject: IDisposable
    {
        private static int _maxColorAttachments = int.MinValue;

        private readonly Indexable<int, int> _attachedId;
        private readonly Indexable<int, int> _attachedType;

        private readonly int _fboId = int.MinValue;
        private int _savedFboId;

        public FrameBufferObject(ContextBase context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            Context = context;

            _fboId = context.GetNewFrameBufferObject(true);

            // Set up the indexable that finds out what is attached at an attachment point
            _attachedType = new Indexable<int, int>(attachment =>
                                                        {
                                                            Bind();
                                                            int type;
                                                            Gl.glGetFramebufferAttachmentParameterivEXT(Gl.GL_FRAMEBUFFER_EXT, attachment,
                                                                                                        Gl.GL_FRAMEBUFFER_ATTACHMENT_OBJECT_TYPE_EXT,
                                                                                                        out type);
                                                            return type;
                                                        });

            // Set up the indexable that gets the id of the object at an attachment-point
            _attachedId = new Indexable<int, int>(attachment =>
                                                      {
                                                          Bind();
                                                          int id;
                                                          Gl.glGetFramebufferAttachmentParameterivEXT(Gl.GL_FRAMEBUFFER_EXT, attachment,
                                                                                                      Gl.GL_FRAMEBUFFER_ATTACHMENT_OBJECT_NAME_EXT,
                                                                                                      out id);
                                                          Unbind();
                                                          return id;
                                                      });
        }

        public bool Disposed
        {
            get;
            set;
        }

        public Indexable<int, int> AttachedType
        {
            get
            {
                return _attachedType;
            }
        }

        public Indexable<int, int> AttachedId
        {
            get
            {
                return _attachedId;
            }
        }

        public bool Valid
        {
            get
            {
                Bind();
                bool result = Gl.glCheckFramebufferStatusEXT(Gl.GL_FRAMEBUFFER_EXT) == Gl.GL_FRAMEBUFFER_COMPLETE_EXT;
                Unbind();

                return result;
            }
        }

        public string Error
        {
            get
            {
                Bind();

                string result; // Someplace to store the result
                switch (Gl.glCheckFramebufferStatusEXT(Gl.GL_FRAMEBUFFER_EXT))
                {
                    case Gl.GL_FRAMEBUFFER_COMPLETE_EXT: // Everything's OK
                        result = string.Empty;
                        break;

                    case Gl.GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT_EXT:
                        result = "GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT_EXT";
                        break;

                    case Gl.GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT_EXT:
                        result = "GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT_EXT";
                        break;

                    case Gl.GL_FRAMEBUFFER_INCOMPLETE_DIMENSIONS_EXT:
                        result = "GL_FRAMEBUFFER_INCOMPLETE_DIMENSIONS_EXT";
                        break;

                    case Gl.GL_FRAMEBUFFER_INCOMPLETE_FORMATS_EXT:
                        result = "GL_FRAMEBUFFER_INCOMPLETE_FORMATS_EXT";
                        break;

                    case Gl.GL_FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER_EXT:
                        result = "GL_FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER_EXT";
                        break;

                    case Gl.GL_FRAMEBUFFER_INCOMPLETE_READ_BUFFER_EXT:
                        result = "GL_FRAMEBUFFER_INCOMPLETE_READ_BUFFER_EXT";
                        break;

                    case Gl.GL_FRAMEBUFFER_UNSUPPORTED_EXT:
                        result = "GL_FRAMEBUFFER_UNSUPPORTED_EXT";
                        break;

                    default:
                        result = "Unknown error"; // We don't know what happened
                        break;
                }

                Unbind();

                return result;
            }
        }

        public ContextBase Context
        {
            get;
            private set;
        }

        public static int MaxColorAttachments
        {
            get
            {
                if (_maxColorAttachments == int.MinValue)
                    Gl.glGetIntegerv(Gl.GL_MAX_COLOR_ATTACHMENTS_EXT, out _maxColorAttachments);

                return _maxColorAttachments;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (Disposed)
                return;

            Context = null; // Don't hold a reference to the context

            GC.SuppressFinalize(this);
            Disposed = true;
        }

        #endregion

        private void Bind()
        {
            Gl.glGetIntegerv(Gl.GL_FRAMEBUFFER_BINDING_EXT, out _savedFboId); // Get the currently bound FBO
            if (_fboId != _savedFboId)
                Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, _fboId); // It's different, bind
        }

        private void Unbind()
        {
            if (_fboId != _savedFboId)
                Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, _savedFboId); // Restore to the old fbo
        }

        private static void FrameBufferTextureND(int target, int textureId, int attachment, int mipLevel, int zSlice)
        {
            switch (target)
            {
                case Gl.GL_TEXTURE_1D:
                    Gl.glFramebufferTexture1DEXT(Gl.GL_FRAMEBUFFER_EXT, attachment,
                                                 Gl.GL_TEXTURE_1D, textureId, mipLevel);
                    break;
                case Gl.GL_TEXTURE_3D:
                    Gl.glFramebufferTexture3DEXT(Gl.GL_FRAMEBUFFER_EXT, attachment,
                                                 Gl.GL_TEXTURE_3D, textureId, mipLevel, zSlice);
                    break;
                default:
                    Gl.glFramebufferTexture2DEXT(Gl.GL_FRAMEBUFFER_EXT, attachment,
                                                 target, textureId, mipLevel);
                    break;
            }
        }

        public void Attach(int target, int textureId)
        {
            Attach(target, textureId, Gl.GL_COLOR_ATTACHMENT0_EXT, 0, 0);
        }

        public void Attach(int target, int textureId, int attachment)
        {
            Attach(target, textureId, attachment, 0, 0);
        }

        public void Attach(int target, int textureId, int attachment, int mipLevel, int zSlice)
        {
            Bind();

            if (AttachedId[attachment] != textureId)
                FrameBufferTextureND(target, textureId, attachment, mipLevel, zSlice);

            Unbind();
        }

        public void Detach(int attachment)
        {
            Bind(); // Make sure our FBO is set
            switch (AttachedType[attachment])
            {
                case Gl.GL_RENDERBUFFER_EXT:
                    throw new NotSupportedException("Renderbuffer attachments are not supported");

                case Gl.GL_TEXTURE:
                    Attach(Gl.GL_TEXTURE_2D, 0, attachment); // un-bind the attachment

                    break;

                default:
                    throw new NotSupportedException("Unknown attachment type");
            }

            Unbind();
        }

        public void Enable()
        {
            Bind();
        }

        public static void Disable()
        {
            Gl.glBindFramebufferEXT(Gl.GL_FRAMEBUFFER_EXT, 0);
        }

        ~FrameBufferObject()
        {
            Dispose();
        }
    }
}