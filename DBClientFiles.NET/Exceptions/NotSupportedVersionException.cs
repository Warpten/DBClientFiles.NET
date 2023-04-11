using System;
using System.Runtime.Serialization;

namespace DBClientFiles.NET.Exceptions
{
    /// <summary>
    /// This exception is thrown when the version of the DBC file is not supported by the library.
    /// </summary>
    [Serializable]
    public class VersionNotSupportedException : Exception
    {
        internal VersionNotSupportedException(Signatures signature) : this($"File version {signature} ({(int)signature:X8} is not handled by this version of DBClientFiles.NET.")
        {
        }

        internal VersionNotSupportedException(string message) : base(message)
        {
        }

        internal VersionNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}