using System;
using System.Runtime.Serialization;

namespace DBClientFiles.NET.Exceptions
{
    /// <summary>
    /// This exceptions is thrown when an user attempts to modify one of the records of a file opened in read-only mode.
    /// </summary>
    [Serializable]
    public class ReadOnlyException : Exception
    {
        internal ReadOnlyException(string message) : base(message)
        {
        }

        internal ReadOnlyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
