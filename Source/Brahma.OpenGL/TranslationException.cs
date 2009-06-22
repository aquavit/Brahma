using System;
using System.Runtime.Serialization;

namespace Brahma.OpenGL
{
    [Serializable]
    public class TranslationException: Exception
    {
        public TranslationException()
        {
        }

        public TranslationException(string message)
            : base(message)
        {
        }

        public TranslationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected TranslationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}