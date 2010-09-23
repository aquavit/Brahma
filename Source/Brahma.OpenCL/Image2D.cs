using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma.OpenCL
{
    public sealed class Image2D<T>: Brahma.Image2D<T> where T: struct, IImageFormat
    {
        public override IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}