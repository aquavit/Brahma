using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma.OpenCL
{
    public sealed class WaitFor: Brahma.WaitFor
    {
        public WaitFor(string name)
            : base(name)
        { 
        }

        public static explicit operator WaitFor(string name)
        {
            return new WaitFor(name);
        }
    }
}