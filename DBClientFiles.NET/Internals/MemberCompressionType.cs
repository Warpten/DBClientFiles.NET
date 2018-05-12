using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClientFiles.NET.Internals
{
    internal enum MemberCompressionType
    {
        None,
        Bitpacked,
        CommonData,
        BitpackedPalletData,
        BitpackedPalletArrayData,
        RelationshipData
    }
}
