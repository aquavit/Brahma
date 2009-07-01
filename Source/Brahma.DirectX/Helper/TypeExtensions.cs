using System;

namespace Brahma.DirectX.Helper
{
    public static class TypeExtensions
    {
        private static readonly Type _outputType = typeof (output);
        
        public static bool IsOutput(this Type type)
        {
            return (type == _outputType);
        }
    }
}