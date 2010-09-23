using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenCL.Net;

namespace Brahma.OpenCL
{
    [Serializable]
    public class CLException : Exception
    {
        public CLException(Cl.ErrorCode error) : base(error.ToString())
        {
        }

        public CLException(Cl.ErrorCode error, Exception inner) : base(error.ToString(), inner)
        { 
        }

        protected CLException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) 
        {
        }
    }
}