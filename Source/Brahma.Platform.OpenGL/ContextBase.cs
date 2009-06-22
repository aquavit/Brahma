﻿using System;
using System.Collections.Generic;

using Tao.OpenGl;

namespace Brahma.Platform.OpenGL
{
    internal abstract class ContextBase: IGLContext, IGLResourceProvider, IDisposable
    {
        private readonly List<Resource> _forDisposal = new List<Resource>();

        #region IGLResourceProvider members

        public int GetNewTextureId(bool registerForDisposal)
        {
            int result;
            Gl.glGenTextures(1, out result);

            if (registerForDisposal)
                _forDisposal.Add(new Resource { Type = ResourceType.TextureId, Handle = result });

            return result;
        }

        public int GetNewVertexShaderObject(bool registerForDisposal)
        {
            int result = Gl.glCreateShaderObjectARB(Gl.GL_VERTEX_SHADER_ARB);

            if (registerForDisposal)
                _forDisposal.Add(new Resource { Type = ResourceType.VertexShaderObject, Handle = result });

            return result;
        }

        public int GetNewFragmentShaderObject(bool registerForDisposal)
        {
            int result = Gl.glCreateShaderObjectARB(Gl.GL_FRAGMENT_SHADER_ARB);

            if (registerForDisposal)
                _forDisposal.Add(new Resource { Type = ResourceType.FragmentShaderObject, Handle = result });

            return result;
        }

        public int GetNewProgramObject(bool registerForDisposal)
        {
            int result = Gl.glCreateProgramObjectARB();

            if (registerForDisposal)
                _forDisposal.Add(new Resource { Type = ResourceType.ProgramObject, Handle = result });

            return result;
        }

        public int GetNewFrameBufferObject(bool registerForDisposal)
        {
            int result;
            Gl.glGenFramebuffersEXT(1, out result);

            if (registerForDisposal)
                _forDisposal.Add(new Resource { Type = ResourceType.FrameBufferObject, Handle = result });

            return result;
        }

        #endregion

        public bool Disposed
        {
            get;
            set;
        }

        #region IGLContext Members

        public abstract void SwapBuffers();

        public abstract void MakeCurrent();

        public abstract bool IsCurrent
        {
            get;
        }

        #endregion

        private enum ResourceType
        {
            TextureId,
            VertexShaderObject,
            FragmentShaderObject,
            ProgramObject,
            FrameBufferObject
        }

        private struct Resource
        {
            public int Handle;
            public ResourceType Type;
        }

        private void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
                DisposeUnmanaged();
            else
            {
                DisposeUnmanaged();
                DisposeManaged();
            }

            Disposed = true;
        }

        protected virtual void DisposeUnmanaged() // Override this to dispose finalizables we control
        {
            // We're going to destroy any resources we were asked to dispose
            foreach (Resource resource in _forDisposal)
            {
                switch (resource.Type)
                {
                    case ResourceType.TextureId:
                        {
                            int handle = resource.Handle;
                            Gl.glDeleteTextures(1, ref handle);
                        }

                        break;

                    case ResourceType.VertexShaderObject:
                    case ResourceType.FragmentShaderObject:
                    case ResourceType.ProgramObject:
                        Gl.glDeleteObjectARB(resource.Handle);

                        break;

                    case ResourceType.FrameBufferObject:
                        {
                            int handle = resource.Handle;
                            Gl.glDeleteFramebuffersEXT(1, ref handle);
                        }

                        break;
                }
            }

            _forDisposal.Clear();
        }

        protected virtual void DisposeManaged() // Override this to dispose shared resources and other managed resources
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ContextBase()
        {
            Dispose(false);
        }
    }
}