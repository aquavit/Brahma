using System.Collections;
using System.Collections.Generic;

namespace Brahma
{
    public abstract class Buffer<T>: IEnumerable<T> where T: struct
    {
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}