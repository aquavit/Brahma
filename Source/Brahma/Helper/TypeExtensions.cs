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
using System.Linq;

namespace Brahma.Helper
{
    public static class TypeExtensions
    {
        private static readonly Type _dataParallelArrayBaseType = typeof(DataParallelArrayBase);
        private static readonly Type[] _dataParallelArray1DBaseTypes;
        private static readonly Type[] _dataParallelArray2DBaseTypes;

        static TypeExtensions()
        {
            _dataParallelArray1DBaseTypes = (from Type allowedType in DataParallelArrayBase.AllowedTypes
                                            select typeof (DataParallelArray1DBase<>).MakeGenericType(allowedType)).ToArray();
            _dataParallelArray2DBaseTypes = (from Type allowedType in DataParallelArrayBase.AllowedTypes
                                             select typeof(DataParallelArray2DBase<>).MakeGenericType(allowedType)).ToArray();
        }

        public static bool IsAnonymous(this Type type)
        {
            return type.Name.StartsWith("<>f__AnonymousType");
        }

        // TODO: Change all calls that use IsAssignableFrom to use DerivesFrom
        public static bool DerivesFrom(this Type type, Type baseType)
        {
            return type.IsSubclassOf(baseType);
        }

        public static bool IsDataParallelArray(this Type type)
        {
            return type.DerivesFrom(_dataParallelArrayBaseType);
        }

        public static bool IsDataParallelArray1D(this Type type)
        {
            foreach (Type allowedType in _dataParallelArray1DBaseTypes)
                if (allowedType.IsAssignableFrom(type))
                    return true;

            return false;
        }

        public static bool IsDataParallelArray2D(this Type type)
        {
            foreach (Type allowedType in _dataParallelArray2DBaseTypes)
                if (allowedType.IsAssignableFrom(type))
                    return true;

            return false;
        }
    }
}