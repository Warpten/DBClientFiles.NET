using System;
using System.Runtime.Serialization;

namespace DBClientFiles.NET.Collections.Generic.Exceptions
{
    [Serializable]
    public class ReadOnlyException : Exception
    {
        public ReadOnlyException(string message) : base(message)
        {
        }

        protected ReadOnlyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
