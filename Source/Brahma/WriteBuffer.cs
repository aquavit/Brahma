using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brahma
{
    public abstract class WriteBuffer<T> : Command<T> where T : struct
    {
        private readonly Buffer<T> _buffer;
        private readonly bool _blocking;
        private readonly int _offset;
        private readonly int _count;
        private readonly T[] _data;

        protected WriteBuffer(Buffer<T> buffer,
                              bool blocking, 
                              int offset, 
                              int count, 
                              T[] data)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (data == null)
                throw new ArgumentNullException("data");
            
            _buffer = buffer;
            _blocking = blocking;
            _offset = offset;
            _count = count;
            _data = data;
        }

        public Buffer<T> Buffer
        {
            get
            {
                return _buffer;
            }
        }

        public bool Blocking
        {
            get
            {
                return _blocking;
            }
        }

        public int Offset
        {
            get
            {
                return _offset;
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public T[] Data
        {
            get
            {
                return _data;
            }
        }
    }
}