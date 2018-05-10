using System;
using System.Runtime.Serialization;

namespace DBClientFiles.NET.Exceptions
{
    [Serializable]
    internal class NotSupportedVersionException : Exception
    {
        public NotSupportedVersionException()
        {
        }

        public NotSupportedVersionException(string message) : base(message)
        {
        }

        public NotSupportedVersionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotSupportedVersionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}