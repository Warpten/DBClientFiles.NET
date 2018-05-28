using DBClientFiles.NET.Attributes;

namespace DBClientFiles.NET.Data.WDB2
{
    public sealed class AreaGroupEntry
    {
        [Index]
        public uint Id { get; set; }
        [Cardinality(SizeConst = 6)]
        public uint[] AreaId { get; set; }
        public uint NextGroup { get; set; }
    }
}