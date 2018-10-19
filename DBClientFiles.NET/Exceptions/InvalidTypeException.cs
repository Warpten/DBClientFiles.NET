using System;

namespace DBClientFiles.NET.Exceptions
{
    [Serializable]
    public sealed class InvalidTypeException : Exception
    {
        public InvalidTypeException(string message) : base(message) { }
    }
}
