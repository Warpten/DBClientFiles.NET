using System;

namespace DBClientFiles.NET.Exceptions
{
    public sealed class UnreachableCodeException : Exception
    {
        public UnreachableCodeException(string message) : base(message) { }
    }
}
