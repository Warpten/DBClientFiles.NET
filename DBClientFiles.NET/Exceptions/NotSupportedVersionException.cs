using System;
using System.Runtime.Serialization;

namespace DBClientFiles.NET.Exceptions
{
    [Serializable]
    internal class VersionNotSupportedException : Exception
    {
        public VersionNotSupportedException(Signatures signature) : this($"File version {signature} ({(int)signature:X8} is not handled by this version of DBClientFiles.NET.")
        {
        }

        public VersionNotSupportedException(string message) : base(message)
        {
        }

        protected VersionNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}