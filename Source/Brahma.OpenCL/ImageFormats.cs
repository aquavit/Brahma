using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Brahma.OpenCL
{
    [StructLayout(LayoutKind.Sequential)] public struct Snorm_Int8 : IImageComponentType { }
    [StructLayout(LayoutKind.Sequential)] public struct Snorm_Int16 : IImageComponentType { }
    [StructLayout(LayoutKind.Sequential)] public struct Unorm_Int8 : IImageComponentType { }
    [StructLayout(LayoutKind.Sequential)] public struct Unorm_Int16 : IImageComponentType { }
    [StructLayout(LayoutKind.Sequential)] public struct Unorm_Short565 : IImageComponentType { }
    [StructLayout(LayoutKind.Sequential)] public struct Unorm_Short555 : IImageComponentType { }
    [StructLayout(LayoutKind.Sequential)] public struct Unorm_Int101010 : IImageComponentType { }
    [StructLayout(LayoutKind.Sequential)] public struct Signed_Int8 : IImageComponentType { }
    [StructLayout(LayoutKind.Sequential)] public struct Signed_Int16 : IImageComponentType { }
    [StructLayout(LayoutKind.Sequential)] public struct Signed_Int32 : IImageComponentType { }
    [StructLayout(LayoutKind.Sequential)] public struct Unsigned_Int8 : IImageComponentType { }
    [StructLayout(LayoutKind.Sequential)] public struct Unsigned_Int16 : IImageComponentType { }
    [StructLayout(LayoutKind.Sequential)] public struct Unsigned_Int32 : IImageComponentType { }
    [StructLayout(LayoutKind.Sequential)] public struct HalfFloat : IImageComponentType { }
    [StructLayout(LayoutKind.Sequential)] public struct Float : IImageComponentType { }
    
    [StructLayout(LayoutKind.Sequential)] public struct R<T> : IImageFormat where T: struct, IImageComponentType
    {
        public T r;
    }

    [StructLayout(LayoutKind.Sequential)] public struct A<T> : IImageFormat where T: struct, IImageComponentType
    {
        public T a;
    }

    [StructLayout(LayoutKind.Sequential)] public struct RG<T> : IImageFormat where T: struct, IImageComponentType
    {
        public T r;
        public T g;
    }

    [StructLayout(LayoutKind.Sequential)] public struct RA<T> : IImageFormat where T: struct, IImageComponentType
    {
        public T r;
        public T a;
    }

    [StructLayout(LayoutKind.Sequential)] public struct RGB<T> : IImageFormat where T: struct, IImageComponentType
    {
        public T r;
        public T g;
        public T b;
    }

    [StructLayout(LayoutKind.Sequential)] public struct RGBA<T> : IImageFormat where T: struct, IImageComponentType
    {
        public T r;
        public T g;
        public T b;
        public T a;
    }

    [StructLayout(LayoutKind.Sequential)] public struct BGRA<T> : IImageFormat where T: struct, IImageComponentType
    {
        public T b;
        public T g;
        public T r;
        public T a;
    }

    [StructLayout(LayoutKind.Sequential)] public struct ARGB<T> : IImageFormat where T: struct, IImageComponentType
    {
        public T a;
        public T r;
        public T g;
        public T b;
    }

    [StructLayout(LayoutKind.Sequential)] public struct Intensity<T> : IImageFormat where T: struct, IImageComponentType
    {
        public T k;
    }

    [StructLayout(LayoutKind.Sequential)] public struct Luminance<T> : IImageFormat where T: struct, IImageComponentType
    {
        public T l;
    }
}