using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Parsing.File.Segments
{
    [Flags]
    internal enum BlockIdentifier : uint
    {
        Header            = 0x00000001,
        StringBlock       = 0x00000002,
        CopyTable         = 0x00000004,
        OffsetMap         = 0x00000008,
        IndexTable        = 0x00000010,
        FieldInfo         = 0x00000020,
        FieldPackInfo     = 0x00000040,
        CommonDataTable   = 0x00000080,
        PalletTable       = 0x00000100,
        RelationShipTable = 0x00000200,


        Records           = 0xFFFFFFFF,
    }
}
