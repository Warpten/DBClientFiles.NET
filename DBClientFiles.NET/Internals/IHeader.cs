using DBClientFiles.NET.Internals.Segments;
using DBClientFiles.NET.Internals.Segments.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Internals
{
    public interface IHeader
    {
        Signatures Signature { get; }

        int RecordCount { get; }

        int FieldCount { get; }
    }

    internal interface IHeader<TValue> : IHeader where TValue : class, new()
    {
        Segment<TValue, StringTableReader<TValue>> StringTable { get; }
        Segment<TValue, OffsetMapReader<TValue>> OffsetMap { get; }
        Segment<TValue> CopyTable { get; }
        Segment<TValue> IndexTable { get; }
        Segment<TValue> CommonTable { get; }
        Segment<TValue> Pallet { get; }
    }
}
