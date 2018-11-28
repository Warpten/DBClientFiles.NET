using DBClientFiles.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBClientFiles.NET.Benchmark.DataTypes
{
    public sealed class LocString
    {
        [Cardinality(SizeConst = 16)]
        public string[] Values { get; set; }
        public uint Mask { get; set; }
    }
}
