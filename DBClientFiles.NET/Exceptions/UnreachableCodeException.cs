using System;

namespace DBClientFiles.NET.Exceptions
{
    [Serializable]
    public sealed class UnreachableCodeException : Exception
    {
        public UnreachableCodeException(string message) : base(message) { }
    }
}
