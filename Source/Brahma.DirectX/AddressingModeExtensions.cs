﻿#region License and Copyright Notice

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

using Microsoft.DirectX.Direct3D;

namespace Brahma.DirectX
{
    internal static class AddressingModeExtensions
    {
        public static int ToDXAddressingMode(this AddressingMode addressingMode)
        {
            switch (addressingMode)
            {
                case AddressingMode.Clamp:
                    return (int)TextureAddress.Clamp;
                case AddressingMode.Repeat:
                    return (int)TextureAddress.Wrap;

                default:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The addressing mode {0} cannot be converted to an equivalent DirectX addressing mode", addressingMode));
            }
        }
    }
}