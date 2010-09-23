using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma.OpenCL
{
    public sealed class WaitFor: Brahma.WaitFor
    {
        internal WaitFor()
        { 
        }
        
        public static explicit operator WaitFor(string name)
        {
            return new WaitFor() { Name = name };
        }
    }
}