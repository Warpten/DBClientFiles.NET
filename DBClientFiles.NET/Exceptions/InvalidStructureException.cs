using System;

namespace DBClientFiles.NET.Exceptions
{
    public sealed class InvalidStructureException : Exception
    {
        public InvalidStructureException(string message) : base(message) { }
    }
}
