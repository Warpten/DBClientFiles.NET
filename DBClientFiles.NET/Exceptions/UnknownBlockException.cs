using System;
using DBClientFiles.NET.Parsing.File.Segments;

namespace DBClientFiles.NET.Exceptions
{
    public class UnknownBlockException : Exception
    {
        public Signatures Signature { get; }

        internal UnknownBlockException(BlockIdentifier identifier, Signatures signature) : base($"Block {identifier} {(int) identifier:X8}) unknown for this {signature} file")
        {
            Signature = signature;
        }
    }
}
