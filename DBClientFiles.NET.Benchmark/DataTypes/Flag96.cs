using DBClientFiles.NET.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBClientFiles.NET.Benchmark.DataTypes
{
    public sealed class Flag96
    {
        [Cardinality(SizeConst = 3)]
        public uint[] Value { get; }
    }
}
