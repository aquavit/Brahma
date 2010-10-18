using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class KernelUsableAttribute: Attribute
    {
        public KernelUsableAttribute()
        {
            CodeGenerator = string.Empty;
        }

        public string CodeGenerator
        {
            get;
            set;
        }
    }
}