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

using Microsoft.DirectX.Direct3D;

namespace Brahma.DirectX
{
    // This class is a wrapper for a texture and its associated surface. This what even gets pooled because
    // each time we call GetSurfaceLevel(0) we get a new surface. This degrades performance a lot
    internal sealed class RenderTargetTexture
    {
        private readonly Texture _texture;
        private readonly Surface _textureSurface;

        public RenderTargetTexture(Texture texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");

            _texture = texture;
            _textureSurface = _texture.GetSurfaceLevel(0); // Do this only once per render target
        }

        public Texture Texture
        {
            get
            {
                return _texture;
            }
        }

        public Surface Surface
        {
            get
            {
                return _textureSurface;
            }
        }
    }
}