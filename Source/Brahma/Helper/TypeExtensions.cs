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

        public static bool DerivesFrom(this Type type, Type baseType)
        {
            return baseType.IsAssignableFrom(type);
        }

        public static bool IsDataParallelArray(this Type type)
        {
            return _dataParallelArrayBaseType.IsAssignableFrom(type);
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