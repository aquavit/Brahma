using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma
{
    public abstract class Image2D<T>: IEnumerable<T> where T: struct, IImageFormat
    {
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}