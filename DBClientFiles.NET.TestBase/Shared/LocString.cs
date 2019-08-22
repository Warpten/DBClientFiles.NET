using System;
using System.Collections.Generic;
using System.Text;
using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Types.Shared
{
    public sealed class LocString
    {
        [Cardinality(SizeConst = 16)]
        public string[] Values { get; set; }
        public uint Mask { get; set; }
    }
}
