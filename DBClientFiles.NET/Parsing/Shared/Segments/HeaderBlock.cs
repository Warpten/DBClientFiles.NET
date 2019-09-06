using DBClientFiles.NET.Parsing.Versions;
using System.Runtime.InteropServices;

namespace DBClientFiles.NET.Parsing.Shared.Segments
{
    internal sealed class HeaderBlock<T> : StructuredSegment<T> where T : struct
    {
        public unsafe override void Read(IBinaryStorageFile storageFile)
        {
            var byteSpan = MemoryMarshal.AsBytes(Span);
            storageFile.DataStream.Read(byteSpan);
        }
    }
}
