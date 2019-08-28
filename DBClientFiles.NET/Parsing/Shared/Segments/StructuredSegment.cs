using DBClientFiles.NET.Parsing.Versions;
using System;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.Shared.Segments
{
    internal abstract class StructuredSegment<T> : Segment
    {
        private T _value;

        public ref readonly T Value => ref _value;

        public abstract void Read(IBinaryStorageFile storageFile);

        protected Span<T> Span => MemoryMarshal.CreateSpan(ref _value, 1);
    }
}
